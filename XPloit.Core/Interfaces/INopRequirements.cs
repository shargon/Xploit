namespace XPloit.Core.Interfaces
{
    public interface INopRequirements
    {
        /// <summary>
        /// Retursn if nop its allowed
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if its allowed</returns>
        bool IsAllowed(Nop obj);
    }
}