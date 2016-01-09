using System;
using System.Collections.Generic;
using System.Globalization;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Geographical position</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1712">RFC 1712</see>
	///   </para>
	/// </summary>
	public class GPosRecord : DnsRecordBase
	{
		/// <summary>
		///   Longitude of the geographical position
		/// </summary>
		public double Longitude { get; private set; }

		/// <summary>
		///   Latitude of the geographical position
		/// </summary>
		public double Latitude { get; private set; }

		/// <summary>
		///   Altitude of the geographical position
		/// </summary>
		public double Altitude { get; private set; }

		internal GPosRecord() {}

		/// <summary>
		///   Creates a new instance of the GPosRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="longitude"> Longitude of the geographical position </param>
		/// <param name="latitude"> Latitude of the geographical position </param>
		/// <param name="altitude"> Altitude of the geographical position </param>
		public GPosRecord(string name, int timeToLive, double longitude, double latitude, double altitude)
			: base(name, RecordType.GPos, RecordClass.INet, timeToLive)
		{
			Longitude = longitude;
			Latitude = latitude;
			Altitude = altitude;
		}

		internal override void ParseRecordData(byte[] resultData, int currentPosition, int length)
		{
			Longitude = Double.Parse(DnsMessageBase.ParseText(resultData, ref currentPosition), CultureInfo.InvariantCulture);
			Latitude = Double.Parse(DnsMessageBase.ParseText(resultData, ref currentPosition), CultureInfo.InvariantCulture);
			Altitude = Double.Parse(DnsMessageBase.ParseText(resultData, ref currentPosition), CultureInfo.InvariantCulture);
		}

		internal override string RecordDataToString()
		{
			return Longitude.ToString(CultureInfo.InvariantCulture)
			       + " " + Latitude.ToString(CultureInfo.InvariantCulture)
			       + " " + Altitude.ToString(CultureInfo.InvariantCulture);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 3 + Longitude.ToString().Length + Latitude.ToString().Length + Altitude.ToString().Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, Longitude.ToString(CultureInfo.InvariantCulture));
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, Latitude.ToString(CultureInfo.InvariantCulture));
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, Altitude.ToString(CultureInfo.InvariantCulture));
		}
	}
}