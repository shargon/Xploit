using XPloit.Server.Http.Enums;

namespace XPloit.Server.Http.Interfaces
{
    public interface IHttpServer
    {
        uint MaxPost { get; }
        uint MaxPostMultiPart { get; }
        bool ProgreesOnAllPost { get; }
        bool SessionCheckFakeId { get; }
        string SessionCookieName { get; }
        string SessionDirectory { get; }
        bool AllowKeepAlive { get; }
        bool GetAndPostNamesToLowerCase { get; }

        void PushDel(HttpProcessor httpProcessor, string push_code);
        void OnPostProgress(HttpPostProgress post_pg, EHttpPostState start);
        bool PushAdd(HttpProcessor httpProcessor, string push_code);
        void OnRequest(HttpProcessor httpProcessor);
        void OnRequestSocket(HttpProcessor httpProcessor);
    }
}