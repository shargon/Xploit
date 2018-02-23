using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace XPloit.Core.Helpers
{
    public class LatchHelper
    {
        public class Config
        {
            string _SecretId, _SecretKey, _UserToken;

            /// <summary>
            /// App Id
            /// </summary>
            public string Id { get { return _SecretId; } set { _SecretId = value; } }
            /// <summary>
            /// Secret Key
            /// </summary>
            public string Key { get { return _SecretKey; } set { _SecretKey = value; } }
            /// <summary>
            /// Token de usuario
            /// </summary>
            public string Token { get { return _UserToken; } set { _UserToken = value; } }
            /// <summary>
            /// Devuelve si es válida
            /// </summary>
            public bool IsValid() { return !string.IsNullOrEmpty(_SecretId) && !string.IsNullOrEmpty(_SecretKey) && !string.IsNullOrEmpty(_UserToken); }
            /// <summary>
            /// Devuelve si es válido y además es un Token de pareado
            /// </summary>
            public bool IsLatched() { return IsValid() && _UserToken.Length > 20; }
            /// <summary>
            /// Obtiene el Manager o NULL
            /// </summary>
            public LatchHelper GetManager()
            {
                if (IsValid()) return new LatchHelper(_SecretId, _SecretKey);
                return null;
            }
        }

        class StatusConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) { return objectType == typeof(string); }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string target = null;

                JObject jObject = JObject.Load(reader);
                if (jObject != null)
                {
                    JToken jt = jObject.First;//.GetItem(0);
                    if (jt != null)
                    {
                        Dictionary<string, string> st = jt.First.ToObject<Dictionary<string, string>>();

                        if (st != null && st.TryGetValue("status", out target))
                            return target;
                    }
                }

                return target;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
        }
        class LatchData
        {
            public string accountId { get; set; }
            [JsonProperty(PropertyName = "operations"), JsonConverter(typeof(StatusConverter))]
            public string status { get; set; }
        }
        class LatchResponse
        {
            public LatchData Data { get; set; }
            public Error Error { get; set; }

            public LatchResponse() { }
        }

        class Error
        {
            public int Code { get; set; }
            public string Message { get; set; }
            public override string ToString() { return this.Code.ToString() + " - " + Message; }
        }

        const string API_VERSION = "0.9";
        const string API_HOST = "https://latch.elevenpaths.com";

        const string API_CHECK_STATUS_URL = "/api/" + API_VERSION + "/status";
        const string API_PAIR_URL = "/api/" + API_VERSION + "/pair";
        const string API_UNPAIR_URL = "/api/" + API_VERSION + "/unpair";

        const string AUTHORIZATION_HEADER_NAME = "Authorization";
        const string DATE_HEADER_NAME = "X-11Paths-Date";
        const string AUTHORIZATION_METHOD = "11PATHS";
        const char AUTHORIZATION_HEADER_FIELD_SEPARATOR = ' ';

        const string UTC_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss";

        const string X_11PATHS_HEADER_PREFIX = "X-11paths-";
        const char X_11PATHS_HEADER_SEPARATOR = ':';
        const char PARAM_SEPARATOR = '&';
        const char PARAM_VALUE_SEPARATOR = '=';

        string _Id, _Key;

        /// <summary>
        /// App Id
        /// </summary>
        public string Id { get { return _Id; } set { _Id = value; } }
        /// <summary>
        /// Secret Key
        /// </summary>
        public string Key { get { return _Key; } set { _Key = value; } }

        enum HttpMethod { GET, POST, PUT, DELETE }
        enum FeatureMode { MANDATORY, OPT_IN, DISABLED }

        /// <summary>
        /// Creates an instance of the class with the <code>Application ID</code> and <code>Secret</code> obtained from Eleven Paths 
        /// </summary>
        public LatchHelper() { }
        /// <summary>
        /// Creates an instance of the class with the <code>Application ID</code> and <code>Secret</code> obtained from Eleven Paths 
        /// </summary>
        public LatchHelper(string appId, string secretKey)
        {
            this._Id = appId;
            this._Key = secretKey;
        }
        /// <summary>
        /// Performs an HTTP request to an URL using the specified method, headers and data, returning the response as a string
        /// </summary>
        static string HttpPerformRequest(string url, HttpMethod method, IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();

            foreach (string key in headers.Keys)
            {
                if (key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, headers[key]);
                }
                else if (key.Equals("Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Date = DateTime.Parse(headers[key], null, System.Globalization.DateTimeStyles.AssumeUniversal);
                }
                else
                {
                    request.Headers.Add(key, headers[key]);
                }
            }

            try
            {
                if (method == HttpMethod.POST || method == HttpMethod.PUT)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                    {
                        sw.Write(GetSerializedParams(data));
                        sw.Flush();
                    }
                }

                using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Performs an HTTP request to an URL using the specified method and data, returning the response as a string
        /// </summary>
        LatchResponse HttpPerformRequest(string url, HttpMethod method = HttpMethod.GET, IDictionary<string, string> data = null)
        {
            try
            {
                IDictionary<string, string> authHeaders = AuthenticationHeaders(method.ToString(), url, null, data);
                string json = HttpPerformRequest(API_HOST + url, method, authHeaders, data);

                LatchResponse r = JsonHelper.Deserialize<LatchResponse>(json);
                //    , new JsonSerializerSettings()
                //{
                //    Converters = new List<JsonConverter> { new StatusConverter() }
                //});
                return r;
            }
            catch { }
            return null;
        }
        /// <summary>
        /// Pairs an account using a token
        /// </summary>
        /// <param name="token">Pairing token obtained by the user from the mobile application</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the AccountID</returns>
        public string Pair(string token)
        {
            LatchResponse r = HttpPerformRequest(API_PAIR_URL + "/" + UrlEncode(token));
            return r != null && r.Data != null ? r.Data.accountId : null;
        }
        /// <summary>
        /// Requests the status of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the status of the account</returns>
        public bool Status(string accountId)
        {
            LatchResponse l = HttpPerformRequest(API_CHECK_STATUS_URL + "/" + UrlEncode(accountId));
            return l != null && l.Data != null && l.Data.status != null && l.Data.status.ToLower() == "on";
        }
        /// <summary>
        /// Unpairs the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, an empty response</returns>
        public bool Unpair(string accountId)
        {
            if (string.IsNullOrEmpty(accountId)) return false;

            LatchResponse l = HttpPerformRequest(API_UNPAIR_URL + "/" + UrlEncode(accountId));
            return l != null && (l.Error == null || (l.Error != null && l.Error.Code == 201));
        }
        /// <summary>
        /// Signs the data provided in order to prevent tampering
        /// </summary>
        /// <param name="data">The string to sign</param>
        /// <returns>Base64 encoding of the HMAC-SHA1 hash of the data parameter using <code>secretKey</code> as cipher key.</returns>              
        string SignData(string data)
        {
            try
            {
                HMACSHA1 hmacSha1 = new HMACSHA1(Encoding.ASCII.GetBytes(_Key));
                return Convert.ToBase64String(hmacSha1.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Calculates the authentication headers to be sent with a request to the API 
        /// </summary>
        /// <param name="httpMethod">The HTTP Method. Currently only GET is supported</param>
        /// <param name="queryString">The urlencoded string including the path (from the first forward slash) and the parameters</param>
        /// <param name="xHeaders">HTTP headers specific to the 11-paths API. Null if not needed.</param>
        /// <returns>An IDictionary with the Authorization and Date headers needed to sign a Latch API request</returns>
        IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> parameters)
        {
            return AuthenticationHeaders(httpMethod, queryString, xHeaders, parameters, GetCurrentUTC());
        }
        /// <summary>
        /// Calculates the authentication headers to be sent with a request to the API 
        /// </summary>
        /// <param name="httpMethod">The HTTP Method. Currently only GET is supported</param>
        /// <param name="queryString">The urlencoded string including the path (from the first forward slash) and the parameters</param>
        /// <param name="xHeaders">HTTP headers specific to the 11-paths API. Null if not needed.</param>
        /// <param name="utc">The Universal Coordinated Time for the Date HTTP header</param>
        /// <returns>An IDictionary with the Authorization and Date headers needed to sign a Latch API request</returns>
        IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> parameters, string utc)
        {
            StringBuilder stringToSign = new StringBuilder()
                .Append(httpMethod.ToUpper().Trim()).Append("\n")
                .Append(utc).Append("\n")
                .Append(GetSerializedHeaders(xHeaders)).Append("\n")
                .Append(queryString.Trim());

            if (parameters != null && parameters.Count > 0)
            {
                string serializedParams = GetSerializedParams(parameters);
                if (!string.IsNullOrEmpty(serializedParams))
                {
                    stringToSign.Append("\n").Append(serializedParams);
                }
            }

            string signedData = SignData(stringToSign.ToString());
            string authorizationHeader = new StringBuilder(AUTHORIZATION_METHOD)
                .Append(AUTHORIZATION_HEADER_FIELD_SEPARATOR)
                .Append(this._Id)
                .Append(AUTHORIZATION_HEADER_FIELD_SEPARATOR)
                .Append(signedData)
                .ToString();

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(AUTHORIZATION_HEADER_NAME, authorizationHeader);
            headers.Add(DATE_HEADER_NAME, utc);
            return headers;
        }
        /// <summary>
        /// Prepares and returns a string ready to be signed from the 11-paths specific HTTP headers received 
        /// </summary>
        /// <param name="xHeaders">A non necessarily sorted IDictionary of the HTTP headers</param>
        /// <returns>A string with the serialized headers, an empty string if no headers are passed, or null if there's a problem
        ///  such as non specific 11paths headers</returns>
        static string GetSerializedHeaders(IDictionary<string, string> xHeaders)
        {
            if (xHeaders != null && xHeaders.Count > 0)
            {
                SortedDictionary<string, string> sorted = new SortedDictionary<string, string>();

                foreach (string key in xHeaders.Keys)
                {
                    if (!key.StartsWith(X_11PATHS_HEADER_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ApplicationException("Error serializing headers. Only specific " + X_11PATHS_HEADER_PREFIX + " headers need to be signed");
                    }
                    sorted.Add(key.ToLower(), xHeaders[key].Replace('\n', ' '));
                }

                StringBuilder serializedHeaders = new StringBuilder();
                foreach (string key in sorted.Keys)
                {
                    serializedHeaders.Append(key).Append(X_11PATHS_HEADER_SEPARATOR).Append(sorted[key]).Append(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
                }

                return serializedHeaders.ToString().Trim(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Prepares and returns a string ready to be signed from the parameters of an HTTP request
        /// </summary>
        /// <param name="parameters">A non necessarily sorted IDictionary of the parameters</param>
        /// <returns>A string with the serialized parameters, an empty string if no headers are passed</returns>
        /// <remarks> The params must be only those included in the body of the HTTP request when its content type
        ///     is application/x-www-urlencoded and must be urldecoded. </remarks>
        static string GetSerializedParams(IDictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                SortedDictionary<string, string> sorted = new SortedDictionary<string, string>(parameters);

                StringBuilder serializedParams = new StringBuilder();
                foreach (string key in sorted.Keys)
                {
                    serializedParams.Append(UrlEncode(key)).Append(PARAM_VALUE_SEPARATOR)
                                    .Append(UrlEncode(sorted[key])).Append(PARAM_SEPARATOR);
                }

                return serializedParams.ToString().Trim(PARAM_SEPARATOR);
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Returns a string representation of the current time in UTC to be used in a Date HTTP Header
        /// </summary>
        static string GetCurrentUTC() { return DateTime.UtcNow.ToString(UTC_STRING_FORMAT); }
        /// <summary>
        /// Encodes a string to be passed as an URL parameter in UTF-8
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static string UrlEncode(string value) { return HttpUtility.UrlEncode(value, Encoding.UTF8); }
        /// <summary>
        /// Clona el objeto
        /// </summary>
        /// <returns>Devuelve el LatchManager</returns>
        public LatchHelper Clone() { return new LatchHelper(this._Id, this._Key); }
    }
}