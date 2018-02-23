﻿namespace XPloit.Core.Interfaces
{
    public interface IEncoderRequirements
    {
        /// <summary>
        /// Return if encoder its allowed
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if its allowed</returns>
        bool IsAllowed(ModuleHeader<Encoder> obj);
    }
}