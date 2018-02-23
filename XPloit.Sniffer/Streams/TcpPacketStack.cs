using PacketDotNet;
using System.Collections.Generic;

namespace XPloit.Sniffer.Streams
{
    public class TcpPacketStack
    {
        Dictionary<uint, TcpPacket> _Stack = new Dictionary<uint, TcpPacket>();
        /// <summary>
        /// SequenceNumber
        /// </summary>
        public uint SequenceNumber { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sequenceNumber">SequenceNumber</param>
        public TcpPacketStack(uint sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }
        /// <summary>
        /// Try get next Packet
        /// </summary>
        /// <param name="sequenceNumber">Sequence Number</param>
        /// <param name="packet">Packet</param>
        public bool TryGetNextPacket(out TcpPacket packet)
        {
            if (_Stack.TryGetValue(SequenceNumber, out packet))
            {
                _Stack.Remove(SequenceNumber);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Append to stack
        /// </summary>
        /// <param name="sequenceNumber">Sequence Number</param>
        /// <param name="packet">Packet</param>
        public void Append(uint sequenceNumber, TcpPacket packet)
        {
            if (!_Stack.ContainsKey(sequenceNumber))
                _Stack.Add(sequenceNumber, packet);
        }
        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString() { return SequenceNumber.ToString(); }
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear() { _Stack.Clear(); }
    }
}