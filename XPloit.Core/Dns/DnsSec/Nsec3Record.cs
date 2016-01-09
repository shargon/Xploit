using System;
using System.Collections.Generic;
using XPloit.Core.Dns.DnsRecord;
using XPloit.Core.Helpers;

namespace XPloit.Core.Dns.DnsSec
{
	/// <summary>
	///   Hashed next owner
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc5155">RFC 5155</see>
	///   </para>
	/// </summary>
	public class NSec3Record : DnsRecordBase
	{
		/// <summary>
		///   Algorithm of hash
		/// </summary>
		public DnsSecAlgorithm HashAlgorithm { get; private set; }

		/// <summary>
		///   Flags of the record
		/// </summary>
		public byte Flags { get; private set; }

		/// <summary>
		///   Number of iterations
		/// </summary>
		public ushort Iterations { get; private set; }

		/// <summary>
		///   Binary data of salt
		/// </summary>
		public byte[] Salt { get; private set; }

		/// <summary>
		///   Binary data of hash of next owner
		/// </summary>
		public byte[] NextHashedOwnerName { get; private set; }

		/// <summary>
		///   Types of next owner
		/// </summary>
		public List<RecordType> Types { get; private set; }

		internal NSec3Record() {}

		/// <summary>
		///   Creates of new instance of the NSec3Record class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="recordClass"> Class of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="hashAlgorithm"> Algorithm of hash </param>
		/// <param name="flags"> Flags of the record </param>
		/// <param name="iterations"> Number of iterations </param>
		/// <param name="salt"> Binary data of salt </param>
		/// <param name="nextHashedOwnerName"> Binary data of hash of next owner </param>
		/// <param name="types"> Types of next owner </param>
		public NSec3Record(string name, RecordClass recordClass, int timeToLive, DnsSecAlgorithm hashAlgorithm, byte flags, ushort iterations, byte[] salt, byte[] nextHashedOwnerName, List<RecordType> types)
			: base(name, RecordType.NSec3, recordClass, timeToLive)
		{
			HashAlgorithm = hashAlgorithm;
			Flags = flags;
			Iterations = iterations;
			Salt = salt ?? new byte[] { };
			NextHashedOwnerName = nextHashedOwnerName ?? new byte[] { };

			if ((types == null) || (types.Count == 0))
			{
				Types = new List<RecordType>();
			}
			else
			{
				Types = new List<RecordType>(types);
				types.Sort((left, right) => ((ushort) left).CompareTo((ushort) right));
			}
		}

		internal override void ParseRecordData(byte[] resultData, int currentPosition, int length)
		{
			int endPosition = currentPosition + length;

			HashAlgorithm = (DnsSecAlgorithm) resultData[currentPosition++];
			Flags = resultData[currentPosition++];
			Iterations = DnsMessageBase.ParseUShort(resultData, ref currentPosition);
			int saltLength = resultData[currentPosition++];
			Salt = DnsMessageBase.ParseByteData(resultData, ref currentPosition, saltLength);
			int hashLength = resultData[currentPosition++];
			NextHashedOwnerName = DnsMessageBase.ParseByteData(resultData, ref currentPosition, hashLength);
			Types = NSecRecord.ParseTypeBitMap(resultData, ref currentPosition, endPosition);
		}

		internal override string RecordDataToString()
		{
			return (byte) HashAlgorithm
			       + " " + Flags
			       + " " + Iterations
                   + " " + ((Salt.Length == 0) ? "-" : BaseEncodingHelper.ToBase16String(Salt))
                   + " " + BaseEncodingHelper.ToBase32HexString(NextHashedOwnerName)
			       + " " + String.Join(" ", Types.ConvertAll<String>(ToString).ToArray());
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 6 + Salt.Length + NextHashedOwnerName.Length + NSecRecord.GetMaximumTypeBitmapLength(Types); }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			messageData[currentPosition++] = (byte) HashAlgorithm;
			messageData[currentPosition++] = Flags;
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Iterations);
			messageData[currentPosition++] = (byte) Salt.Length;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Salt);
			messageData[currentPosition++] = (byte) NextHashedOwnerName.Length;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, NextHashedOwnerName);
			NSecRecord.EncodeTypeBitmap(messageData, ref currentPosition, Types);
		}
	}
}