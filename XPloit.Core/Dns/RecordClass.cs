namespace XPloit.Core.Dns
{
	/// <summary>
	///   DNS record class
	/// </summary>
	public enum RecordClass : ushort
	{
		/// <summary>
		///   Invalid record class
		/// </summary>
		Invalid = 0,

		/// <summary>
		///   <para>Record class Internet (IN)</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
		///   </para>
		/// </summary>
		INet = 1,

		/// <summary>
		///   <para>Record class Chaois (CH)</para>
		///   <para>Defined: D. Moon, "Chaosnet", A.I. Memo 628, Massachusetts Institute of Technology Artificial Intelligence Laboratory, June 1981.</para>
		/// </summary>
		Chaos = 3,

		/// <summary>
		///   <para>Record class Hesiod (HS)</para>
		///   <para>Defined: Dyer, S., and F. Hsu, "Hesiod", Project Athena Technical Plan - Name Service, April 1987.</para>
		/// </summary>
		Hesiod = 4,

		/// <summary>
		///   <para>Record class NONE</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc2136">RFC 2136</see>
		///   </para>
		/// </summary>
		None = 254,

		/// <summary>
		///   <para>Record class * (ANY)</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
		///   </para>
		/// </summary>
		Any = 255
	}
}