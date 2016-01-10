using System;
using System.Collections.Generic;
using System.Text;
using XPloit.Core.Enums;
using XPloit.Core.Extensions;

namespace XPloit.Core.Helpers
{
    public class AsmHelper
    {
        /// <summary>
        /// Generate the asm necessary for put this char in the stack
        /// </summary>
        /// <param name="data">Data</param>
        public static byte[] StringToAsmX86(byte[] data)
        {
            int dl = data.Length;
            List<byte> ls = new List<byte>();

            for (int x = 0; x < dl; )
            {
                byte[] chars = new byte[4];

                Array.Copy(data, x, chars, 0, Math.Min(4, dl - x));

                x += 4;
                ls.InsertRange(0, chars);
                ls.Insert(0, 0x68);
            }

            return ls.ToArray();
        }
        /// <summary>
        /// Generate the asm necessary for put this char in the stack
        /// </summary>
        /// <param name="data">Data</param>
        public static byte[] StringToAsmX86(string data)
        {
            return StringToAsmX86(Encoding.ASCII.GetBytes(data));
        }

        public static byte[] GetExitMethod(EExitFunc method, EPlatform platform, EArquitecture arq)
        {
            switch (platform)
            {
                case EPlatform.Windows:
                    {
                        switch (arq)
                        {
                            case EArquitecture.x86:
                                {
                                    string function = "";

                                    switch (method)
                                    {
                                        case EExitFunc.Process: function = "ExitProcess"; break;
                                        case EExitFunc.Thread: function = "ExitThread"; break;
                                        default: return null;
                                    }
                                    /*
                                        ;; eax = GetProcAddress(ebx, "ExitProcess")
                                        push edi
                                        push 01737365h
                                        dec byte ptr [esp + 3h]
                                        push 'corP'
                                        push 'tixE'
                                        push esp
                                        push ebx
                                        call esi
                                     */
                                    byte[] shell1 = AsmHelper.StringToAsmX86(function);
                                    //0x68,0x50,0x72,0x6f,0x63,0x68,0x45,0x78, 0x69,0x74,
                                    /*
                                     ;; ExitProcess(NULL);
                                    push edi
                                    call eax
                                     */
                                    byte[] shell2 = new byte[]
                                    {
                                        0x54,0x53,0xff,0xd6,0x57,0xff,0xd0
                                    };

                                    return shell1.Concat(shell2);
                                }
                        }

                        break;
                    }
            }


            return null;
        }
    }
}