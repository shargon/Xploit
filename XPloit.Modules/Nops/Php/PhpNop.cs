using XPloit.Core;

namespace XPloit.Modules.Nops.Php
{
    public class PhpNop : Nop
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "PHP Nop"; } }
        public override string Name { get { return "PhpNop"; } }
        public override string Path { get { return "Nops/Php"; } }

        public override byte[] Get(int size)
        {
            byte[] ret = new byte[size];

            for (int x = 0; x < size; x++)
                ret[x] = (byte)' ';

            return ret;
        }
    }
}