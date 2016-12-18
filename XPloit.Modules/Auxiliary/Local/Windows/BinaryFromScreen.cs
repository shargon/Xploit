using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Windows.Api;
using XPloit.Windows.Api.Native;

namespace Auxiliary.Local.Windows
{
    public class BinaryFromScreen : Module
    {
        public enum ECapture : byte
        {
            ScreenShoot = 0,
            Clipboard = 1,
        }

        public enum EImageMode : byte
        {
            BlackAndWhite = 0,
            RGB = 1
        }

        public enum EKey : byte
        {
            None = 0,
            Enter = 0x0D,
            Tab = 0x09,
            Escape = 0x1B,
            Left = 0x25,
            Right = 0x27,
        }

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Binary from screen"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/system.diagnostics.processstartinfo(v=vs.110).aspx") ,
                    new Reference(EReferenceType.URL,"http://referencesource.microsoft.com/#System/services/monitoring/system/diagnosticts/ProcessStartInfo.cs")
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Path")]
        public FileInfo FileName { get; set; }
        [ConfigurableProperty(Description = "Capture method")]
        public ECapture CaptureMethod { get; set; }
        [ConfigurableProperty(Description = "Beep")]
        public bool PlayBeep { get; set; }
        [ConfigurableProperty(Description = "PressKey")]
        public EKey PressKey { get; set; }
        [ConfigurableProperty(Description = "ImageMode")]
        public EImageMode ImageMode { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public BinaryFromScreen()
        {
            // Default variables
            FileName = null;
            PlayBeep = true;
            PressKey = EKey.Enter;
        }

        void ICapture()
        {
            ushort step = 0, max = ushort.MaxValue, widht = 0, height = 0;
            uint length = 0;

            using (MemoryStream stream = new MemoryStream())
            {
                WriteInfo("Waiting frame " + step);

                string extra = "";
                while (step < max)
                {
                    Bitmap capture = GetBitmap();
                    if (capture == null)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    byte[] search = SearchFor(capture, step, ref extra, ref max, ref length, ref widht, ref height);
                    capture.Dispose();

                    if (search == null || search.Length == 0) continue;

                    // Write
                    stream.Write(search, 0, search.Length);

                    WriteInfo("Get Frame " + step, StringHelper.Convert2KbWithBytes(search.Length), ConsoleColor.Cyan);
                    step++;
                    if (step != max) WriteInfo("Waiting frame " + step);

                    // Play beep
                    if (PlayBeep) Beep();

                    // Sendkey
                    switch (PressKey)
                    {
                        case EKey.None: break;
                        default:
                            {
                                InputSimulator input = new InputSimulator();

                                VirtualKeyCode key = (VirtualKeyCode)(int)PressKey;
                                input.Keyboard.KeyDown(key);
                                input.Keyboard.KeyUp(key);
                                break;
                            }
                    }
                }

                // Dump
                if (stream.Length > 0)
                {
                    byte[] data = stream.ToArray();

                    if (CompressHelper.IsGzipped(data, 0))
                    {
                        data = CompressHelper.Compress(data, 0, data.Length, false);

                        WriteInfo("Dump file [GZIP]", StringHelper.Convert2KbWithBytes(data.Length), ConsoleColor.Cyan);
                    }
                    else
                    {
                        WriteInfo("Dump file", StringHelper.Convert2KbWithBytes(data.Length), ConsoleColor.Cyan);
                    }

                    if (FileName.Exists) FileName.Delete();

                    using (FileStream fw = FileName.Create())
                        fw.Write(data, 0, data.Length);
                }
            }
        }

        class BitData
        {
            public class RGB
            {
                public byte R, G, B;

                public RGB(byte r, byte g, byte b)
                {
                    R = r;
                    G = g;
                    B = b;
                }

                public bool IsEqual(RGB b)
                {
                    return b != null && b.R == R && b.G == G && b.B == B;
                }

                public override string ToString()
                {
                    return R.ToString() + "," + G.ToString() + "," + B.ToString();
                }
            }

            public ushort Height;
            public ushort Width;
            public List<byte[]> DataBN;
            public List<RGB[]> DataRGB;

            public int IndexOfX;
            public int IndexOfY;
            public int CurrentY;
            public int CurrentX;

            public uint Length;
            EImageMode Mode;

            public string ExtraBN = "";
            public List<byte> ExtraRGB = new List<byte>();

            public BitData(ushort w, ushort h, EImageMode mode)
            {
                Mode = mode;
                Width = w;
                Height = h;

                switch (mode)
                {
                    case EImageMode.BlackAndWhite:
                        {
                            DataBN = new List<byte[]>();
                            for (int y = 0; y < h; y++) DataBN.Add(new byte[w]);
                            break;
                        }
                    case EImageMode.RGB:
                        {
                            DataRGB = new List<RGB[]>();
                            for (int y = 0; y < h; y++) DataRGB.Add(new RGB[w]);
                            break;
                        }
                }
            }

            bool ReadBitBN(out char c)
            {
                if (CurrentX >= IndexOfX + Width)
                {
                    CurrentX = IndexOfX;
                    CurrentY++;
                }
                if (CurrentY >= IndexOfY + Height)
                {
                    c = '0';
                    return false;
                }

                c = DataBN[CurrentY][CurrentX] == 0 ? '0' : '1';
                CurrentX++;

                return true;
            }

            public bool ReadByte(out byte read)
            {
                switch (Mode)
                {
                    case EImageMode.BlackAndWhite:
                        {
                            char c;
                            while (ExtraBN.Length < 8)
                            {
                                if (!ReadBitBN(out c))
                                {
                                    read = 0;
                                    return false;
                                }

                                ExtraBN += c.ToString();
                            }

                            read = Convert.ToByte(ExtraBN, 2);
                            ExtraBN = "";
                            return true;
                        }
                    case EImageMode.RGB:
                        {
                            if (ExtraRGB.Count > 0)
                            {
                                read = ExtraRGB[0];
                                ExtraRGB.RemoveAt(0);
                                return true;
                            }
                            else
                            {
                                if (CurrentX >= IndexOfX + Width)
                                {
                                    CurrentX = IndexOfX;
                                    CurrentY++;
                                }
                                if (CurrentY >= IndexOfY + Height)
                                {
                                    read = 0;
                                    return false;
                                }

                                RGB r = DataRGB[CurrentY][CurrentX];
                                CurrentX++;

                                read = r.R;
                                ExtraRGB.AddRange(new byte[] { r.G, r.B });
                                return true;
                            }
                        }
                }

                read = 0;
                return false;
            }

            public RGB ParseRGB(int oldRed, int oldGreen, int oldBlue) { return new RGB((byte)oldRed, (byte)oldGreen, (byte)oldBlue); }
            public byte ParseBN(int oldRed, int oldGreen, int oldBlue)
            {
                // Bit
                if (oldBlue == 255 && oldGreen == 255 && oldRed == 255)
                    return 1;

                return 0;
            }
        }
        static bool SearchBytes(BitData.RGB[] haystack, BitData.RGB[] needle, out int ix)
        {
            int len = needle.Length;
            int limit = haystack.Length - len;
            for (int i = 0; i <= limit; i++)
            {
                BitData.RGB a = haystack[i];
                if (a.IsEqual(needle[0]))
                {
                    BitData.RGB b = haystack[i + 1];
                    if (b.R == needle[1].R)
                    {
                        ix = i;
                        return true;
                    }
                }
            }

            ix = -1;
            return false;
        }
        static bool SearchBytes(byte[] haystack, byte[] needle, out int ix)
        {
            int len = needle.Length;
            int limit = haystack.Length - len;
            for (int i = 0; i <= limit; i++)
            {
                int k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len)
                {
                    ix = i;
                    return true;
                }
            }
            ix = -1;
            return false;
        }
        byte[] SearchFor(Bitmap capture, ushort step, ref string extra, ref ushort max, ref uint length, ref ushort width, ref ushort height)
        {
            if (capture == null) return null;

            // BLACK&WHITE
            if (ImageMode == EImageMode.BlackAndWhite)
            {
                /*    using (Graphics gr = Graphics.FromImage(capture))
                    {
                        float[][] gray_matrix = new float[][]
                        {
                            new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                            new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                            new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                            new float[] { 0,      0,      0,      1, 0 },
                            new float[] { 0,      0,      0,      0, 1 }
                        };

                        ImageAttributes ia = new ImageAttributes();
                        ia.SetColorMatrix(new ColorMatrix(gray_matrix));
                        ia.SetThreshold(0.8F); // Change this threshold as needed
                        Rectangle rc = new Rectangle(0, 0, capture.Width, capture.Height);
                        gr.DrawImage(capture, rc, 0, 0, capture.Width, capture.Height, GraphicsUnit.Pixel, ia);
                    }*/
            }

            // BYTE ARRAY
            BitData array = new BitData((ushort)capture.Width, (ushort)capture.Height, ImageMode);
            array.ExtraBN = extra;

            using (MemoryStream ms = new MemoryStream())
            {
                unsafe
                {
                    BitmapData bitmapData = capture.LockBits(new Rectangle(0, 0, capture.Width, capture.Height), ImageLockMode.ReadWrite, capture.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(capture.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < heightInPixels; y++)
                    {
                        byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            int oldBlue = currentLine[x];
                            int oldGreen = currentLine[x + 1];
                            int oldRed = currentLine[x + 2];

                            switch (ImageMode)
                            {
                                case EImageMode.BlackAndWhite:
                                    array.DataBN[y][x / bytesPerPixel] = array.ParseBN(oldRed, oldGreen, oldBlue);
                                    break;
                                case EImageMode.RGB:
                                    array.DataRGB[y][x / bytesPerPixel] = array.ParseRGB(oldRed, oldGreen, oldBlue);
                                    break;
                            }
                        }
                    }
                    capture.UnlockBits(bitmapData);
                }

                // SEARCH HEADER
                byte[] HEADER = new byte[]
                {
                    0xB1, 0xA5, // MAGIC NUMBER
                    0x00, 0x00  // STEP
                };

                // FIRST FRAME
                //      {HEADER+STEP}+MAX+WIDTH+HEIGHT+SIZE  => 14*8 = 112
                // NEXT
                //      {HEADER+STEP}                        =>  4*8 =  32
                // 10110001101001010000000000000000
                byte[] bnHeader = BitConverter.GetBytes(step);
                Array.Copy(bnHeader, 0, HEADER, 2, bnHeader.Length);

                BitData.RGB[] rgbHeader = null;
                switch (ImageMode)
                {
                    case EImageMode.BlackAndWhite:
                        {
                            string sheader = "";
                            foreach (byte b in HEADER)
                                sheader += Convert.ToString(b, 2).PadLeft(8, '0');

                            bnHeader = new byte[sheader.Length];
                            for (int x = 0; x < bnHeader.Length; x++)
                                bnHeader[x] = sheader[x] == '1' ? (byte)1 : (byte)0;

                            break;
                        }
                    case EImageMode.RGB:
                        {
                            rgbHeader = new BitData.RGB[] { new BitData.RGB(HEADER[0], HEADER[1], HEADER[2]), new BitData.RGB(HEADER[3], 0, 0) };
                            break;
                        }
                }
                // Search header or use the BytesPeerLine
                for (int y = 0; y < array.Height; y++)
                {
                    int index;
                    if ((ImageMode == EImageMode.BlackAndWhite && SearchBytes(array.DataBN[y], bnHeader, out index)) ||
                        (ImageMode == EImageMode.RGB && SearchBytes(array.DataRGB[y], rgbHeader, out index)))
                    {
                        array.IndexOfY = y;
                        array.CurrentY = y;

                        array.IndexOfX = array.CurrentX = index;
                        if (ImageMode == EImageMode.BlackAndWhite)
                            array.CurrentX += bnHeader.Length;
                        else
                        {
                            BitData.RGB[] row = array.DataRGB[y];
                            BitData.RGB cur = row[array.IndexOfX + 1];
                            array.ExtraRGB.AddRange(new byte[] { cur.G, cur.B });
                            array.CurrentX+=2;
                        }

                        // First frame
                        if (step == 0)
                        {
                            byte[] b = new byte[2];
                            if (!array.ReadByte(out b[0]) || !array.ReadByte(out b[1]))
                            {
                                extra = array.ExtraBN;
                                return ms.ToArray();
                            }

                            max = BitConverter.ToUInt16(b, 0);

                            if (!array.ReadByte(out b[0]) || !array.ReadByte(out b[1]))
                            {
                                extra = array.ExtraBN;
                                return ms.ToArray();
                            }

                            array.Width = width = BitConverter.ToUInt16(b, 0);

                            if (!array.ReadByte(out b[0]) || !array.ReadByte(out b[1]))
                            {
                                extra = array.ExtraBN;
                                return ms.ToArray();
                            }

                            array.Height = height = BitConverter.ToUInt16(b, 0);

                            b = new byte[4];
                            if (!array.ReadByte(out b[0]) || !array.ReadByte(out b[1]) || !array.ReadByte(out b[2]) || !array.ReadByte(out b[3]))
                            {
                                extra = array.ExtraBN;
                                return ms.ToArray();
                            }

                            array.Length = length = BitConverter.ToUInt32(b, 0);
                        }
                        else
                        {
                            array.Length = length;
                            array.Width = width;
                            array.Height = height;
                        }

                        break;
                    }
                }

                if (array.Length == 0)
                {
                    extra = array.ExtraBN;
                    return ms.ToArray();
                }

                byte r;
                while (array.Length > 0)
                {
                    if (!array.ReadByte(out r)) break;

                    ms.WriteByte(r);
                    array.Length--;
                }

                length = array.Length;
                extra = array.ExtraBN;
                return ms.ToArray();
            }
        }

        Bitmap GetBitmap()
        {
            switch (CaptureMethod)
            {
                case ECapture.Clipboard:
                    {
                        while (!Clipboard.ContainsImage())
                            Thread.Sleep(50);

                        Bitmap ret = (Bitmap)Clipboard.GetImage();
                        Clipboard.Clear();
                        return ret;
                    }
                case ECapture.ScreenShoot:
                    {
                        Rectangle bounds = Screen.GetBounds(Point.Empty);
                        Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                        }
                        return bitmap;
                    }
            }
            return null;
        }

        public override bool Run()
        {
            Thread th = new Thread(ICapture);
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
            CreateJob(th);

            return true;
        }
    }
}