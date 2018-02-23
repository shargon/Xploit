using System;
using System.Collections.Generic;
using System.Text;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   <para>Parsed instance of the textual representation of a SPF record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4408">RFC 4408</see>
	///   </para>
	/// </summary>
	public class SpfRecord : SpfRecordBase
	{
		/// <summary>
		///   Returns the textual representation of a SPF record
		/// </summary>
		/// <returns> Textual representation </returns>
		public override string ToString()
		{
			StringBuilder res = new StringBuilder();
			res.Append("v=spf1");

			if ((Terms != null) && (Terms.Count > 0))
			{
				foreach (SpfTerm term in Terms)
				{
					SpfModifier modifier = term as SpfModifier;
					if ((modifier == null) || (modifier.Type != SpfModifierType.Unknown))
					{
						res.Append(" ");
						res.Append(term.ToString());
					}
				}
			}

			return res.ToString();
		}

		/// <summary>
		///   Checks, whether a given string starts with a correct SPF prefix
		/// </summary>
		/// <param name="s"> Textual representation to check </param>
		/// <returns> true in case of correct prefix </returns>
		public static bool IsSpfRecord(string s)
		{
			return !String.IsNullOrEmpty(s) && s.StartsWith("v=spf1 ");
		}

		/// <summary>
		///   Tries to parse the textual representation of a SPF string
		/// </summary>
		/// <param name="s"> Textual representation to check </param>
		/// <param name="value"> Parsed spf record in case of successful parsing </param>
		/// <returns> true in case of successful parsing </returns>
		public static bool TryParse(string s, out SpfRecord value)
		{
			if (!IsSpfRecord(s))
			{
				value = null;
				return false;
			}

			string[] terms = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			List<SpfTerm> parsedTerms;
			if (TryParseTerms(terms, out parsedTerms))
			{
				value = new SpfRecord { Terms = parsedTerms };
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}
	}
}