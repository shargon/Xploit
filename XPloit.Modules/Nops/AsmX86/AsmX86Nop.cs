using XPloit.Core;

namespace Nops.AsmX86
{
    public class AsmX86Nop : Nop
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Asm X86 Nop"; } }

        public override bool Fill(byte[] buffer, int index, int length)
        {
            for (; index < length; index++)
                buffer[index] = 0x90;

            return true;
        }
    }
}