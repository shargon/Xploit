using System.Net;

namespace XPloit.Core.Dns.DnsRecord
{
    /// <summary>
    ///   Interface for host address providing <see cref="DnsRecordBase">records</see>
    /// </summary>
    public interface IAddressRecord
    {
        /// <summary>
        ///   IP address of the host
        /// </summary>
        IPAddress Address { get; }
    }
}