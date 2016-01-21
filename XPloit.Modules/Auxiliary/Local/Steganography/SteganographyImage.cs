using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;

namespace Auxiliary.Local.Steganography
{
    public class SteganographyImage : Module, AESHelper.IAESConfig
    {
        public enum EMode { Read, Write };

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Steganography by Image generator/parser (in PNG)\nHave two modes:\n - Write: Destroy original message file\n - Read : Read the image and write the secret file in LocalFileWrite"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] 
                { 
                    new Reference(EReferenceType.URL, "https://en.wikipedia.org/wiki/Steganography") ,
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Mode")]
        public EMode Mode { get; set; }
        [ConfigurableProperty(Required = true, Description = "File for read")]
        public FileInfo LocalFileWrite { get; set; }
        [FileRequireExists]
        [ConfigurableProperty(Required = true, Description = "File for write")]
        public FileInfo LocalFileRead { get; set; }
        [ConfigurableProperty(Description = "IV for AES encryption")]
        public string AesIV { get; set; }
        [ConfigurableProperty(Description = "Password for AES encryption")]
        public string AesPassword { get; set; }
        [ConfigurableProperty(Description = "Iterations for AES encryption")]
        public int AesIterations { get; set; }
        [ConfigurableProperty(Description = "KeyLength for AES encryption")]
        public AESHelper.EKeyLength AesKeyLength { get; set; }
        [ConfigurableProperty(Description = "RGBSalt for AES encryption")]
        public string AesRGBSalt { get; set; }
        #endregion

        public SteganographyImage()
        {
            Mode = EMode.Write;
            AesIterations = 1000;
            AesKeyLength = AESHelper.EKeyLength.Length256;
            AesRGBSalt = null;
        }

        public override bool Run()
        {
            if (!LocalFileRead.Exists) return false;

            AESHelper aes = AESHelper.Create(this);
            if (aes != null) WriteInfo("Using AES Encryption");
            else WriteError("Read/Write in RawMode (without any Encryption)");

            using (Bitmap img = (Bitmap)Image.FromFile(LocalFileRead.FullName))
            {
                switch (Mode)
                {
                    case EMode.Write:
                        {
                            if (!LocalFileWrite.Exists)
                            {
                                WriteError("In this mode LocalFileWrite must exists, and will be replaced with the result image");
                                return false;
                            }
                            WriteInfo("Start reading file");

                            byte[] data = File.ReadAllBytes(LocalFileWrite.FullName);
                            if (aes != null) data = aes.Encrypt(data);

                            byte[] header = BitConverterHelper.GetBytesInt32(data.Length);
                            int totalSize = data.Length + header.Length;

                            int av = CalculateMaxLength(img.Width, img.Height);

                            WriteInfo("Bytes to encode", StringHelper.Convert2KbWithBytes(totalSize), ConsoleColor.Green);
                            WriteInfo("Bytes available", StringHelper.Convert2KbWithBytes(av), ConsoleColor.DarkCyan);

                            if (totalSize <= av) WriteInfo("Its viable!");
                            else
                            {
                                WriteError("You need a image more larger or a message more shorter ... sorry :(");
                                return false;
                            }

                            // crear array binario
                            StringBuilder binary = new StringBuilder();

                            for (int x = 0, m = header.Length; x < m; x++)
                                binary.Append(Convert.ToString(header[x], 2).PadLeft(8, '0'));

                            for (int x = 0, m = data.Length; x < m; x++)
                                binary.Append(Convert.ToString(data[x], 2).PadLeft(8, '0'));

                            char[] sb = binary.ToString().ToCharArray();
                            int sbl = sb.Length;
                            binary.Clear();

                            // Cadena binaria creada
                            int width = img.Width;
                            int height = img.Height;
                            bool toLower = true;

                            WriteInfo("Start writing image");
                            StartProgress(width * height);

                            Random rd = new Random(Environment.TickCount);
                            byte r, g, b;
                            int index = 0, current = 0;
                            for (int x = 0; x < width; x++)
                                for (int y = 0; y < height; y++)
                                {
                                    r = GetBinary(rd, sb, ref index, sbl);
                                    g = GetBinary(rd, sb, ref index, sbl);
                                    b = GetBinary(rd, sb, ref index, sbl);

                                    Color clr = img.GetPixel(x, y);

                                    clr = SetColor(clr, r, g, b, toLower);
                                    img.SetPixel(x, y, clr);

                                    current++;
                                    WriteProgress(current);
                                }

                            EndProgress();

                            WriteInfo("Writing output");
                            img.Save(LocalFileWrite.FullName, ImageFormat.Png);
                            break;
                        }
                    case EMode.Read:
                        {
                            WriteInfo("Start reading image");

                            int width = img.Width;
                            int height = img.Height;
                            int av = CalculateMaxLength(width, height);

                            StartProgress(width * height);

                            byte[] data = null;
                            byte[] header = new byte[4];

                            string binary = "";

                            int dataLength = 0;
                            int dataIndex = 0;
                            int headerReaded = 0;
                            byte b;
                            int current = 0;
                            for (int x = 0; x < width; x++)
                                for (int y = 0; y < height; y++)
                                {
                                    Color clr = img.GetPixel(x, y);

                                    if (Append(ref binary, out b, clr.R % 2 == 0, clr.G % 2 == 0, clr.B % 2 == 0))
                                    {
                                        if (headerReaded < 4)
                                        {
                                            header[headerReaded] = b;
                                            headerReaded++;

                                            if (headerReaded == 4)
                                            {
                                                dataLength = BitConverterHelper.ToInt32(header, 0);

                                                if (dataLength > av)
                                                {
                                                    EndProgress();

                                                    WriteInfo("Image maybe contains", StringHelper.Convert2KbWithBytes(dataLength), ConsoleColor.Green);
                                                    WriteError("Max bytes available " + StringHelper.Convert2KbWithBytes(av));
                                                    return false;
                                                }

                                                data = new byte[dataLength];
                                            }
                                        }
                                        else
                                        {
                                            data[dataIndex] = b;
                                            dataIndex++;
                                            if (dataIndex >= dataLength)
                                            {
                                                x = width + 1;
                                                break;
                                            }
                                        }
                                    }

                                    current++;
                                    WriteProgress(current);
                                }

                            EndProgress();

                            if (aes != null)
                            {
                                WriteInfo("Start decrypting file", StringHelper.Convert2KbWithBytes(data.Length), ConsoleColor.Green);
                                data = aes.Decrypt(data);
                            }

                            if (data == null)
                            {
                                WriteInfo("Error decrypting file");
                                return false;
                            }

                            WriteInfo("Writing output", StringHelper.Convert2KbWithBytes(data.Length), ConsoleColor.Green);
                            File.WriteAllBytes(LocalFileWrite.FullName, data);

                            break;
                        }
                }
            }

            return true;
        }
        public override ECheck Check()
        {
            try
            {
                if (!LocalFileRead.Exists)
                {
                    WriteError("LocalFileRead must exists");
                    return ECheck.Error;
                }

                if (Mode == EMode.Write && !LocalFileWrite.Exists)
                {
                    WriteError("In this mode LocalFileWrite must exists, and will be replaced with the result image");
                    return ECheck.Error;
                }

                try
                {
                    Bitmap img = (Bitmap)Image.FromFile(LocalFileRead.FullName);

                    AESHelper aes = AESHelper.Create(this);

                    if (Mode == EMode.Write)
                    {
                        byte[] data = File.ReadAllBytes(LocalFileWrite.FullName);
                        if (aes != null) data = aes.Encrypt(data);

                        int totalSize = data.Length + 4;
                        int av = CalculateMaxLength(img.Width, img.Height);

                        WriteInfo("Bytes to encode", StringHelper.Convert2KbWithBytes(totalSize), ConsoleColor.Green);
                        WriteInfo("Bytes available", StringHelper.Convert2KbWithBytes(av), ConsoleColor.DarkCyan);

                        if (totalSize <= av)
                        {
                            if (totalSize != av)
                                WriteInfo("You can write more!", StringHelper.Convert2KbWithBytes(av - totalSize), ConsoleColor.DarkCyan);
                            return ECheck.Ok;
                        }
                        else
                        {
                            WriteError("You need a image more larger or a message more shorter ... sorry :(");
                            return ECheck.Error;
                        }
                    }

                    img.Dispose();
                }
                catch
                {
                    WriteError("LocalFileRead must be a valid image");
                    return ECheck.Error;
                }

                return ECheck.Ok;
            }
            catch { return ECheck.Error; }
        }

        bool Append(ref string binary, out byte b, params bool[] par)
        {
            foreach (bool p in par)
                binary += p ? "1" : "0";

            if (binary.Length >= 8)
            {
                b = Convert.ToByte(binary.Substring(0, 8), 2);
                binary = binary.Remove(0, 8);

                return true;
            }

            b = 0;
            return false;
        }
        byte GetBinary(Random r, char[] sb, ref int index, int l)
        {
            if (index >= l)
            {
                // More than data, could be random
                if (r.Next(0, 100) < 50)
                    return (byte)0;
                else
                    return (byte)1;
            }

            index++;
            return (byte)(sb[index - 1] == '1' ? 1 : 0);
        }
        Color SetColor(Color clr, byte r, byte g, byte b, bool toLower)
        {
            return Color.FromArgb(clr.A,
                ToColor(clr.R, r == 1, toLower),
                ToColor(clr.G, g == 1, toLower),
                ToColor(clr.B, b == 1, toLower));
        }
        byte ToColor(byte current, bool par, bool toLower)
        {
            bool isPar = current % 2 == 0;
            if (isPar == par) return current;

            // generar par/impar

            if (par)
            {
                if (toLower)
                    return (byte)(current - 1);
                else
                {
                    if (current == 255)
                        return (byte)(current - 1);

                    return (byte)(current + 1);
                }
            }
            else
            {
                if (toLower)
                {
                    if (current == 0)
                        return (byte)(current + 1);

                    return (byte)(current - 1);
                }
                else
                    return (byte)(current + 1);
            }
        }
        int CalculateMaxLength(int width, int height)
        {
            int binary = (width * height) * 3; // 101 -> 3 binary per pixel
            return (binary / 8); // -> 8 digitos 1 byte
        }
    }
}
