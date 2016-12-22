namespace Xploit.Server.Http
{
    public class HttpCookie
    {
        string name, value;
        public string Name { get { return name; } }
        public string Value { get { return value; } }

        public HttpCookie(string n, string v) { name = n; value = v; }
    }
}