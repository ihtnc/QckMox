using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QckMox.Matcher
{
    internal interface IQckMoxMatchResultHandler
    {
        Task<QckMoxMatchResult> MatchResponse(HttpRequest request);
    }
}