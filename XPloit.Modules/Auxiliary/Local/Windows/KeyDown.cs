using System;
using System.IO;
using System.Text;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Helpers.Attributes;
using XPloit.Windows.Api;
using XPloit.Windows.Api.Native;

namespace Auxiliary.Local.Windows
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Key down a textfile")]
    public class KeyDown : Module
    {
        public enum EFilter : byte
        {
            None = 0,
            Base64 = 1,
            Hex = 2,
        }

        #region Configure
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/system.diagnostics.processstartinfo(v=vs.110).aspx") ,
                    new Reference(EReferenceType.URL,"http://referencesource.microsoft.com/#System/services/monitoring/system/diagnosticts/ProcessStartInfo.cs")
                };
            }
        }
        #endregion

        #region Properties
        [RequireExists]
        [ConfigurableProperty(Description = "Path")]
        public FileInfo FileName { get; set; }
        [ConfigurableProperty(Description = "Wait for send")]
        public TimeSpan WaitSend { get; set; }
        [ConfigurableProperty(Description = "Wait between steps")]
        public TimeSpan WaitBetweenSteps { get; set; }
        [ConfigurableProperty(Description = "Step char count")]
        public int StepCount { get; set; }
        [ConfigurableProperty(Description = "Convert filter")]
        public EFilter Filter { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyDown()
        {
            // Default variables
            FileName = null;
            StepCount = 100;
            WaitBetweenSteps = TimeSpan.FromMilliseconds(100);
            WaitSend = TimeSpan.FromSeconds(5);
        }

        public override bool Run()
        {
            byte[] data = File.ReadAllBytes(FileName.FullName);

            string text;
            switch (Filter)
            {
                case EFilter.None: text = Encoding.UTF8.GetString(data); break;
                case EFilter.Base64: text = Convert.ToBase64String(data); break;
                case EFilter.Hex:
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in data)
                            sb.Append(b.ToString("x2"));

                        text = sb.ToString();
                        break;
                    }
                default: return false;
            }

            Thread.Sleep(WaitSend);

            InputSimulator input = new InputSimulator();

            for (int x = 0; x < text.Length; x++)
            {
                if (x % StepCount == 0) Thread.Sleep(WaitBetweenSteps);

                char l = text[x];

                if (l == '\r')
                    continue;

                bool shift = false;
                short vk = NativeMethods.VkKeyScan(l);
                if (((vk >> 8) & 1) == 1)  // presionamos la mayúscula
                    shift = true;

                VirtualKeyCode key = (VirtualKeyCode)(vk & 0xFF);

                if (shift) input.Keyboard.KeyDown(VirtualKeyCode.SHIFT);

                input.Keyboard.KeyDown(key);
                input.Keyboard.KeyUp(key);

                if (shift) input.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            }

            return true;
        }
    }
}