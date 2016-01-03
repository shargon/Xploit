namespace XPloit.Core.Interfaces
{
    public interface INopRequirements
    {
        /// <summary>
        /// Retursn if its allowed
        /// </summary>
        /// <param name="nop">Nop</param>
        /// <returns>Return true if nop its allowed</returns>
        bool IsAllowed(Nop nop);
    }
}