using System.Collections.Generic;
using XPloit.Core.Dns.DnsSec;

namespace XPloit.Core.Dns.EDns
{
	/// <summary>
	///   <para>DNSSEC Algorithm Understood option</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6975">RFC 6975</see>
	///   </para>
	/// </summary>
	public class DnssecAlgorithmUnderstoodOption : EDnsOptionBase
	{
		/// <summary>
		///   List of Algorithms
		/// </summary>
		public List<DnsSecAlgorithm> Algorithms { get; private set; }

		internal DnssecAlgorithmUnderstoodOption()
			: base(EDnsOptionType.DnssecAlgorithmUnderstood) {}

		/// <summary>
		///   Creates a new instance of the DnssecAlgorithmUnderstoodOption class
		/// </summary>
		/// <param name="algorithms">The list of algorithms</param>
		public DnssecAlgorithmUnderstoodOption(List<DnsSecAlgorithm> algorithms)
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