namespace XPloit.Core.Interfaces
{
    public interface IPayloadRequirements
    {
        /// <summary>
        /// Retursn if payload its allowed
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if its allowed</returns>
        bool IsAllowed(Payload obj);
        /// <summary>
        /// Return true if requirePayload
        /// </summary>
        bool ItsRequired();
    }
}