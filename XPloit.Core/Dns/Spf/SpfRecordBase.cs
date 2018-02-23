using System.Collections.Generic;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Base class of a SPF or SenderID record
	/// </summary>
	public class SpfRecordBase
	{
		/// <summary>
		///   Modifiers and mechanisms of a record
		/// </summary>
		public List<SpfTerm> Terms { get; set; }

		protected static bool TryParseTerms(string[] terms, out List<SpfTerm> parsedTerms)
		{
			parsedTerms = new List<SpfTerm>(terms.Length - 1);

			for (int i = 1; i < terms.Length; i++)
			{
				SpfTerm term;
				if (SpfTerm.TryParse(terms[i], out term))
				{
					parsedTerms.Add(term);
				}
				else
				{
					parsedTerms = null;
					return false;
				}
			}

			return true;
		}
	}
}