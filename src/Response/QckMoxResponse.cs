using System.Collections.Generic;
using System.IO;

namespace QckMox.Response
{
    internal class QckMoxResponse
    {
        public QckMoxResponseResult Result { get; set; }

        public QckMoxMatchResult Data { get; set; }

        public static QckMoxResponse RedirectResponse => new QckMoxResponse
        {
            Result = QckMoxResponseResult.Redirect,
            Data = null
        };

        public static QckMoxResponse GetErrorResponse(IReadOnlyDictionary<string, string> headers) => new QckMoxResponse
        {
            Result = QckMoxResponseResult.Error,
            Data = new QckMoxMatchResult
            {
                ResponseHeaders = new Dictionary<string, string>(headers)
            }
        };

        public static QckMoxResponse GetNotFoundResponse(IReadOnlyDictionary<string, string> headers) => new QckMoxResponse
        {
            Result = QckMoxResponseResult.NotFound,
            Data = new QckMoxMatchResult
            {
                ResponseHeaders = new Dictionary<string, string>(headers)
            }
        };

        public static QckMoxResponse GetSuccessResponse(Stream content, string contentType, IReadOnlyDictionary<string, string> headers) => new QckMoxResponse
        {
            Result = QckMoxResponseResult.Success,
            Data = new QckMoxMatchResult
            {
                ContentType = contentType,
                ResponseHeaders = new Dictionary<string, string>(headers),
                Content = content
            }
        };
    }
}