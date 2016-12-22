namespace Xploit.Server.Http
{
    public class HttpAuth
    {
        string _user, _password;

        public string User { get { return _user; } }
        public string Password { get { return _password; } }

        public HttpAuth(string user, string password) { _user = user; _password = password; }

        public bool IsEqual(string user, string pwd) { return _user == user && _password == pwd; }

        public override string ToString() { return _user + ":" + _password; }
    }
}