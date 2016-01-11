using System;
using System.Runtime.InteropServices;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace Auxiliary.Local
{
    public class TestPayload : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "NFC Restore system"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override Target[] Targets
        {
            get
            {
                return new Target[]
                {
                    new Target(EPlatform.Windows ,EArquitecture.x86, "Windows - x86")
                };
            }
        }
        #endregion

        class WinX86
        {
            const uint PAGE_EXECUTE_READWRITE = 0x40;
            const uint MEM_COMMIT = 0x1000;

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

            delegate int IntReturner();

            public /*unsafe */void RunAsm(params byte[] asm)
            {
                IntPtr buf = VirtualAlloc(IntPtr.Zero, (uint)asm.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                Marshal.Copy(asm, 0, buf, asm.Length);

                IntReturner ptr = (IntReturner)Marshal.GetDelegateForFunctionPointer(buf, typeof(IntReturner));

                //fixed (byte* newBuffer = &asm[0])
                //{
                ptr();
                //del(newBuffer);
                //}
                //Console.WriteLine(ptr());
            }
        }

        [IntrusiveCheck]
        public override ECheck Check()
        {
            return base.Check();
        }
        public override bool Run()
        {
            Target t = this.Target;

            switch (t.Platform)
            {
                case EPlatform.Windows:
                    {
                        switch (t.Arquitecture)
                        {
                            case EArquitecture.x86:
                            case EArquitecture.x64:
                                {
                                    WinX86 c = new WinX86();
                                    c.RunAsm(Payload.GetValue(t));
                                    break;
                                }
                        }

                        break;
                    }
            }

            return false;
        }
    }
}