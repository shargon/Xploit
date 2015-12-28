using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using XPloit.Core;
using XPloit.Core.Enums;

namespace XPloit.Modules.Auxiliary.Local
{
    public class BruteForceBitLockerRaw : Payload, BruteForce.ICheckPassword
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Crack Bitlocker drive"; } }
        public override string Path { get { return "Payloads/Local/BruteForce"; } }
        public override string Name { get { return "BruteForceBitLockerRaw"; } }
        public override Reference[] References { get { return new Reference[] { new Reference(EReferenceType.URL, "https://github.com/Aorimn/dislocker") }; } }
        #endregion

        #region Properties
        public string HexSalt { get; set; }
        public string HexNonce { get; set; }
        public string HexInputBuffer { get; set; }
        public string HexMac { get; set; }
        #endregion

        public BruteForceBitLockerRaw()
        {
            // VALORES REQUERIDOS PARA EL CRACK DEL TEST #123456#
            HexSalt = "33 b1 76 41 46 2c 04 5d 3e 55 db d9 c3 43 60 44";
            HexNonce = "e0 49 1b 14 4a b7 d0 01 03 00 00 00";
            HexInputBuffer = "0b 73 ab e8 60 c9 05 c7 57 62 d4 85 1b 0e 49 a0 6d c4 72 d4 99 1f 23 49 30 f9 27 de 50 69 12 66 23 74 d7 cd b5 09 52 66 e2 fd b9 88";
            HexMac = "16 21 33 02 af aa 69 2c-4a a3 f8 0b dc b9 54 af";
        }

        const int SHA256_DIGEST_LENGTH = 32;
        const int SALT_LENGTH = 16;
        const int AUTHENTICATOR_LENGTH = 16, AES_CTX_LENGTH = 256;
        const bool AES_ENCRYPT = true;
        static int InputBufferLength;
        static byte[] Salt, Nonce, InputBuffer, Mac;
        //public const int FILE_FLAG_NO_BUFFERING = 0x20000000;

        //[DllImport("kernel32", SetLastError = true)]
        //static extern IntPtr CreateFile(string FileName, FileAccess DesiredAccess, FileShare ShareMode, IntPtr SecurityAttributes,
        //     FileMode CreationDisposition, int FlagsAndAttributes, IntPtr hTemplate);


        #region Get HASH AND SALT
        //public enum version_t : ushort
        //{
        //    V_VISTA = 1,
        //    V_SEVEN = 2  // Same version used by Windows 8
        //};
        //public enum state_types : ushort
        //{
        //    DECRYPTED = 1,
        //    SWITCHING_ENCRYPTION = 2,
        //    ENCRYPTED = 4,
        //    SWITCH_ENCRYPTION_PAUSED = 5
        //};
        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public unsafe struct _bitlocker_header
        //{
        //    public fixed byte signature[8]; // = "-FVE-FS-"                                                   -- offset 0
        //    public fixed byte size[2];        // Total size (has to be multiplied by 16 when the version is 2)  -- offset 8
        //    public  /*version_t*/ fixed byte version[2];    // = 0x0002 for Windows 7 and 1 for Windows Vista                 -- offset 0xa

        //    /* Not sure about the next two fields */
        //    public /*state_types*/ fixed byte curr_state[2];  // Current encryption state                                        -- offset 0xc
        //    public /*state_types*/ fixed byte next_state[2];  // Next encryption state                                           -- offset 0xe

        //    public fixed byte encrypted_volume_size[8]; // Size of the encrypted volume                         -- offset 0x10
        //    /*
        //     * This size describe a virtualized region. This region is only checked when
        //     * this->curr_state == 2. It begins at the offset described by
        //     * this->encrypted_volume_size
        //     */
        //    public fixed byte unknown_size[4];  //                                                              -- offset 0x18
        //    public fixed byte nb_backup_sectors[4];   //                                                        -- offset 0x1c

        //    public fixed byte offset_bl_header[24]; //                                                        -- offset 0x20

        //    //union {
        //    //    uint64_t boot_sectors_backup; // Address where the boot sectors have been backed up -- offset 0x38
        //    public fixed byte mftmirror_backup[8];    // This is the address of the MftMirror for Vista     -- offset 0x38
        //    //};

        //    public _bitlocker_dataset dataset; // See above                                         -- offset 0x40
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public unsafe struct _bitlocker_dataset
        //{
        //    public fixed byte size[4];         //                      -- offset 0
        //    public fixed byte unknown1[4];     // = 0x0001 FIXME       -- offset 4
        //    public fixed byte header_size[4];  // = 0x0030             -- offset 8
        //    public fixed byte copy_size[4];    // = dataset_size       -- offset 0xc

        //    public fixed byte guid[16];           // dataset GUID         -- offset 0x10
        //    public fixed byte next_counter[4]; //                      -- offset 0x20

        //    public fixed byte algorithm[2];    //                      -- offset 0x24
        //    public fixed byte trash[2];        //                      -- offset 0x26
        //    public fixed byte timestamp[8]; //                      -- offset 0x28
        //} ;

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public unsafe struct _volume_header
        //{
        //    /* 512 bytes long */
        //    public fixed byte jump[3];             //                                                -- offset 0
        //    public fixed byte signature[8];        // = "-FVE-FS-" (without 0 at the string's end)   -- offset 3
        //    // = "NTFS    " (idem) for NTFS volumes (ORLY?)

        //    public fixed byte sector_size[2];         // = 0x0200 = 512 bytes                           -- offset 0xb
        //    public byte sectors_per_cluster; //                                                -- offset 0xd
        //    public fixed byte reserved_clusters[2];   //                                                -- offset 0xe
        //    public byte fat_count;           //                                                -- offset 0x10
        //    public fixed byte root_entries[2];        //                                                -- offset 0x11
        //    public fixed byte nb_sectors_16b[2];      //                                                -- offset 0x13
        //    public byte media_descriptor;    //                                                -- offset 0x15
        //    public fixed byte sectors_per_fat[2];     //                                                -- offset 0x16
        //    public fixed byte sectors_per_track[2];   //                                                -- offset 0x18
        //    public fixed byte nb_of_heads[2];         //                                                -- offset 0x1a
        //    public fixed byte hidden_sectors[4];      //                                                -- offset 0x1c
        //    public fixed byte nb_sectors_32b[4];      //                                                -- offset 0x20
        //    public fixed byte unknown2[4];         // For NTFS, always 0x00800080 (little endian)    -- offset 0x24
        //    public fixed byte nb_sectors_64b[8];      //                                                -- offset 0x28
        //    public fixed byte mft_start_cluster[8];   //                                                -- offset 0x30
        //    //union {                       // Metadata LCN or MFT Mirror                     -- offset 0x38
        //    //    uint64_t metadata_lcn;    //  depending on whether we're talking about a Vista volume
        //    public fixed byte mft_mirror[8];      //  or an NTFS one
        //    //};
        //    public fixed byte unknown3[96];        // FIXME                                          -- offset 0x40

        //    public fixed byte guid[16];                //                                                -- offset 0xa0

        //    public fixed byte offset_bl_header[24]; // NOT for Vista                                  -- offset 0xb0
        //    public fixed byte offset_eow_information[16]; // NOT for Vista nor 7                      -- offset 0xc8

        //    public fixed byte unknown4[294];       // FIXME                                          -- offset 0xd8

        //    public fixed byte boot_partition_identifier[2]; // = 0xaa55                                 -- offset 0x1fe
        //} ;
        #endregion
        #region VALIDATE PASSWORD
        public bool CheckPassword(string password)
        {
            //uint8_t *user_password = NULL;
            //uint8_t salt[16] = {0,};
            //uint8_t *result_key = NULL;
            //char good_key[] = {
            //    '\x39', '\xf5', '\x3f', '\xaf', '\x64', '\x09', '\x97', '\x2b',
            //    '\xb1', '\x2b', '\x8e', '\xb2', '\x44', '\xcb', '\x04', '\x40',
            //    '\x63', '\x57', '\x5c', '\xe5', '\xca', '\x3f', '\xce', '\x7f',
            //    '\xac', '\xc6', '\x8c', '\x66', '\x96', '\x2d', '\x94', '\xb6'
            //};
            //int ret = FALSE;

            byte[] result_key = new byte[32];

            //user_password = (uint8_t*) ck_password;

            ///* From function's documentation, size should be 32 */
            //result_key = xmalloc(32 * sizeof(char));
            //memset(result_key, 0, 32 * sizeof(char));

            ///* Tested unit */
            //ret = user_key(user_password, salt, result_key);

            byte[] utf16_password = Encoding.Unicode.GetBytes(password);

            //if (HashHelper.user_key(utf16_password, Salt, result_key))
            if (user_key(utf16_password, Salt, result_key))
            {
                // xprintf(L_DEBUG, "}--------[ Data passed to aes_ccm_encrypt_decrypt ]--------{\n");
                //xprintf(L_DEBUG, "-- Nonce:\n");
                //hexdump(L_DEBUG, input->nonce, 0xc);
                //xprintf(L_DEBUG, "-- Input buffer:\n");
                //hexdump(L_DEBUG, aes_input_buffer, input_size);
                //xprintf(L_DEBUG, "-- MAC:\n");
                //hexdump(L_DEBUG, mac_first, AUTHENTICATOR_LENGTH);
                //xprintf(L_DEBUG, "}----------------------------------------------------------{\n");

                //aes_ccm_encrypt_decrypt(&ctx, input->nonce, 0xc, aes_input_buffer, input_size, mac_first, AUTHENTICATOR_LENGTH, (unsigned char*) *output);

                byte[] mac_first = (byte[])Mac.Clone();
                byte[] inputBuffer = (byte[])InputBuffer.Clone();
                //Array.Copy(Mac, mac_first, AUTHENTICATOR_LENGTH);

                byte[] output = aes_ccm_encrypt_decrypt(Nonce, 0xc, inputBuffer, InputBufferLength, mac_first, AUTHENTICATOR_LENGTH, result_key);
                if (output == null) return false;

                //xfree(aes_input_buffer);
                ///*
                // * Compute to check decryption
                // */
                //memset(mac_second, 0, AUTHENTICATOR_LENGTH);
                //aes_ccm_compute_unencrypted_tag(&ctx, input->nonce, 0xc, (unsigned char*) *output, *output_size, mac_second);

                byte[] mac_second = aes_ccm_compute_unencrypted_tag(Nonce, 0xc, output, output.Length, result_key);


                /*
	            * Check if the MACs correspond, if not,
	             * we didn't decrypt correctly the input buffer
	             */
                //xprintf(L_INFO, "Looking if MACs match...\n");
                //xprintf(L_DEBUG, "They are just below:\n");
                //hexdump(L_DEBUG, mac_first, AUTHENTICATOR_LENGTH);
                //hexdump(L_DEBUG, mac_second, AUTHENTICATOR_LENGTH);

                //if (memcmp(mac_first, mac_second, AUTHENTICATOR_LENGTH) != 0)
                //{
                //    xprintf(L_ERROR, "The MACs don't match.\n");
                //    return FALSE;
                //}

                if (mac_second == null) return false;
                for (byte b = 0; b < 16; b++)
                    if (mac_second[b] != mac_first[b]) return false;
            }

            ///* Check unit outputs */
            //ck_assert_int_eq(ret, TRUE);
            //if(memcmp(result_key, good_key, 32) != 0)
            //{
            //    xfree(result_key);
            //    ck_abort_msg("Found result key doesn't match what it should");
            //}

            //xfree(result_key);

            return true;
        }

        public static byte[] AES_ECB_ENC(bool encrypt, byte[] cipherData, byte[] key)
        {
            try
            {
                //RijndaelManaged
                using (SymmetricAlgorithm rijndaelManaged = new AesManaged
                {
                    KeySize = 256,
                    BlockSize = 128,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None,

                    Key = key,
                    IV = new byte[16],
                })
                {
                    using (MemoryStream memoryStream = new MemoryStream(cipherData))
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream,
                        encrypt ? rijndaelManaged.CreateEncryptor() : rijndaelManaged.CreateDecryptor()
                        , CryptoStreamMode.Read))
                    {
                        byte[] bx = ReadFully(cryptoStream);
                        return bx;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }
      /*  public static byte[] AES_CBC(bool encrypt, byte[] cipherData, byte[] key)
        {
            try
            {
                //RijndaelManaged
                using (AesManaged rijndaelManaged = new AesManaged
                {
                    Key = key,
                    IV = cipherData,
                    //BlockSize = 256,
                    Mode = CipherMode.CBC
                })
                using (MemoryStream memoryStream = new MemoryStream(cipherData))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream,
                    encrypt ? rijndaelManaged.CreateEncryptor() : rijndaelManaged.CreateDecryptor()
                    , CryptoStreamMode.Read))
                {
                    byte[] bx = ReadFully(cryptoStream);
                    return bx;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }*/
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        static void xor_buffer(byte[] buf1, int index1, byte[] buf2, int index2, byte[] output, int index3, int size)
        {
            if (output != null)
                for (int loop = 0; loop < size; ++loop)
                    output[loop + index3] = (byte)(buf1[loop + index1] ^ buf2[loop + index2]);
            else
                for (int loop = 0; loop < size; ++loop)
                    buf1[loop + index1] = (byte)(buf1[loop + index1] ^ buf2[loop + index2]);
        }
        static byte[] aes_ccm_compute_unencrypted_tag(
                    byte[] nonce, int nonce_length,
                    byte[] buffer, int buffer_length,
                    byte[] key
            )
        {
            // Check parameters
            //if(!ctx || !buffer || !mac || nonce_length > 0xe)
            //    return FALSE;

            //xprintf(L_INFO, "Entering aes_ccm_compute_unencrypted_tag...\n");

            //unsigned char iv[AUTHENTICATOR_LENGTH];
            //unsigned int loop = 0;
            //unsigned int tmp_size = buffer_length;

            byte[] iv = new byte[AUTHENTICATOR_LENGTH];
            int loop = 0;
            int tmp_size = buffer_length;

            ///*
            // * Construct the IV
            // */
            //memset(iv, 0, AUTHENTICATOR_LENGTH);
            //iv[0] = ((unsigned char)(0xe - nonce_length)) | ((AUTHENTICATOR_LENGTH - 2) & 0xfe) << 2;

            iv[0] = (byte)((0xe - nonce_length) | (((AUTHENTICATOR_LENGTH - 2) & 0xfe) << 2));

            //memcpy(iv + 1, nonce, (nonce_length % AUTHENTICATOR_LENGTH));
            Array.Copy(nonce, 0, iv, 1, (nonce_length % AUTHENTICATOR_LENGTH));
            //for(loop = 15; loop > nonce_length; --loop)
            //{
            //    *(iv + loop) = tmp_size & 0xff;
            //    tmp_size = tmp_size >> 8;
            //}
            for (loop = 15; loop > nonce_length; loop--)
            {
                iv[loop] = (byte)(tmp_size & 0xff);
                tmp_size = tmp_size >> 8;
            }


            ///*
            // * Compute algorithm
            // */
            //AES_ECB_ENC(ctx, AES_ENCRYPT, iv, iv);
            iv = AES_ECB_ENC(AES_ENCRYPT, iv, key);


            //if(buffer_length > 16)
            //{
            //    loop = buffer_length >> 4;

            //    do
            //    {
            //        xprintf(L_DEBUG, "\tBuffer:\n");
            //        hexdump(L_DEBUG, buffer, 16);
            //        xprintf(L_DEBUG, "\tInternal IV:\n");
            //        hexdump(L_DEBUG, iv, 16);

            //        xor_buffer(iv, buffer, NULL, AUTHENTICATOR_LENGTH);

            //        AES_ECB_ENC(ctx, AES_ENCRYPT, iv, iv);

            //        buffer += AUTHENTICATOR_LENGTH;
            //        buffer_length -= AUTHENTICATOR_LENGTH;

            //    } while(--loop);
            //}

            int bufferIndex = 0;
            if (buffer_length > 16)
            {
                loop = buffer_length >> 4;
                do
                {
                    //xprintf(L_DEBUG, "\tBuffer:\n");
                    //hexdump(L_DEBUG, buffer, 16);
                    //xprintf(L_DEBUG, "\tInternal IV:\n");
                    //hexdump(L_DEBUG, iv, 16);

                    xor_buffer(iv, 0, buffer, bufferIndex, null, 0, AUTHENTICATOR_LENGTH);

                    iv = AES_ECB_ENC(AES_ENCRYPT, iv, key);

                    bufferIndex += AUTHENTICATOR_LENGTH;
                    buffer_length -= AUTHENTICATOR_LENGTH;

                    loop--;
                } while (loop > 0);
            }

            ///*
            // * Last block
            // */
            //if(buffer_length)
            //{
            //    xor_buffer(iv, buffer, NULL, buffer_length);
            //    AES_ECB_ENC(ctx, AES_ENCRYPT, iv, iv);
            //}

            if (buffer_length > 0)
            {
                xor_buffer(iv, 0, buffer, bufferIndex, null, 0, buffer_length);
                iv = AES_ECB_ENC(AES_ENCRYPT, iv, key);
            }

            //memcpy(mac, iv, AUTHENTICATOR_LENGTH);
            //memset(iv, 0, AUTHENTICATOR_LENGTH);

            //xprintf(L_INFO, "Ending aes_ccm_compute_unencrypted_tag successfully!\n");

            //return TRUE;
            return iv;
        }
        static byte[] aes_ccm_encrypt_decrypt(
                    byte[] nonce, int nonce_length,
                    byte[] input, int input_length,
                    byte[] mac, int mac_length,
                    byte[] key)
        {
            // Check parameters
            //if(!ctx || !input || !mac || !output)
            //    return FALSE;

            //xprintf(L_INFO, "Entering aes_ccm_encrypt_decrypt...\n");

            //unsigned char iv[16];
            //unsigned int loop = 0;
            //unsigned char tmp_buf[16] = {0,};
            //unsigned char* failsafe = NULL;

            byte[] iv = new byte[16];
            byte[] tmp_buf = new byte[16];
            int loop = 0;

            // * Here is how the counter works in microsoft compatible ccm implementation:
            // * 
            // * - User supplies a less than 16 bytes and more than 12 bytes iv
            // * 
            // * - Copy it in order to form this format:
            // *   15-iv_length-1 (1 byte)  |  iv (max 14 bytes) | counter counting from zero (from 1 to 3 byte)
            // * 
            // * - Apply counter mode of aes
            // * 
            // * (thanks to Kumar and Kumar for these explanations)

            //memset(iv, 0, sizeof(iv));
            int ixIv = 0;
            //memcpy(iv + 1, nonce, (nonce_length % sizeof(iv)));
            Array.Copy(nonce, 0, iv, 1, nonce_length % 16);
            ixIv += 1 + nonce_length % 16;

            //if(15 - nonce_length - 1 < 0) return FALSE;
            if (15 - nonce_length - 1 < 0) return null;

            //*iv = (unsigned char)(15 - nonce_length - 1);
            iv[0] = (byte)(15 - nonce_length - 1);

            //AES_ECB_ENC(ctx, AES_ENCRYPT, iv, tmp_buf);
            tmp_buf = AES_ECB_ENC(AES_ENCRYPT, iv, key);

            //string hex = BitConverter.ToString(tmp_buf);

            //xprintf(L_DEBUG, "\tTmp buffer:\n");
            //hexdump(L_DEBUG, tmp_buf, 16);
            //xprintf(L_DEBUG, "\tInput:\n");
            //hexdump(L_DEBUG, mac, mac_length);

            //xor_buffer(mac, tmp_buf, NULL, mac_length);
            xor_buffer(mac, 0, tmp_buf, 0, null, 0, mac_length);

            //xprintf(L_DEBUG, "\tOutput:\n");
            //hexdump(L_DEBUG, mac, mac_length);
            //hex = BitConverter.ToString(mac);

            ///* Increment the internal iv counter */
            iv[15] = 1;

            byte[] output = new byte[input_length];
            //if(input_length > sizeof(iv))
            //{
            //    loop = input_length >> 4;
            //    xprintf(L_DEBUG, "Input length: %d, loop: %d\n", input_length, loop);
            //    do
            //    {
            //        AES_ECB_ENC(ctx, AES_ENCRYPT, iv, tmp_buf);
            //        xor_buffer(input, tmp_buf, output, sizeof(iv));
            //        iv[15]++;
            //        /* A failsafe to not have the same iv twice */
            //        if(!iv[15])
            //        {
            //            failsafe = &iv[15];
            //            do
            //            {
            //                failsafe--;
            //                (*failsafe)++;
            //            } while(*failsafe == 0 && failsafe >= &iv[0]);
            //        }
            //        input += sizeof(iv);
            //        output += sizeof(iv);
            //        input_length = (unsigned int)(input_length - sizeof(iv));
            //    } while(--loop);
            //}

            int inputIndex = 0, outputIndex = 0;
            if (input_length > 16)
            {
                loop = input_length >> 4;
                //xprintf(L_DEBUG, "Input length: %d, loop: %d\n", input_length, loop);
                do
                {
                    tmp_buf = AES_ECB_ENC(AES_ENCRYPT, iv, key);
                    xor_buffer(input, inputIndex, tmp_buf, 0, output, outputIndex, 16);
                    iv[15]++;

                    /* A failsafe to not have the same iv twice */
                    //if(!iv[15])
                    //{
                    //    failsafe = &iv[15];
                    //    do
                    //    {
                    //        failsafe--;
                    //        (*failsafe)++;
                    //    } while(*failsafe == 0 && failsafe >= &iv[0]);
                    //}
                    inputIndex += 16;
                    outputIndex += 16;
                    input_length = (input_length - 16);

                    loop--;
                }
                while (loop > 0);
            }
            //xprintf(L_DEBUG, "Input length remain: %d\n", input_length);

            ///*
            //* Last block
            //*/
            //       if (input_length)
            //       {
            //           AES_ECB_ENC(ctx, AES_ENCRYPT, iv, tmp_buf);

            //           xor_buffer(input, tmp_buf, output, input_length);
            //       }
            if (input_length > 0)
            {
                tmp_buf = AES_ECB_ENC(AES_ENCRYPT, iv, key);

                xor_buffer(input, inputIndex, tmp_buf, 0, output, outputIndex, input_length);
            }

            ///* Cleanup */
            //memset(iv, 0, sizeof(iv));
            //memset(tmp_buf, 0, sizeof(tmp_buf));

            //xprintf(L_INFO, "Ending aes_ccm_encrypt_decrypt successfully!\n");

            //return TRUE;
            return output;
        }

        /**
* Compute the user hash from a user password using the stretch algorithm.
*
* @param user_password The raw user password that we have to calculate the hash
* @param salt The salt used for crypto (16 bytes)
* @param result_key Will contain the resulting hash key (32 bytes)
* @return TRUE if result can be trusted, FALSE otherwise
*/
        bool user_key(byte[] utf16_password, byte[] salt, byte[] result_key)
        {
            //uint16_t* utf16_password = NULL;
            //size_t    utf16_length   = 0;
            //uint8_t   user_hash[32]  = {0,};
            byte[] user_hash = new byte[SHA256_DIGEST_LENGTH];

            ///*
            // * We first get the SHA256(SHA256(to_UTF16(user_password)))
            // */
            //utf16_length   = (strlen((char*)user_password)+1) * sizeof(uint16_t);
            //utf16_password = xmalloc(utf16_length);
            //int utf16_length = (user_password.Length) * 2;

            //if(!asciitoutf16(user_password, utf16_password))
            //{
            //    xprintf(L_ERROR, "Can't convert user password to UTF-16, aborting.\n");
            //    memclean(utf16_password, utf16_length);
            //    return FALSE;
            //}

            //xprintf(L_DEBUG, "UTF-16 user password:\n");
            //hexdump(L_DEBUG, (uint8_t*)utf16_password, utf16_length);

            ///* We're not taking the '\0\0' end of the UTF-16 string */
            //SHA256((unsigned char *)utf16_password, utf16_length-2, user_hash);
            //SHA256((unsigned char *)user_hash,      32,             user_hash);

            user_hash = CalcSha256(utf16_password, utf16_password.Length/* utf16_length*/);
            user_hash = CalcSha256(user_hash, SHA256_DIGEST_LENGTH);

            ///*
            // * We then pass it to the key stretching manipulation
            // */
            //if(!stretch_user_key(user_hash, (uint8_t *)salt, result_key))
            //{
            //    xprintf(L_ERROR, "Can't stretch the user password, aborting.\n");
            //    memclean(utf16_password, utf16_length);
            //    return FALSE;
            //}

            //memclean(utf16_password, utf16_length);

            return stretch_user_key(user_hash, salt, result_key);
        }

        /**
 * Function implementing the stretching of a hashed user password.
 * @see stretch_key()
 * 
 * @param user_hash The 32-bytes hash from SHA256(SHA256(UTF16(user_password)))
 * @param salt The salt used for crypto (16 bytes)
 * @param result Will contain the resulting hash key (32 bytes)
 * @return TRUE if result can be trusted, FALSE otherwise
 */
        unsafe bool stretch_user_key(byte[] user_hash, byte[] salt, byte[] result)
        {
            //size_t size = sizeof(bitlocker_chain_hash_t);
            //bitlocker_chain_hash_t ch;

            bitlocker_chain_hash_t ch = new bitlocker_chain_hash_t();
            //memset(&ch, 0, size);

            //memcpy(ch.password_hash, user_hash, SHA256_DIGEST_LENGTH);
            //memcpy(ch.salt, salt, SALT_LENGTH);

            CopyToPointer(ch.password_hash, user_hash, SHA256_DIGEST_LENGTH);
            CopyToPointer(ch.salt, salt, SALT_LENGTH);

            //xprintf(L_INFO, "Stretching the user password, it could take some time...\n");
            //if (!stretch_key(&ch, result))
            //    return FALSE;

            if (!stretch_key(ch, result))
                return false;

            //xprintf(L_INFO, "Stretching of the user password is now ok!\n");

            ///* Wipe out with zeros */
            //memset(&ch, 0, size);

            return true;
        }
        unsafe static void CopyToPointer(byte* p, byte[] data, int length)
        {
            for (byte b = 0; b < length; b++) p[b] = data[b];
        }
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct bitlocker_chain_hash_t
        {
            public fixed byte updated_hash[SHA256_DIGEST_LENGTH];
            public fixed byte password_hash[SHA256_DIGEST_LENGTH];
            public fixed byte salt[SALT_LENGTH];
            public long hash_count;
        } ;

        public static byte[] CalcSha256(byte[] input, int size)
        {
            //byte[] output = new byte[32];
            //fixed (byte* pa = input, pb = output)
            //{
            //    ECudaResult x = GPU.sha256(pa, input.Length, pb);
            //    return output;
            //}
            using (SHA256Managed crypt = new SHA256Managed())
                return crypt.ComputeHash(input, 0, size);
        }

        /**
 * Core function implementing the chain hash algorithm.
 * @see stretch_recovery_key()
 * 
 * @param ch A pointer to a bitlocker_chain_hash_t structure
 * @param result Will contain the resulting hash key (32 bytes)
 * @return TRUE if result can be trusted, FALSE otherwise
 */
        unsafe static bool stretch_key(bitlocker_chain_hash_t ch, byte[] result)
        {
            //size_t   size = sizeof(bitlocker_chain_hash_t);
            //uint64_t loop = 0;

            //for(loop = 0; loop < 0x100000; ++loop)
            //{
            //    SHA256((unsigned char *)ch, size, ch->updated_hash);
            //    ch->hash_count++;
            //}

            for (int loop = 0; loop < 0x100000; loop++)
            {
                byte[] data = getBytes(ch);
                byte[] sha = CalcSha256(data, data.Length);

                CopyToPointer(ch.updated_hash, sha, SHA256_DIGEST_LENGTH);

                ch.hash_count++;
            }

            //memcpy(result, ch->updated_hash, SHA256_DIGEST_LENGTH);
            for (byte b = 0; b < SHA256_DIGEST_LENGTH; b++) result[b] = ch.updated_hash[b];
            //Array.Copy(ch.updated_hash, result, SHA256_DIGEST_LENGTH);

            return true;
        }

        unsafe static byte[] getBytes(bitlocker_chain_hash_t str)
        {
            //int len = 32 + 32 + 16 + 8;
            //int len = 184;// Marshal.SizeOf(str);

            int len = 88;// sizeof(bitlocker_chain_hash_t);// *myArray.Length;
            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);

            //bitlocker_chain_hash_t t = new bitlocker_chain_hash_t();
            //ByteArrayToStructure(arr, ref t);

            return arr;
        }
        #endregion

        public bool AllowMultipleOk { get { return false; } }
        /*
        public ushort ReadUShort(byte[] data, int index)
        {
            byte[] bUshort = new byte[2];
            Array.Copy(data, index, bUshort, 0, 2);

            return BitConverter.ToUInt16(bUshort, 0);
        }*/
        static byte[] ConvertToByteArray(string cad)
        {
            cad = cad.Trim();
            string[] bx = cad.Split(new char[] { ':', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] ret = new byte[bx.Length];

            for (int x = 0; x < ret.Length; x++)
                ret[x] = (byte)Convert.ToInt32(bx[x], 16);

            return ret;
        }
        public bool PreRun()
        {
            Salt = ConvertToByteArray(HexSalt);

            Nonce = ConvertToByteArray(HexNonce);
            InputBuffer = ConvertToByteArray(HexInputBuffer);
            Mac = ConvertToByteArray(HexMac);

            // VALORES REQUERIDOS PARA EL CRACK DEL TEST #123456#
            //Salt = ConvertToByteArray("33 b1 76 41 46 2c 04 5d 3e 55 db d9 c3 43 60 44");

            //Nonce = ConvertToByteArray("e0 49 1b 14 4a b7 d0 01 03 00 00 00");
            //InputBuffer = ConvertToByteArray("0b 73 ab e8 60 c9 05 c7 57 62 d4 85 1b 0e 49 a0 6d c4 72 d4 99 1f 23 49 30 f9 27 de 50 69 12 66 23 74 d7 cd b5 09 52 66 e2 fd b9 88");
            //Mac = ConvertToByteArray("16 21 33 02 af aa 69 2c-4a a3 f8 0b dc b9 54 af");
            // ********************************

            // VALORES REQUERIDOS PARA EL CRACK DEL REAL!
            //Salt = ConvertToByteArray("9f 0a ba 66 70 c6 cb 0c b0 75 d4 84 f0 ad 68 34");

            //Nonce = ConvertToByteArray("00 6a e1 86 34 45 d0 01 03 00 00 00");
            //InputBuffer = ConvertToByteArray("38 3c 76 ea 61 35 3f 23 c5 e0 7e 38 c1 34 c7 d8 5c 7d a7 3a 0d 4f 70 3e 56 9d bb 3d b6 1d eb c1 9e 77 48 a5 e3 93 2b e8 c1 23 cf ca");
            //Mac = ConvertToByteArray("e9 bc 37 40 1d 43 2d fe-a7 f1 77 e8 09 88 47 18");
            // ********************************

            /*
            int x = 0;
            x++;
            
            long offSet = -1;
            int blockSize = -1;

            #region Get Offset of partition
            using (Process proc = new Process
              {
                  StartInfo = new ProcessStartInfo
                  {
                      FileName = "wmic",
                      Arguments = "partition get BlockSize, StartingOffset, Name",
                      UseShellExecute = false,
                      RedirectStandardOutput = true,
                      CreateNoWindow = true
                  }
              })
            {

                proc.Start();
                proc.WaitForExit();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    if (line.Contains("#" + driveNum.ToString() + ", "))
                    {
                        string[] sp = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        offSet = Convert.ToInt64(sp[sp.Length - 1]);
                        blockSize = Convert.ToInt32(sp[0]);
                        break;
                    }
                    // do something with line
                }
            }
            #endregion

            //*****
            IntPtr handle = CreateFile(
                //"\\\\.\\" + drive.ToString() + ":",
                @"\\.\PHYSICALDRIVE" + driveNum.ToString(),
             FileAccess.Read, FileShare.None, IntPtr.Zero, FileMode.Open, FILE_FLAG_NO_BUFFERING, IntPtr.Zero);

            ushort bytesPerSector, numberOfSectorsPerCluster, metadataLogicalClusterNumber;

            byte[] inBuffer = new byte[blockSize];
            using (FileStream disk = new FileStream(handle, FileAccess.Read))
            {
                disk.Seek(offSet, SeekOrigin.Begin);

                disk.Read(inBuffer, 0, blockSize);
                //for (int i = 0; i < blockSize; i++)
                //    inBuffer[i] = (byte)disk.ReadByte();

                bytesPerSector = ReadUShort(inBuffer, 0xB);
                numberOfSectorsPerCluster = ReadUShort(inBuffer, 0xD);
                metadataLogicalClusterNumber = ReadUShort(inBuffer, 0x38);

            }

            _bitlocker_header header = BruteForce.FromBytes<_bitlocker_header>(inBuffer);
            */
            /*
            C:\Windows\system32>manage-bde -status E:
            Cifrado de unidad BitLocker: versión de la herramienta de configuración 6.3.9600
            Copyright (C) 2013 Microsoft Corporation. Todos los derechos reservados.

            Volumen E: [Etiqueta desconocida]
            [Volumen de datos]

                Tamaño:                 Desconocido GB
                Versión de BitLocker:   2.0
                Estado de conversión:   Desconocido
                Porcentaje cifrado:     Desconocido%
                Método de cifrado:      AES 128
                Estado de protección:   Desconocido
                Estado de bloqueo:      Bloqueado
                Campo de identificación:Desconocido
                Desbloqueo automático:  Deshabilitado
                Protectores de clave:
                    Contraseña
                    Contraseña numérica


            C:\Windows\system32>manage-bde -protectors -get e:
            Cifrado de unidad BitLocker: versión de la herramienta de configuración 6.3.9600
            Copyright (C) 2013 Microsoft Corporation. Todos los derechos reservados.

            Volumen E: [Etiqueta desconocida]
            Todos los protectores de clave

                Contraseña:
                  Id.:      {9FFF4471-E895-46A4-A69F-9D3856F78ED3}
                  Salt:     9f 0a ba 66 70 c6 cb 0c b0 75 d4 84 f0 ad 68 34

                Contraseña numérica:
                  Id.:      {DAC7EEA6-DD1F-4CA7-B68A-8A8B14D83866}
                  Salt:     69 24 b5 21 f3 1c 8d 4e 97 b3 c0 86 36 a0 56 0a
            */

            InputBufferLength = InputBuffer.Length;
            return true;
        }
        public void PostRun() { }
    }
}