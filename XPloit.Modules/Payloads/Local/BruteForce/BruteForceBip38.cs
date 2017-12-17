using Auxiliary.Local;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.BruteForce
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Crack Bip38")]
    public class BruteForceBip38 : Payload, WordListBruteForce.ICheckPassword
    {
        #region Configure
        public override Reference[] References
        {
            get
            {
                return new Reference[] { new Reference(EReferenceType.URL, "https://github.com/MetacoSA/NBitcoin/blob/master/NBitcoin/BIP38/BitcoinEncryptedSecret.cs") };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Address")]
        public string Address { get; set; }
        [ConfigurableProperty(Description = "PrivateKey")]
        public string PrivateKey { get; set; }
        #endregion

        public bool CheckPassword(string password)
        {

            return false;
        }

        public bool PreRun()
        {
            //Key key = new Key(); //Create a new key
            //BitcoinSecret secret = key.GetBitcoinSecret(Network.Main);
            //Console.WriteLine(secret); //Will print the key in base58 check format
            //BitcoinEncryptedSecret encrypted = secret.Encrypt("This is my secret password");
            //Console.WriteLine(encrypted); //Will print the encrypted key in base58 check format
            //key = encrypted.GetKey("This is my secret password"); //Get the same key as before

            return true;
        }
        public void PostRun()
        {
            
        }
    }
}