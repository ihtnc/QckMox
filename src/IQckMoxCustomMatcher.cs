using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QckMox
{
    public interface IQckMoxCustomMatcher
    {
        Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig config);
    }
}