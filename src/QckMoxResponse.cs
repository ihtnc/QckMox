using System.Collections.Generic;
using System.IO;
using QckMox.Configs;

namespace QckMox
{
    public class QckMoxResponse
    {
        public QckMoxResponseResult Result { get; set; }

        public QckMoxResponseData Data { get; set; }

        public static QckMoxResponse RedirectResponse => new QckMoxResponse
        {
            Result = QckMoxResponseResult.Redirect,
            Data = null
        };

        public static QckMoxResponse GetErrorResponse(QckMoxResponseFileConfig config) => new QckMoxResponse
        {
            Result = QckMoxResponseResult.Error,
            Data = new QckMoxResponseData
            {
                Headers = config.Headers
            }
        };

        public static QckMoxResponse GetNotFoundResponse(QckMoxResponseFileConfig config) => new QckMoxResponse
        {
            Result = QckMoxResponseResult.NotFound,
            Data = new QckMoxResponseData
            {
                Headers = config.Headers
            }
        };

        public static QckMoxResponse GetSuccessResponse(Stream content, QckMoxResponseFileConfig config) => new QckMoxResponse
        {
            Result = QckMoxResponseResult.Success,
            Data = new QckMoxResponseData
            {
                ContentType = config.ContentType,
                Headers = config.Headers,
                Content = content
            }
        };
    }

    public class QckMoxResponseData
    {
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Stream Content { get; set; }
    }
}