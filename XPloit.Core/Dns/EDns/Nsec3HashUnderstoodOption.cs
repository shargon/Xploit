using System.Collections.Generic;
using XPloit.Core.Dns.DnsSec;

namespace XPloit.Core.Dns.EDns
{
	/// <summary>
	///   <para>NSEC3 Hash Unterstood option</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6975">RFC 6975</see>
	///   </para>
	/// </summary>
	public class Nsec3HashUnderstoodOption : EDnsOptionBase
	{
		/// <summary>
		///   List of Algorithms
		/// </summary>
		public List<DnsSecAlgorithm> Algorithms { get; private set; }

		internal Nsec3HashUnderstoodOption()
			: base(EDnsOptionType.Nsec3HashUnderstood) {}

		/// <summary>
		///   Creates a new instance of the Nsec3HashUnderstoodOption class
		/// </summary>
		/// <param name="algorithms">The list of algorithms</param>
		public Nsec3HashUnderstoodOption(List<DnsSecAlgorithm> algorithms)
			: this()
		{
			Algorithms = algorithms;
		}

		internal override void ParseData(byte[] resultData, int startPosition, int length)
		{
			Algorithms = new List<DnsSecAlgorithm>(length);
			for (int i = 0; i < length; i++)
			{
				Algorithms.Add((DnsSecAlgorithm) resultData[startPosition++]);
			}
		}

		internal override ushort DataLength
		{
			get { return (ushort) ((Algorithms == null) ? 0 : Algorithms.Count); }
		}

		internal override void EncodeData(byte[] messageData, ref int currentPosition)
		{
			foreach (var algorithm in Algorithms)
			{
				messageData[currentPosition++] = (byte) algorithm;
			}
		}
	}
}