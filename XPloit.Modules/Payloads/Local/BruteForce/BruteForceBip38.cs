using Auxiliary.Local;
using NBitcoin;
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

        BitcoinEncryptedSecret _Check;

        public bool CheckPassword(string password)
        {
            try
            {
                return _Check.GetSecret(password).PubKey.ToString() == Address;
            }
            catch
            {
                return false;
            }
        }

        public bool PreRun()
        {
            _Check = BitcoinEncryptedSecret.Create(Address, Network.Main);
            return true;
        }
        public void PostRun() { }
    }
}