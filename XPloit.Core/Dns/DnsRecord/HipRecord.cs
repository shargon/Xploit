using System;
using System.Collections.Generic;
using System.Linq;
using XPloit.Helpers;

namespace XPloit.Core.Dns.DnsRecord
{
    /// <summary>
    ///   <para>Host identity protocol</para>
    ///   <para>
    ///     Defined in
    ///     <see cref="!:http://tools.ietf.org/html/rfc5205">RFC 5205</see>
    ///   </para>
    /// </summary>
    public class HipRecord : DnsRecordBase
    {
        /// <summary>
        ///   Algorithm of the key
        /// </summary>
        public IpSecKeyRecord.IpSecAlgorithm Algorithm { get; private set; }

        /// <summary>
        ///   Host identity tag
        /// </summary>
        public byte[] Hit { get; private set; }

        /// <summary>
        ///   Binary data of the public key
        /// </summary>
        public byte[] PublicKey { get; private set; }

        /// <summary>
        ///   Possible rendezvous servers
        /// </summary>
        public List<string> RendezvousServers { get; private set; }

        internal HipRecord() { }

        /// <summary>
        ///   Creates a new instace of the HipRecord class
        /// </summary>
        /// <param name="name"> Name of the record </param>
        /// <param name="timeToLive"> Seconds the record should be cached at most </param>
        /// <param name="algorithm"> Algorithm of the key </param>
        /// <param name="hit"> Host identity tag </param>
        /// <param name="publicKey"> Binary data of the public key </param>
        /// <param name="rendezvousServers"> Possible rendezvous servers </param>
        public HipRecord(string name, int timeToLive, IpSecKeyRecord.IpSecAlgorithm algorithm, byte[] hit, byte[] publicKey, List<string> rendezvousServers)
            : base(name, RecordType.Hip, RecordClass.INet, timeToLive)
        {
            Algorithm = algorithm;
            Hit = hit ?? new byte[] { };
            PublicKey = publicKey ?? new byte[] { };
            RendezvousServers = rendezvousServers ?? new List<string>();
        }

        internal override void ParseRecordData(byte[] resultData, int currentPosition, int length)
        {
            int endPosition = currentPosition + length;

            int hitLength = resultData[currentPosition++];
            Algorithm = (IpSecKeyRecord.IpSecAlgorithm)resultData[currentPosition++];
            int publicKeyLength = DnsMessageBase.ParseUShort(resultData, ref currentPosition);
            Hit = DnsMessageBase.ParseByteData(resultData, ref currentPosition, hitLength);
            PublicKey = DnsMessageBase.ParseByteData(resultData, ref currentPosition, publicKeyLength);
            RendezvousServers = new List<string>();
            while (currentPosition < endPosition)
            {
                RendezvousServers.Add(DnsMessageBase.ParseDomainName(resultData, ref currentPosition));
            }
        }

        internal override string RecordDataToString()
        {
            return (byte)Algorithm
                   + " " + BaseEncodingHelper.ToBase16String(Hit)
                   + " " + BaseEncodingHelper.ToBase64String(PublicKey)
                   + String.Join("", RendezvousServers.Select(s => " " + s).ToArray());
        }

        protected internal override int MaximumRecordDataLength
        {
            get
            {
                int res = 4;
                res += Hit.Length;
                res += PublicKey.Length;
                res += RendezvousServers.Sum(s => s.Length + 2);
                return res;
            }
        }

        protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
        {
            messageData[currentPosition++] = (byte)Hit.Length;
            messageData[currentPosition++] = (byte)Algorithm;
            DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort)PublicKey.Length);
            DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Hit);
            DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, PublicKey);
            foreach (string server in RendezvousServers)
            {
                DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, server, false, domainNames);
            }
        }
    }
}