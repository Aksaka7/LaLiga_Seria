using System.Net;

namespace LaLiga.WebUI.Services
{
    // API'nin döndürdüğü (ya da API'ye ulaşılamadığı için oluşan) hatayı taşır.

    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ApiException(HttpStatusCode statusCode, string message, IReadOnlyDictionary<string, string[]>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
