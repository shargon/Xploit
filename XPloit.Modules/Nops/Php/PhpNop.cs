using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.Attributes;

namespace Nops.Php
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "PHP Nop")]
    public class PhpNop : INopPhp
    {
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