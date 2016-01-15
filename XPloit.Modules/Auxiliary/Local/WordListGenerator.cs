using System;
using System.IO;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.BruteForce;
using XPloit.Core.Enums;

namespace Auxiliary.Local
{
    public class WordListGenerator : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Generate a wordList"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "File where is write the word list")]
        public FileInfo FileDest { get; set; }
        [FileRequireExists]
        [ConfigurableProperty(Description = "File where its store the Sets")]
        public FileInfo ConfigFile { get; set; }
        [ConfigurableProperty(Required = true, Description = "Picture to use, sample: [C1][C1][C1][C1][C1]")]
        public string ConfigPicture { get; set; }
        #endregion

        public override ECheck Check()
        {
            BruteForce b = new BruteForce();
            if (!b.CompilePicture(ConfigFile == null ? null : ConfigFile.FullName, ConfigPicture)) return ECheck.Error;

            WriteInfo("Sets loaded", b.Sets.ToString(), b.Sets <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
            WriteInfo("Steps loaded", b.Steps.ToString(), b.Steps <= 0 ? ConsoleColor.Red : ConsoleColor.Green);

            WriteInfo("Count ", b.Count().ToString(), ConsoleColor.Green);
            return ECheck.Ok;
        }

        public override bool Run()
        {
            BruteForce b = new BruteForce();
            if (!b.CompilePicture(ConfigFile == null ? null : ConfigFile.FullName, ConfigPicture)) return false;

            WriteInfo("Sets loaded", b.Sets.ToString(), b.Sets <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
            WriteInfo("Steps loaded", b.Steps.ToString(), b.Steps <= 0 ? ConsoleColor.Red : ConsoleColor.Green);

            ulong gen = b.Count();
            ulong va = 0;
            StartProgress(gen);

            using (FileStream sw = new FileStream(FileDest.FullName, FileMode.Append, FileAccess.Write))
                foreach (string password in b.Run())
                {
                    byte[] data = Encoding.UTF8.GetBytes(password + "\n");
                    sw.Write(data, 0, data.Length);
                    va++;
                    WriteProgress(va);
                }

            EndProgress();

            WriteInfo("Generated file successful ", gen.ToString(), ConsoleColor.Green);
            return true;
        }
    }
}