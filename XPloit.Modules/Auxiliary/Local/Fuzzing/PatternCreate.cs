using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Attributes;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Fuzzing
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Generate pattern string for exploit development")]
    public class PatternCreate : Module
    {
        #region Configure
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "http://unlogic.co.uk/2014/07/16/exploit-pattern-generator/") }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Length of pattern")]
        public int Length { get; set; }
        #endregion

        public PatternCreate() { Length =5000; }

        public override bool Run()
        {
            if (Length > PatternHelper.MaxPatternUnique)
                WriteInfo("Max unique values are " + PatternHelper.MaxPatternUnique.ToString() + " you can need specify Start in search mode");

            WriteInfo(PatternHelper.Create(Length));
            return true;
        }
    }
}