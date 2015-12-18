namespace XPloit.Core.Sockets.Enums
{
    /// <summary>
    /// Especifica el tipo de desconexión que se realiza
    /// </summary>
    public enum EDissconnectReason
    {
        /// <summary>
        /// Desconectado sin razon aparente
        /// </summary>
        None = 0,
        /// <summary>
        /// Desconectado a petición
        /// </summary>
        Requested = 1,
        /// <summary>
        /// Desconectado por Error
        /// </summary>
        Error = 2,
        /// <summary>
        /// Desconectado por Baneo
        /// </summary>
        Banned = 3,
        /// <summary>
        /// Desconectado por que supera el máximo de clientes
        /// </summary>
        MaxAllowed = 4,
        /// <summary>
        /// Desconectado por TimeOut
        /// </summary>
        TimeOut = 5
    };
}