using XPloit.Core.Charset;
using XPloit.Helpers;

namespace XPloit.Core.Helpers
{
    public class EvasionHelper
    {
        private static string RandomVariable(string OBFUSCATION)
        {
            switch (OBFUSCATION)
            {
                case "lalpha":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetLowercase("lalpha"));
                    break;
                case "ualpha":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetUppercase("ualpha"));
                    break;
                case "mixalpha":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetMixCase("mixalpha"));
                    break;
                case "sv-lalpha":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetSvLowercase("sv-lalpha"));
                    break;
                case "sv-ualpha":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetSvMixcase("sv-ualpha"));
                    break;
                case "sv-mixalpha":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetSvMixcase("sv-mixalpha"));
                    break;
                case "lcyrillic":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Cyrillic.AlphabetLowercase("lcyrillic"));
                    break;
                case "ucyrillic":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Cyrillic.AlphabetUppercase("ucyrillic"));
                    break;
                case "mixcyrillic":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Cyrillic.AlphabettMix("mixcyrillic"));
                    break;
                case "mix":
                    OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Mix.MixMergeTwo(Latin.AlphabetSvMixcase("sv-mixalpha"), Cyrillic.AlphabettMix("mixcyrillic")));
                    break;
                default:
                    int select = IntegerHelper.RandomInterger(10);

                    if (select == 1)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetLowercase("lalpha"));
                    }
                    else if (select == 2)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetUppercase("ualpha"));
                    }
                    else if (select == 3)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetMixCase("mixalpha"));
                    }
                    else if (select == 4)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetSvLowercase("sv-lalpha"));
                    }
                    else if (select == 5)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetSvMixcase("sv-ualpha"));
                    }
                    else if (select == 6)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Latin.AlphabetSvMixcase("sv-mixalpha"));
                    }
                    else if (select == 7)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Cyrillic.AlphabetLowercase("lcyrillic"));
                    }
                    else if (select == 8)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Cyrillic.AlphabetUppercase("ucyrillic"));
                    }
                    else if (select == 9)
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Cyrillic.AlphabettMix("mixcyrillic"));
                    }
                    else
                    {
                        OBFUSCATION = StringHelper.RandomUniqString(IntegerHelper.RandomInterger(20), Mix.MixMergeTwo(Latin.AlphabetSvMixcase("sv-mixalpha"), Cyrillic.AlphabettMix("mixcyrillic")));
                    }
                    break;
            }

            return OBFUSCATION;
        }

        public static string Variable(string variable, string OBFUSCATION)
        {
            return (OBFUSCATION == "none") ? variable : RandomVariable(OBFUSCATION);
        }
    }
}