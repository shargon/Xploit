using System.IO;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local
{
    public class FileToHex : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Create a Hex string from file"; } }
        #endregion

        public enum Emode
        {
            C_Like_String
        }

        #region Properties
        [ConfigurableProperty(Description = "Mode")]
        public Emode Mode { get; set; }
        [RequireExists()]
        [ConfigurableProperty(Description = "File for read")]
        public FileInfo Input { get; set; }
        [ConfigurableProperty(Description = "File for write")]
        public FileInfo Output { get; set; }

        [ConfigurableProperty(Description = "Bytes peer line")]
        public int Split { get; set; }
        [ConfigurableProperty(Optional = true, Description = "Prefix (use {bytes} for byte length, {line} for line number)")]
        public string Prefix { get; set; }
        [ConfigurableProperty(Optional = true, Description = "Suffix (use {bytes} for byte length, {line} for line number)")]
        public string Suffix { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public FileToHex()
        {
            // Default variables
            Prefix = "char data_{line}[] = \"";
            Suffix = "\";";
            Mode = Emode.C_Like_String;
            Split = 8000;
        }

        public override bool Run()
        {
            byte[] data = File.ReadAllBytes(Input.FullName);

            StringBuilder sb = new StringBuilder();
            int index = 0;
            int linex = 0;
            foreach (byte b in data)
            {
                if (index == 0 || index % Split == 0)
                {
                    if (sb.Length > 0)
                        sb.AppendLine(Suffix
                            .Replace("{line}", linex.ToString())
                            .Replace("{bytes}", index.ToString()));

                    sb.Append(Prefix
                        .Replace("{line}", linex.ToString())
                        .Replace("{bytes}", index.ToString()));
                    linex++;
                    index = 0;
                }

                switch (Mode)
                {
                    case Emode.C_Like_String:
                        {
                            sb.Append("\\x" + b.ToString("x2"));
                            break;
                        }
                }
                index++;
            }

            if (sb.Length > 0)
                sb.AppendLine(Suffix
                    .Replace("{line}", linex.ToString())
                    .Replace("{bytes}", index.ToString()));

            using (FileStream fs = Output.OpenWrite())
            {
                data = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Write(data, 0, data.Length);
            }

            return true;
        }
    }
}