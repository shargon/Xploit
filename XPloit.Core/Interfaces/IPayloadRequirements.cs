namespace XPloit.Core.Interfaces
{
    public interface IPayloadRequirements
    {
        /// <summary>
        /// Retursn if its allowed
        /// </summary>
        /// <param name="payload">Payload</param>
        /// <returns>Return true if payload its allowed</returns>
        bool IsAllowed(Payload payload);
    }
}