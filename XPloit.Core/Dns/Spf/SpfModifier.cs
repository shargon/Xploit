using System.Text;
using XPloit.Core.Helpers;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Represents a single modifier term in a SPF record
	/// </summary>
	public class SpfModifier : SpfTerm
	{
		/// <summary>
		///   Type of the modifier
		/// </summary>
		public SpfModifierType Type { get; set; }

		/// <summary>
		///   Domain part of the modifier
		/// </summary>
		public string Domain { get; set; }

		/// <summary>
		///   Returns the textual representation of a modifier term
		/// </summary>
		/// <returns> Textual representation </returns>
		public override string ToString()
		{
			StringBuilder res = new StringBuilder();

			res.Append(EnumHelper<SpfModifierType>.ToString(Type).ToLower());
			res.Append("=");
			res.Append(Domain);

			return res.ToString();
		}
	}
}