namespace XPloit.Core.Dns.TSig
{
	/// <summary>
	///   Type of algorithm
	/// </summary>
	public enum TSigAlgorithm
	{
		/// <summary>
		///   Unknown
		/// </summary>
		Unknown,

		/// <summary>
		///   <para>MD5</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc2845">RFC 2845</see>
		///   </para>
		/// </summary>
		Md5,

		/// <summary>
		///   <para>SHA-1</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4635">RFC 4635</see>
		///   </para>
		/// </summary>
		Sha1, // RFC4635

		/// <summary>
		///   <para>SHA-256</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4635">RFC 4635</see>
		///   </para>
		/// </summary>
		Sha256,

		/// <summary>
		///   <para>SHA-384</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4635">RFC 4635</see>
		///   </para>
		/// </summary>
		Sha384,

		/// <summary>
		///   <para>SHA-512</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4635">RFC 4635</see>
		///   </para>
		/// </summary>
		Sha512,
	}
}