namespace XPloit.Core.Interfaces
{
    public interface IEncoderRequirements
    {
        /// <summary>
        /// Retursn if encoder its allowed
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if its allowed</returns>
        bool IsAllowed(Nop obj);
    }
}