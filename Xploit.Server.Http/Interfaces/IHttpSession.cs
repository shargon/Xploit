using System.Collections.Generic;

namespace Xploit.Server.Http.Interfaces
{
    public interface IHttpSession
    {
        string SessionID { get; }
        string this[string name] { get; set; }
        Dictionary<string, string> Variables { get; }

        void Save();
        void Clear();
    }
}