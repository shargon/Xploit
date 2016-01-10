using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Payloads
{
    public class SpecifyPlatformRequired : IPayloadRequirements
    {
        Module _Module;

        public bool IsAllowed(Payload obj)
        {
            Target t = _Module.Target;
            if (t == null) return false;

            if (t.Arquitecture == EArquitecture.None) return false;
            if (t.Platform == EPlatform.None) return false;

            return t.Arquitecture == obj.Arquitecture && t.Platform == obj.Platform;
        }

        public bool ItsRequired()
        {
            return _Module != null && _Module.Target != null &&
                _Module.Target.Arquitecture != EArquitecture.None && _Module.Target.Platform != EPlatform.None;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="module">Module</param>
        public SpecifyPlatformRequired(Module module)
        {
            _Module = module;
        }
    }
}