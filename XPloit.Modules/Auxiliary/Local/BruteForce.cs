using System;
using System.IO;
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
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Bruteforce Base"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override bool IsRemote { get { return false; } }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(ICheckPassword)); } }
        #endregion

        #region Properties
        public string WordList { get; set; }
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
                using (StreamReader reader = new StreamReader(WordList))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (check.CheckPassword(line))
                        {

                            if (!check.AllowMultipleOk) break;
                        }
                        else
                        {

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
    }
}