using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Interfaces;
using XPloit.Core.PayloadRequirements;

namespace XPloit.Modules.Auxiliary.Local
{
    public class BruteForce : Exploit
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
        public int Threads { get; set; }
        public int ReadBlock { get; set; }
        public string WordList { get; set; }
        public bool SaveState { get; set; }
        #endregion

        public override bool Run(ICommandLayer cmd)
        {
            ICheckPassword check = (ICheckPassword)this.Payload;
            if (!check.PreRun())
            {
                check.PostRun();
                return false;
            }
            try
            {
                int readBlock = ReadBlock;
                int threads = Threads;
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
                            if (Crack(cmd, toCrack, 0, index, threads, check, check.AllowMultipleOk))
                            {
                                index = 0;
                                if (!check.AllowMultipleOk) break;
                            }
                            if (save)
                            {
                                // Write position
                            }
                        }
                    }

                    if (index != 0)
                    {
                        // Sobras
                        if (Crack(cmd, toCrack, 0, index, threads, check, check.AllowMultipleOk))
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

            return false;
        }

        bool Crack(ICommandLayer cmd, string[] toCrack, int index, int length, int threads, ICheckPassword check, bool allowMultipleOk)
        {
            bool found = false;

            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;

            try
            {
                ParallelLoopResult res = Parallel.For(index, index + length, x =>
                    {
                        string w = toCrack[x];
                        if (check.CheckPassword(w))
                        {
                            found = true;

                            cmd.SetForeColor(ConsoleColor.DarkGray);
                            cmd.Write("Password Found! ");
                            cmd.SetForeColor(ConsoleColor.Green);
                            cmd.WriteLine(w);

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