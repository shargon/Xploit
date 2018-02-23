namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Type of the spf modifier
	/// </summary>
	public enum SpfModifierType
	{
		/// <summary>
		///   Unknown mechanism
		/// </summary>
		Unknown,

		/// <summary>
		///   REDIRECT modifier, redirects the evaluation to another record, if of all tests fail
		/// </summary>
		Redirect,

		/// <summary>
		///   EXP modifier, used for lookup of a explanation in case of failed test
		/// </summary>
		Exp,
	}
}