﻿namespace XPloit.Core.Dns.DnsSec
{
	/// <summary>
	///   DNSSEC algorithm type
	/// </summary>
	public enum DnsSecAlgorithm : byte
	{
		/// <summary>
		///   <para>RSA MD5</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4034">RFC 4034</see>
		///   </para>
		/// </summary>
		RsaMd5 = 1,

		/// <summary>
		///   <para>Diffie Hellman</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc2539">RFC 2539</see>
		///   </para>
		/// </summary>
		DiffieHellman = 2,

		/// <summary>
		///   <para>DSA/SHA-1</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc3755">RFC 3755</see>
		///   </para>
		/// </summary>
		DsaSha1 = 3,

		/// <summary>
		///   <para>Elliptic curves</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc">RFC</see>
		///   </para>
		/// </summary>
		EllipticCurve = 4,

		/// <summary>
		///   <para>RSA/SHA-1</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc3755">RFC 3755</see>
		///   </para>
		/// </summary>
		RsaSha1 = 5,

		/// <summary>
		///   <para>DSA/SHA-1 using NSEC3 hashs</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc5155">RFC 5155</see>
		///   </para>
		/// </summary>
		DsaNsec3Sha1 = 6,

		/// <summary>
		///   <para>RSA/SHA-1 using NSEC3 hashs</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc5155">RFC 5155</see>
		///   </para>
		/// </summary>
		RsaSha1Nsec3Sha1 = 7,

		/// <summary>
		///   <para>RSA/SHA-256</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc5702">RFC 5702</see>
		///   </para>
		/// </summary>
		RsaSha256 = 8,

		/// <summary>
		///   <para>RSA/SHA-512</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc5702">RFC 5702</see>
		///   </para>
		/// </summary>
		RsaSha512 = 10,

		/// <summary>
		///   <para>GOST Signature</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc5933">RFC 5933</see>
		///   </para>
		/// </summary>
		EccGost = 12,

		/// <summary>
		///   <para>Indirect</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4034">RFC 4034</see>
		///   </para>
		/// </summary>
		Indirect = 252, // RFC4034

		/// <summary>
		///   <para>Private key using named algorithm</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc3755">RFC 3755</see>
		///   </para>
		/// </summary>
		PrivateDns = 253,

		/// <summary>
		///   <para>Private key using algorithm object identifier</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc3755">RFC 3755</see>
		///   </para>
		/// </summary>
		PrivateOid = 254,
	}
}