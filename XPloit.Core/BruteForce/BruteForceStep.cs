using System.Collections.Generic;

namespace XPloit.Core.BruteForce
{
    public class BruteForceStep
    {
        BruteForceAllowedChars[] ret = null;

        public BruteForceStep() { }
        public static BruteForceStep FromChar(string[] input, bool lowerAndUpper, bool mix)
        {
            string sinput = string.Join("", input);
            BruteForceStep c = new BruteForceStep();
            c.ret = new BruteForceAllowedChars[] { new BruteForceAllowedChars(lowerAndUpper, mix, sinput) };
            return c;
        }
        public static BruteForceStep[] FromWord(bool appendEmpty, string[] input, bool lowerAndUpper, bool upperOnlyFirst, bool mix)
        {
            List<BruteForceStep> ls = new List<BruteForceStep>();
            if (appendEmpty) ls.Add(new BruteForceStep());

            foreach (string sinput in input)
            {
                BruteForceStep c = new BruteForceStep();
                c.ret = BruteForceAllowedChars.SplitWordMixed(sinput, lowerAndUpper, upperOnlyFirst, mix);
                ls.Add(c);
            }
            return ls.ToArray();
        }
        public BruteForceAllowedChars[] Get() { return ret; }
    }
}