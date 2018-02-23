using System;

namespace XPloit.Core.Dns
{
	/// <summary>
	///   Operation code of a dns query
	/// </summary>
	public enum OperationCode : ushort
	{
		/// <summary>
		///   <para>Normal query</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
		///   </para>
		/// </summary>
		Query = 0,

		/// <summary>
		///   <para>Inverse query</para>
		///   <para>
		///     Obsoleted by
		///     <see cref="!:http://tools.ietf.org/html/rfc3425">RFC 3425</see>
		///   </para>
		/// </summary>
		[Obsolete]
		InverseQuery = 1,

		/// <summary>
		///   <para>Server status request</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
		///   </para>
		/// </summary>
		Status = 2,

		/// <summary>
		///   <para>Notify of zone change</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc1996">RFC 1996</see>
		///   </para>
		/// </summary>
		Notify = 4,

		/// <summary>
		///   <para>Dynamic update</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc2136">RFC 2136</see>
		///   </para>
		/// </summary>
		Update = 5,
	}
}