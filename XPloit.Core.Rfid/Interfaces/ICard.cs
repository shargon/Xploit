using Xploit.Core.Rfid.Enums;

namespace Xploit.Core.Rfid.Interfaces
{
    public interface ICard
    {
        /// <summary>
        /// Identificación
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Tipo de tarjeta
        /// </summary>
        ECardType Type { get; }
        /// <summary>
        /// ATR
        /// </summary>
        byte[] Atr { get; }
    }
}