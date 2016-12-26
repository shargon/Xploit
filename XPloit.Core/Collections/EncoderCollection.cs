using System.Collections.Generic;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Collections
{
    public class EncoderCollection : IModuleCollection<Encoder>
    {
        static EncoderCollection _Current = null;

        /// <summary>
        /// Current Exploits
        /// </summary>
        public static EncoderCollection Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new EncoderCollection();
                    _Current.Load();
                }
                return _Current;
            }
        }

        public IEnumerable<ModuleHeader<Encoder>> GetAvailables(IEncoderRequirements req)
        {
            foreach (ModuleHeader<Encoder> p in Current)
            {
                if (!req.IsAllowed(p)) continue;
                yield return p;
            }
        }
    }
}