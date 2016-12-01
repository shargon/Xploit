<?php

$mode='bn';
//$mode='rgb';
$w=800;
$h=$w;
$file='D:\\test.jpg';
$dpath='D:\\';

$entra=0;
switch($mode)
{
  // Low Quality
  case 'bn':
    if(($w*$h)%8!=0) die('Require Width*Height%8==0');
    $entra=($w*$h)/8; 
    break;
  // FullQuality
  case 'rgb': $entra=($w*$h)*3; break;
}

$file=file_get_contents($file);

// Compress
$gzdata = gzencode($file, 9);
//if(strlen($gzdata)<strlen($file)) $file=$gzdata;

// Max
$szFile=strlen($file);
$max=0;
$check=$szFile;
while($check>0)
  {
  // Header
  $check-=$entra-($max==0?14:4);  // Cabereras
  $max++;
  }

echo 'Max images '.$max." for ".$szFile." bytes\n";

// Parse to Binary
function PrepareDataBN(&$data)
{
  $ret='';
  
  for($i=0,$m=strlen($data);$i<$m;$i++)
  {
    // Hex
    $value = unpack('H*', substr($data,$i,1));
    $value=base_convert($value[1], 16, 2);
    $ret.=str_pad($value, 8, 0, STR_PAD_LEFT);
  }

  return $ret;
}

$sizeOrg=$szFile;
// Prepare Buff
switch($mode)
{
  case 'bn': { $file=PrepareDataBN($file); break; }
}
$szFile=strlen($file);

$image = imagecreatetruecolor($w,$h);
$black = imagecolorallocate($image,0,0,0);
$white = imagecolorallocate($image,255,255,255);

function GetColorBN(&$send,&$black,&$white,&$ix)
{
  $ix++;
  //echo(substr($send,$ix-1,1));
  return substr($send,$ix-1,1)=='0'?$black:$white;
}

function GetColorRGB(&$image,&$send,&$ix,$l)
{
  $r=$l>=1?ord(substr($send,$ix,1)):0;
  $g=$l>=2?ord(substr($send,$ix+1,1)):0;
  $b=$l>=3?ord(substr($send,$ix+2,1)):0;
  
  $ix+=min($l-$ix,3);
  return imagecolorallocate($image,$r,$g,$b);
}

$ixFile=0;
for($i=0;$i<$max;$i++)
{
  // Images
  imagefill($image,0,0,$black);
  
  // Header
  $header='B1A5'.unpack('H*', pack('S',$i))[1]; // Header+Step
  if($i==0)
    $header.=
        unpack('H*', pack('S',$max))[1].    // m
        unpack('H*', pack('S',$w))[1].      // w
        unpack('H*', pack('S',$h))[1].      // h
        unpack('H*', pack('I',$sizeOrg))[1]; // s
        
  //die($header);
  $header=hex2bin($header);
  //die(''.strlen($header));
   
  switch($mode)
  {
    case 'bn': { $header=PrepareDataBN($header); break; }
    case 'rgb': 
    { 
      if($i==0) // Mod 14
        { $header.=substr($file,$ixFile,1); $ixFile++; }
      else      // Mod 2
        { $header.=substr($file,$ixFile,2); $ixFile+=2; }
      break; 
    }
  }
  //echo (strlen($header));
  //die($header);

  $ixHeader=0;
  // Draw
 for($y=0;$y<$h;$y++) for($x=0;$x<$w;$x++)
      {
      $color=$black;
      
      $sz=0; $ix=0;
      $IsHeader=false;
      
      if($header!='') 
      {
        $IsHeader=true;
        $ix=$ixHeader;
        $value=$header;
        $sz=strlen($header);
      }
      else
      {
        $ix=$ixFile;
        $sz=$szFile;
        $value=$file;
      }
      
      if($ix<$sz)
        {
          switch($mode)
            {
              case 'bn' : { $color=GetColorBN($value,$black,$white,$ix);break; }
              case 'rgb': { $color=GetColorRGB($image,$value,$ix,$sz);  break; }
            }
          if($IsHeader) 
          {
            // End of header
            $ixHeader=$ix;
            if($ixHeader>=strlen($header)) 
              $header='';
          }
          else $ixFile=$ix;
        }    
      
      imagesetpixel($image,$x,$y,$color);
      }
  
  echo 'Creating image: '.$i." ";
  imagepng($image,$dpath.'image.'.$i.'.png');
  echo "[OK]\n";
}
imagedestroy($image);

$extra=substr($file,$ixFile);
//if($extra!='') echo "** Tail: ".$extra;