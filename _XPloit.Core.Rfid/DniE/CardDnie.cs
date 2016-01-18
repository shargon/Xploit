using Xploit.Core.Rfid.Enums;
using Xploit.Core.Rfid.Interfaces;

namespace XPloit.Core.Rfid.DniE
{
    public class CardDnie : ICard
    {
        /// <summary>
        /// Nombre completo
        /// </summary>
        public string CompleteName { get; set; }
        /// <summary>
        /// Primer apellido
        /// </summary>
        public string Surname1 { get; set; }
        /// <summary>
        /// Segundo apellido
        /// </summary>
        public string Surname2
        {
            get
            {
                string s = CompleteName;
                string f = Surname1;
                string n = Name;

                string t = s.Substring(f.Length + 1, s.Length - f.Length - 1);
                t = t.Substring(0, t.Length - (n.Length + 2));

                return t;
            }
        }
        /// <summary>
        /// Nombre
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id de la tarjeta
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// pais
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Tipo de tarjeta
        /// </summary>
        public ECardType Type { get { return ECardType.DNIe; } }
    }
}