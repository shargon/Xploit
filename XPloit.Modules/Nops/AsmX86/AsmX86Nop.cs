using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace Nops.AsmX86
{
    public class AsmX86Nop : INopAsm
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Asm X86 Nop"; } }
        public override Target[] AllowedTargets
        {
            get
            {
                return new Target[]
                {
                    new Target(EPlatform.Windows, EArquitecture.x86),
                    new Target(EPlatform.Linux, EArquitecture.x86),
                    new Target(EPlatform.MAC, EArquitecture.x86)
                };
            }
        }

        public override bool Fill(byte[] buffer, int index, int length)
        {
            for (; index < length; index++)
                buffer[index] = 0x90;

            return true;
        }
    }
}