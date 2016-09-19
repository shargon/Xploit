using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace Nops.Php
{
    public class PhpNop : INopPhp
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "PHP Nop"; } }
        public override Target[] AllowedTargets
        {
            get
            {
                return new Target[] { new Target(EPlatform.Http, EArquitecture.All) };
            }
        }

        public override bool Fill(byte[] buffer, int index, int length)
        {
            for (; index < length; index++)
                buffer[index] = (byte)' ';

            return true;
        }
    }
}