using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;

namespace Auxiliary.Local.Fuzzing
{
    public class PatternCreate : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Generate pattern string for exploit development"; } }
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "http://unlogic.co.uk/2014/07/16/exploit-pattern-generator/") }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Length of pattern")]
        public int Length { get; set; }
        #endregion

        public PatternCreate() { Length = 1000; }

        public override bool Run()
        {
            if (Length > PatternHelper.MaxPatternUnique)
                WriteInfo("Max unique values are " + PatternHelper.MaxPatternUnique.ToString() + " you can need specify Start in search mode");

            WriteInfo(PatternHelper.Create(Length));
            return true;
        }
    }
}