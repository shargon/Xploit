using System;
using System.IO;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;

namespace Auxiliary.Local.Fuzzing
{
    public class Fuzzer : Module
    {
        public interface IFuzzerPayload
        {
            void Run(string ascii);
        }

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Generic Fuzzer"; } }
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "http://unlogic.co.uk/2014/07/16/exploit-pattern-generator/") }; }
        }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(IFuzzerPayload)); } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "From Length")]
        public int From { get; set; }
        [ConfigurableProperty(Required = true, Description = "To Length")]
        public int To { get; set; }
        [ConfigurableProperty(Required = true, Description = "To Length")]
        public int Step { get; set; }
        [FileRequireExists()]
        [ConfigurableProperty(Required = true, Description = "File for replay")]
        public FileInfo File { get; set; }
        [ConfigurableProperty(Required = true, Description = "Injection string for replace the pattern")]
        public string InjectionPoint { get; set; }
        #endregion

        public Fuzzer()
        {
            From = 1;
            To = 5000;
            Step = 1;
            InjectionPoint = "{InjectHere}";
        }

        public override bool Run()
        {
            IFuzzerPayload payload = (IFuzzerPayload)Payload;

            WriteInfo("Loading file ...");
            string file = System.IO.File.ReadAllText(File.FullName, Encoding.ASCII);
            WriteInfo("File loaded", StringHelper.Convert2KbWithBytes(file.Length), System.ConsoleColor.Green);

            if (!file.Contains(InjectionPoint))
            {
                WriteError("Not found injection point, please add '" + InjectionPoint + "' to the file");
                return false;
            }

            for (int max = To, st = Math.Max(Step, 1); From <= max; From += st)
            {
                WriteInfo("Checking ", From.ToString(), ConsoleColor.Green);

                string fuz = PatternHelper.Create(From);
                fuz = file.Replace(InjectionPoint, fuz);

                try
                {
                    payload.Run(fuz);
                    CopyPropertiesToActiveModule("From");
                }
                catch (Exception e)
                {
                    WriteInfo("Good news :) , payload send fail!");
                    WriteError(e.Message);
                    return true;
                }
            }

            return true;
        }
    }
}