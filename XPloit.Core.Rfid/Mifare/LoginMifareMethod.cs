namespace Xploit.Core.Rfid.Mifare
{
    public class LoginMifareMethod
    {
        /// <summary>
        /// Tipo de la clave
        /// </summary>
        public ConfigMifareRead.EKeyType KeyType { get; set; }
        /// <summary>
        /// Número de la clave
        /// </summary>
        public ConfigMifareRead.EKeyNum KeyNum { get; set; }
    }
}