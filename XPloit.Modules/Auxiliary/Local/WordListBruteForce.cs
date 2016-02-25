using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Interfaces;
using XPloit.Core.Streams;
using XPloit.Core.Requirements.Payloads;
using XPloit.Core.Helpers;
using System.IO.Compression;

namespace Auxiliary.Local
{
    public class WordListBruteForce : Module
    {
        public interface ICheckPassword
        {
            bool PreRun();
            void PostRun();
            bool CheckPassword(string password);
        }

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Local Brute force by wordlist"; } }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(ICheckPassword)); } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Number of threads")]
        public int Threads { get; set; }
        [ConfigurableProperty(Description = "Read lines number per block")]
        public int ReadBlock { get; set; }
        [FileRequireExists]
        [ConfigurableProperty(Description = "Wordlist file")]
        public FileInfo WordListFile { get; set; }
        [ConfigurableProperty(Description = "Line in the WordListFile for start")]
        public int StartAtLine { get; set; }
        [ConfigurableProperty(Description = "Save the last line checked")]
        public bool SaveState { get; set; }
        #endregion

        public WordListBruteForce()
        {
            ReadBlock = 1000;
            Threads = 5;
            StartAtLine = 0;
            SaveState = true;
        }

        public override bool Run()
        {
            ICheckPassword check = (ICheckPassword)this.Payload;
            if (!check.PreRun())
            {
                check.PostRun();
                return false;
            }

            FileStream stream = null;
            bool found = false;
            string tempFile = null;

            try
            {
                if (!WordListFile.Exists) return false;

                int readBlock = Math.Max(1, ReadBlock);
                int threads = Math.Max(1, Threads);

                stream = File.OpenRead(WordListFile.FullName);

                if (FileDetectionHelper.DetectFileFormat(stream, true, true) == FileDetectionHelper.EFileFormat.Gzip)
                {
                    WriteInfo("Decompress gzip wordlist");
                    WriteInfo("Compressed size", StringHelper.Convert2KbWithBytes(stream.Length), ConsoleColor.Green);

                    stream.Close();
                    stream.Dispose();

                    using (FileStream streamR = File.OpenRead(WordListFile.FullName))
                    using (GZipStream gz = new GZipStream(streamR, CompressionMode.Decompress))
                    {
                        // decompress
                        tempFile = Path.GetTempFileName();
                        stream = File.Open(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        gz.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    WriteInfo("Decompressed size", StringHelper.Convert2KbWithBytes(stream.Length), ConsoleColor.Green);
                }

                using (StreamLineReader reader = new StreamLineReader(stream))
                {
                    WriteInfo("Start counting file");
                    int hay = reader.GetCount(StartAtLine);

                    WriteInfo("Total lines", hay.ToString(), ConsoleColor.Green);
                    StartProgress(Math.Max(0, hay - StartAtLine));

                    string[] toCrack = new string[readBlock];
                    int index = 0;

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Fill
                        toCrack[index] = line;
                        index++;

                        if (index == readBlock)
                        {
                            // Crack
                            if (Crack(toCrack, 0, index, threads, check))
                            {
                                index = 0;
                                found = true;
                                break;
                            }
                            else index = 0;

                            if (SaveState)
                            {
                                StartAtLine = reader.CurrentLine;
                                CopyPropertiesToActiveModule("StartAtLine");
                            }
                            WriteProgress(reader.CurrentLine);
                        }
                    }

                    if (index != 0)
                    {
                        // Sobras
                        if (Crack(toCrack, 0, index, threads, check))
                        {
                            index = 0;
                            found = true;
                        }

                        if (SaveState)
                        {
                            StartAtLine = reader.CurrentLine;
                            CopyPropertiesToActiveModule("StartAtLine");
                        }
                        WriteProgress(reader.CurrentLine);
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
                if (tempFile != null)
                {
                    File.Delete(tempFile);
                }
                EndProgress();
                check.PostRun();
            }

            return found;
        }

        bool Crack(string[] toCrack, int index, int length, int threads, ICheckPassword check)
        {
            if (threads <= 1)
            {
                for (int x = index, max = index + length; x < max; x++)
                {
                    string w = toCrack[x];
                    if (check.CheckPassword(w))
                    {
                        EndProgress();
                        WriteInfo("Password Found! ", w, ConsoleColor.Green);
                        Beep();

                        return true;
                    }
                }

                return false;
            }
            else
            {
                bool found = false;
                CancellationTokenSource cts = new CancellationTokenSource();

                try
                {
                    // Use ParallelOptions instance to store the CancellationToken
                    ParallelOptions po = new ParallelOptions();
                    po.CancellationToken = cts.Token;
                    po.MaxDegreeOfParallelism = threads;

                    ParallelLoopResult res = Parallel.For(index, index + length, x =>
                        {
                            string w = toCrack[x];
                            if (check.CheckPassword(w))
                            {
                                found = true;

                                EndProgress();
                                WriteInfo("Password Found! ", w, ConsoleColor.Green);
                                Beep();

                                cts.Cancel();
                                return;
                            }

                            po.CancellationToken.ThrowIfCancellationRequested();
                        });
                }
                catch { }
                finally { cts.Dispose(); }

                return found;
            }
        }
    }
}