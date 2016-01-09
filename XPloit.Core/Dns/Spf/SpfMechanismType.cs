namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Type of spf mechanism
	/// </summary>
	public enum SpfMechanismType
	{
		/// <summary>
		///   Unknown mechanism
		/// </summary>
		Unknown,

		/// <summary>
		///   All mechanism, matches always
		/// </summary>
		All,

		/// <summary>
		///   IP4 mechanism, matches if ip address (IPv4) is within the given network
		/// </summary>
		Ip4,

		/// <summary>
		///   IP6 mechanism, matches if ip address (IPv6) is within the given network
		/// </summary>
		Ip6,

		/// <summary>
		///   A mechanism, matches if the ip address is the target of a hostname lookup for the given domain
		/// </summary>
		A,

		/// <summary>
		///   MX mechanism, matches if the ip address is a mail exchanger for the given domain
		/// </summary>
		Mx,

		/// <summary>
		///   PTR mechanism, matches if a correct reverse mapping exists
		/// </summary>
		Ptr,

		/// <summary>
		///   EXISTS mechanism, matches if the given domain exists
		/// </summary>
		Exist,

		/// <summary>
		///   INCLUDE mechanism, triggers a recursive evaluation
		/// </summary>
		Include,
	}
}