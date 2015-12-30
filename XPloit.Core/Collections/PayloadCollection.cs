using System.Collections.Generic;
using XPloit.Core.Interfaces;
namespace XPloit.Core.Collections
{
    public class PayloadCollection : IModuleCollection<Payload>
    {
        static PayloadCollection _Current = null;

        /// <summary>
        /// Current Payloads
        /// </summary>
        public static PayloadCollection Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new PayloadCollection();
                    _Current.Load();
                }
                return _Current;
            }
        }

        public Payload[] GetPayloadAvailables(IPayloadRequirements req)
        {
            if (req == null) return new Payload[] { };
            List<Payload> ls = new List<Payload>();
            foreach (Payload p in PayloadCollection.Current)
            {
                if (!req.IsAllowedPayload(p)) continue;
                ls.Add(p);
            }
            return ls.ToArray();
        }
    }
}