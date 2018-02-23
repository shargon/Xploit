namespace XPloit.Core.Dns.DnsSec
{
	/// <summary>
	///   Type of DNSSEC digest
	/// </summary>
	public enum DnsSecDigestType : byte
	{
		/// <summary>
		///   <para>SHA-1</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc3658">RFC 3658</see>
		///   </para>
		/// </summary>
		Sha1 = 1,

		/// <summary>
		///   <para>SHA-256</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4509">RFC 4509</see>
		///   </para>
		/// </summary>
		Sha256 = 2,
	}
}