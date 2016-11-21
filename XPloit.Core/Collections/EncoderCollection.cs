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


        public Encoder[] GetAvailables(IEncoderRequirements req)
        {
            if (req == null) return new Encoder[] { };
            List<Encoder> ls = new List<Encoder>();
            foreach (ModuleHeader<Encoder> p in Current)
            {
                if (!req.IsAllowed(p)) continue;

                ls.Add(p.Current);
            }
            return ls.ToArray();
        }
    }
}