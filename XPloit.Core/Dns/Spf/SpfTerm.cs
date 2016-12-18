using System;
using System.Text.RegularExpressions;
using XPloit.Helpers;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Represents a single term of a SPF record
	/// </summary>
	public class SpfTerm
	{
		internal static bool TryParse(string s, out SpfTerm value)
		{
			if (String.IsNullOrEmpty(s))
			{
				value = null;
				return false;
			}

			#region Parse Mechanism
			Regex regex = new Regex(@"^(\s)*(?<qualifier>[~+?-]?)(?<type>[a-z0-9]+)(:(?<domain>[^/]+))?(/(?<prefix>[0-9]+)(/(?<prefix6>[0-9]+))?)?(\s)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Match match = regex.Match(s);
			if (match.Success)
			{
				SpfMechanism mechanism = new SpfMechanism();

				switch (match.Groups["qualifier"].Value)
				{
					case "+":
						mechanism.Qualifier = SpfQualifier.Pass;
						break;
					case "-":
						mechanism.Qualifier = SpfQualifier.Fail;
						break;
					case "~":
						mechanism.Qualifier = SpfQualifier.SoftFail;
						break;
					case "?":
						mechanism.Qualifier = SpfQualifier.Neutral;
						break;

					default:
						mechanism.Qualifier = SpfQualifier.Pass;
						break;
				}

				SpfMechanismType type;
				mechanism.Type = EnumHelper<SpfMechanismType>.TryParse(match.Groups["type"].Value, true, out type) ? type : SpfMechanismType.Unknown;

				mechanism.Domain = match.Groups["domain"].Value;

				string tmpPrefix = match.Groups["prefix"].Value;
				int prefix;
				if (!String.IsNullOrEmpty(tmpPrefix) && Int32.TryParse(tmpPrefix, out prefix))
				{
					mechanism.Prefix = prefix;
				}

				tmpPrefix = match.Groups["prefix6"].Value;
				if (!String.IsNullOrEmpty(tmpPrefix) && Int32.TryParse(tmpPrefix, out prefix))
				{
					mechanism.Prefix6 = prefix;
				}

				value = mechanism;
				return true;
			}
			#endregion

			#region Parse Modifier
			regex = new Regex(@"^(\s)*(?<type>[a-z]+)=(?<domain>[^\s]+)(\s)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			match = regex.Match(s);
			if (match.Success)
			{
				SpfModifier modifier = new SpfModifier();

				SpfModifierType type;
				modifier.Type = EnumHelper<SpfModifierType>.TryParse(match.Groups["type"].Value, true, out type) ? type : SpfModifierType.Unknown;
				modifier.Domain = match.Groups["domain"].Value;

				value = modifier;
				return true;
			}
			#endregion

			value = null;
			return false;
		}
	}
}