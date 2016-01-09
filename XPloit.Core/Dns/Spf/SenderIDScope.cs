namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Scope of a SenderID record
	/// </summary>
	public enum SenderIDScope
	{
		/// <summary>
		///   Unknown scope
		/// </summary>
		Unknown,

		/// <summary>
		///   MFrom scope, used for lookups of SMTP MAIL FROM address
		/// </summary>
		MFrom,

		/// <summary>
		///   PRA scope, used for lookups of the Purported Responsible Address
		/// </summary>
		Pra,
	}
}