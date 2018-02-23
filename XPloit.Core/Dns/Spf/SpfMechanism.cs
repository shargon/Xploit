using System;
using System.Text;
using XPloit.Helpers;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Represents a single mechanism term in a SPF record
	/// </summary>
	public class SpfMechanism : SpfTerm
	{
		/// <summary>
		///   Qualifier of the mechanism
		/// </summary>
		public SpfQualifier Qualifier { get; set; }

		/// <summary>
		///   Type of the mechanism
		/// </summary>
		public SpfMechanismType Type { get; set; }

		/// <summary>
		///   Domain part of the mechanism
		/// </summary>
		public string Domain { get; set; }

		/// <summary>
		///   IPv4 prefix of the mechanism
		/// </summary>
		public int? Prefix { get; set; }

		/// <summary>
		///   IPv6 prefix of the mechanism
		/// </summary>
		public int? Prefix6 { get; set; }

		/// <summary>
		///   Returns the textual representation of a mechanism term
		/// </summary>
		/// <returns> Textual representation </returns>
		public override string ToString()
		{
			StringBuilder res = new StringBuilder();

			switch (Qualifier)
			{
				case SpfQualifier.Fail:
					res.Append("-");
					break;
				case SpfQualifier.SoftFail:
					res.Append("~");
					break;
				case SpfQualifier.Neutral:
					res.Append("?");
					break;
			}

			res.Append(EnumHelper<SpfMechanismType>.ToString(Type).ToLower());

			if (!String.IsNullOrEmpty(Domain))
			{
				res.Append(":");
				res.Append(Domain);
			}

			if (Prefix.HasValue)
			{
				res.Append("/");
				res.Append(Prefix.Value);
			}

			if (Prefix6.HasValue)
			{
				res.Append("//");
				res.Append(Prefix6.Value);
			}

			return res.ToString();
		}
	}
}