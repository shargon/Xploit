namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Qualifier of spf mechanism
	/// </summary>
	public enum SpfQualifier
	{
		/// <summary>
		///   No records were published or no checkable sender could be determined
		/// </summary>
		None,

		/// <summary>
		///   Client is allowed to send mail with the given identity
		/// </summary>
		Pass,

		/// <summary>
		///   Client is explicit not allowed to send mail with the given identity
		/// </summary>
		Fail,

		/// <summary>
		///   Client is not allowed to send mail with the given identity
		/// </summary>
		SoftFail,

		/// <summary>
		///   No statement if a client is allowed or not allowed to send mail with the given identity
		/// </summary>
		Neutral,

		/// <summary>
		///   A transient error encountered while performing the check
		/// </summary>
		TempError,

		/// <summary>
		///   The published record could not be correctly interpreted
		/// </summary>
		PermError,
	}
}