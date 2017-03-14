using System;
using System.Text;
using XPloit.Sniffer.Enums;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;

namespace XPloit.Sniffer.Extractors
{
    public class ExtractFtpPop3 : IObjectExtractor
    {
        /*
# FTP https://en.wikipedia.org/wiki/List_of_FTP_server_return_codes

## OK ##

S> 220-  ~~~ Welcome to OVH ~~~
S> 220 This is a private system - No anonymous login
C> USER miUser
S> 331 User nabla OK. Password required
C> PASS miPass
S> 230 OK. Current restricted directory is /


## ERROR ##

S> 220-  ~~~ Welcome to OVH ~~~
S> 220 This is a private system - No anonymous login
C> USER miUser
S> 331 User nabla OK. Password required
C> PASS miPass
S> 530 Login authentication failed


# POP3
#https://tools.ietf.org/html/rfc1939#page-15


S> +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
C> APOP mrose c4c9334bac560ecc979e58001b3e22fb
S> +OK mrose's maildrop has 2 messages (320 octets)
C> STAT


S> +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
C> USER mrose
S> +OK User accepted
C> PASS tanstaaf
S> +OK Pass accepted


S> +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
C> USER mrose
S> +OK User accepted
C> PASS tanstaaf
S> -ERR
*/
        static IObjectExtractor _Current = new ExtractFtpPop3();
        public static IObjectExtractor Current { get { return _Current; } }

        public EExtractorReturn GetObjects(TcpStream stream, out object[] cred)
        {
            if (stream.ClientLength < 1 || (stream.Count != 1 && stream.Count < 3) || stream.ServerLength < 4)
            {
                cred = null;
                if (stream.FirstStream != null && stream.FirstStream.Emisor != ETcpEmisor.Server) return EExtractorReturn.DontRetry;
                return EExtractorReturn.Retry;
            }

            string pop3Type = null;
            bool isValidEnd = false;
            bool isValid = false, isPasswordFilled = false;
            bool isPop3 = false, isFtp = false;
            string user = null, password = null, challenge = null;

            foreach (TcpStreamMessage pack in stream)
            {
                string sp = pack.DataAscii;

                if (!isPop3 && !isFtp)
                {
                    if (pack.Emisor != ETcpEmisor.Server)
                    {
                        cred = null;
                        return EExtractorReturn.DontRetry;
                    }

                    // CHECK POP3
                    if (sp.StartsWith("+OK "))
                    {
                        isPop3 = true;
                        continue;
                    }
                    // Check FTP
                    int ix;
                    if (sp.Length >= 3 && int.TryParse(sp.Substring(0, 3), out ix) && ix >= 200 && ix <= 299)
                    {
                        isFtp = true;
                        continue;
                    }

                    cred = null;
                    return EExtractorReturn.DontRetry;
                }

                if (pack.Emisor == ETcpEmisor.Client)
                {
                    if (isPop3)
                    {
                        if (sp.StartsWith("APOP "))
                        {
                            pop3Type = "APOP";
                            user = sp.Substring(5).TrimEnd('\n', '\r');
                            int ix = user.IndexOf(' ');
                            if (ix != -1)
                            {
                                password = user.Substring(ix + 1);
                                user = user.Substring(0, ix);
                                isPasswordFilled = true;
                            }
                            continue;
                        }
                    }

                    if (sp.StartsWith("USER ")) user = sp.Substring(5).TrimEnd('\n', '\r');
                    else if (sp.StartsWith("PASS "))
                    {
                        password = sp.Substring(5).TrimEnd('\n', '\r');
                        isPasswordFilled = true;
                    }
                    else
                    {
                        if (sp.StartsWith("STLS"))
                        {
                            cred = null;
                            return EExtractorReturn.DontRetry;
                        }

                        if (isPop3)
                        {
                            if (sp.StartsWith("AUTH PLAIN")) pop3Type = "PLAIN";
                            else
                            {
                                if (sp.StartsWith("AUTH CRAM-MD5")) pop3Type = "CRAM-MD5";
                                else
                                {
                                    if (password == null)
                                    {
                                        switch (pop3Type)
                                        {
                                            case "CRAM-MD5":
                                                {
                                                    try
                                                    {
                                                        user = Encoding.ASCII.GetString(Convert.FromBase64String(sp.TrimEnd('\n', '\r'))).Trim('\0');
                                                        int ix = user.IndexOf(' ');
                                                        if (ix != -1)
                                                        {
                                                            password = user.Substring(ix + 1);
                                                            user = user.Substring(0, ix);
                                                            password += " [" + challenge + "]";
                                                            isPasswordFilled = true;
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        cred = null;
                                                        return EExtractorReturn.DontRetry;
                                                    }
                                                    break;
                                                }
                                            case "PLAIN":
                                                {
                                                    try
                                                    {
                                                        user = Encoding.ASCII.GetString(Convert.FromBase64String(sp.TrimEnd('\n', '\r'))).Trim('\0');
                                                        int ix = user.IndexOf('\0');
                                                        if (ix != -1)
                                                        {
                                                            password = user.Substring(ix + 1);
                                                            user = user.Substring(0, ix);
                                                            isPasswordFilled = true;
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        cred = null;
                                                        return EExtractorReturn.DontRetry;
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (isPasswordFilled)
                    {
                        if (isPop3)
                        {
                            isValid = sp.StartsWith("+OK ");
                        }
                        else
                        {
                            int ix;
                            if (sp.Length >= 3 && int.TryParse(sp.Substring(0, 3), out ix))
                                isValid = (ix >= 200 && ix <= 299);
                        }

                        isValidEnd = true;
                        break;
                    }
                    else
                    {
                        if (isFtp)
                        {
                            int ix;
                            if (sp.Length >= 3 && int.TryParse(sp.Substring(0, 3), out ix))
                                isValidEnd = (ix >= 500 && ix <= 599);
                        }
                        else
                        {
                            if (isPop3)
                            {
                                if (pop3Type == "CRAM-MD5")
                                {
                                    try
                                    {
                                        challenge = sp.TrimEnd('\n', '\r');
                                        int ix = challenge.IndexOf(' ');
                                        if (ix != -1)
                                        {
                                            challenge = challenge.Substring(ix + 1);
                                            //user = Encoding.ASCII.GetString(Convert.FromBase64String(challenge));
                                        }
                                    }
                                    catch
                                    {
                                        cred = null;
                                        return EExtractorReturn.DontRetry;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (isValidEnd)
            {
                if (isPop3)
                {
                    cred = new Credential[] { new Pop3Credential(stream.StartDate,stream.Destination)
                        {
                            AuthType = pop3Type,
                            User = user,
                            Password = password,
                            IsValid = isValid
                        } };
                    return EExtractorReturn.True;
                }
                else
                {
                    if (isFtp)
                    {
                        cred = new Credential[] {new FTPCredential(stream.StartDate,stream.Destination)
                            {
                                User = user,
                                Password = password,
                                IsValid = isValid
                            } };
                        return EExtractorReturn.True;
                    }
                    else cred = null;
                }
            }
            else cred = null;

            return isPop3 || isFtp || stream.Count == 0 ? EExtractorReturn.Retry : EExtractorReturn.DontRetry;
        }
    }
}