using System;
using System.IO;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.BruteForce;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = @"Generate a wordList
-------------------

To configure the 'ConfigFile' it's so easy. It is a 'ini file' like this:

[C1]                             <- Section name
abcdefghijklmnopqrstuvwxyz       <- Values

[N]                              <- Section name
0123456789                       <- Values

[Words]                          <- Section name
dog                              <- Value
cat                              <- Value
home                             <- Value

They are set that be predefined:
    
[0-F]
0123456789ABCDEF

[0-f]
0123456789abcdef

[0-9]
0123456789
    
[a-z]
abcdefghijklmnopqrstuvwxyz

[A-Z]
ABCDEFGHIJKLMNOPQRSTUVWXYZ

[a-Z]
abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ

[¡!]
 !""#$%&'()*+,-./:;<=>?@[\]^_`{|}~

[¡s!]
!#$%&()*+,-./:;<=>?@_|~

Configure the 'ConfigPicture'
-----------------------------

It's so easy too, you can watch this sample:

[C1][Word=w0rd][C2] <- Use set 'C1' then set 'Word' (as word set) and then set 'C2'

One set can be used like 'char' or like 'word', 'char' it's by default
To use a set like 'char':

[C1=c]       <- Same as [C1=char]
[C1=C]       <- Same as [C1=CHAR]
[C1=char]    <- To use set like 'char' (case-sensitive)
[C1=CHAR]    <- To use set like 'char', but this set changes into upper and lower (non case-sensitive)

A 'char' set can be used to replace of vocals 'a' to '4' , 'e' to '3' , 'i' to '1' and 'o' to '0'

[C1=ch4r]    <- To use set like 'char' (to replace vocals)
[C1=CH4R]    <- To use set like 'char', but this set changes into upper and lower (to replace vocals)

When a set it's a 'word' Set, can be used like this

[MySet=w]    <- Same as [MySet=word]
[MySet=W]    <- Same as [MySet=WORD]
[MySet=word] <- To use set like 'word' set (case-sensitive)
[MySet=WORD] <- To use set like 'word' set (non case-sensitive)
[MySet=Word] <- To use set like 'word' set (The first char won't be case-sensitive)

[MySet=w0rd] <- To use set like 'word' set (case-sensitive & to replace vocals)
[MySet=W0RD] <- To use set like 'word' set (non case-sensitive & to replace vocals)
[MySet=W0rd] <- To use set like 'word' set (The first char won't be case-sensitive & to replace vocals)

You can do optional set starting with '!', for example:

[C1][MyWords=!W0rds][C1]

It means:
    - Firstly use set C1
    - Then, you use a optional word set 'MyWords', the first char must be non case-sensitive, replacing vocals
    - And finish with C1
")]
    public class WordListGenerator : Module
    {
        #region Properties
        [ConfigurableProperty(Description = "File where is write the word list")]
        public FileInfo FileDest { get; set; }
        [RequireExists]
        [ConfigurableProperty(Optional = true, Description = "File where its store the Sets")]
        public FileInfo ConfigFile { get; set; }
        [ConfigurableProperty(Description = "Picture to use, sample: [C1][C2][C3][0-F][a-Z]")]
        public string ConfigPicture { get; set; }
        [ConfigurableProperty(Description = "Bytes for split the file")]
        public long SplitInBytes { get; set; }
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

        string GetNextFile(string file, ref int step)
        {
            string file2 = System.IO.Path.GetDirectoryName(file) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(file);
            string ext = System.IO.Path.GetExtension(file);
            file2 = file2 + "." + step.ToString() + ext;
            step++;
            return file2;
        }

        public override bool Run()
        {
            BruteForce b = new BruteForce();
            if (!b.CompilePicture(ConfigFile == null ? null : ConfigFile.FullName, ConfigPicture)) return false;

            WriteInfo("Sets loaded", b.Sets.ToString(), b.Sets <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
            WriteInfo("Steps loaded", b.Steps.ToString(), b.Steps <= 0 ? ConsoleColor.Red : ConsoleColor.Green);

            int step = 0;
            long split = SplitInBytes;
            string file = FileDest.FullName;

            if (split > 0)
                WriteInfo("Split files in", StringHelper.Convert2Kb(split), ConsoleColor.Green);

            ulong gen = b.Count();
            ulong va = 0;
            StartProgress(gen);

            FileStream sw = new FileStream(split > 0 ? GetNextFile(file, ref step) : file, FileMode.Append, FileAccess.Write);
            long ps = sw.Position;

            foreach (string password in b.Run())
            {
                byte[] data = Encoding.UTF8.GetBytes(password + "\n");
                sw.Write(data, 0, data.Length);
                ps += data.Length;

                va++;
                WriteProgress(va);

                if (split > 0 && ps >= split)
                {
                    sw.Dispose();

                    sw = new FileStream(GetNextFile(file, ref step), FileMode.Append, FileAccess.Write);
                    ps = sw.Position;
                }
            }

            sw.Dispose();

            EndProgress();

            WriteInfo("Generated file successful ", gen.ToString(), ConsoleColor.Green);
            return true;
        }
    }
}