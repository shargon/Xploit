using XPloit.Core;

namespace XPloit.Modules.Nops.Php
{
    public class PhpNop : Nop
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "PHP Nop"; } }
        public override string Name { get { return "PhpNop"; } }
        public override string Path { get { return "Nops/Php"; } }

        public override bool Fill(byte[] buffer, int index, int length)
        {
            for (; index < length; index++)
                buffer[index] = (byte)' ';

            return true;
        }
    }
}