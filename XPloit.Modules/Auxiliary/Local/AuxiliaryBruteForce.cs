using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliaryBruteForce : Module
    {
        public interface ICheckPassword
        {
            bool PreRun();
            void PostRun();
            bool AllowMultipleOk { get; }
            bool CheckPassword(string password);
        }

        #region Configure
        public override string Name { get { return "WordListBruteForce"; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Local Brute force by wordlist"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override bool IsRemote { get { return false; } }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(ICheckPassword)); } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Number of threads")]
        public int Threads { get; set; }
        [ConfigurableProperty(Required = true, Description = "Read lines number per block")]
        public int ReadBlock { get; set; }
        [ConfigurableProperty(Required = true, Description = "Wordlist file")]
        public string WordList { get; set; }
        [ConfigurableProperty(Required = true, Description = "Save state for next call")]
        public bool SaveState { get; set; }
        #endregion

        public AuxiliaryBruteForce()
        {
            ReadBlock = 1000;
            Threads = 5;
        }

        public override bool Run()
        {
            ICheckPassword check = (ICheckPassword)this.Payload;
            if (!check.PreRun())
            {
                check.PostRun();
                return false;
            }

            try
            {
                int readBlock = Math.Max(1, ReadBlock);
                int threads = Math.Max(1, Threads);
                bool save = SaveState;

                using (StreamReader reader = new StreamReader(WordList))
                {
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
                            if (Crack(toCrack, 0, index, threads, check, check.AllowMultipleOk))
                            {
                                index = 0;

                                if (!check.AllowMultipleOk) break;
                            }
                            else index = 0;
                            if (save)
                            {
                                // Write position
                            }
                        }
                    }

                    if (index != 0)
                    {
                        // Sobras
                        if (Crack(toCrack, 0, index, threads, check, check.AllowMultipleOk))
                        {
                            index = 0;
                        }
                        if (save)
                        {
                            // End
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                check.PostRun();
            }

            return true;
        }

        bool Crack(string[] toCrack, int index, int length, int threads, ICheckPassword check, bool allowMultipleOk)
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

                            WriteInfo("Password Found! ", w, ConsoleColor.Green);

                            cts.Cancel();
                            return;
                        }

                        po.CancellationToken.ThrowIfCancellationRequested();
                    });
            }
            catch
            {

            }
            finally
            {
                cts.Dispose();
            }
            return found;
        }
    }
}