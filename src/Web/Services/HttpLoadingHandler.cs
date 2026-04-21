using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoodHamburger.Web.Services;

public sealed class HttpLoadingHandler : DelegatingHandler
{
    private readonly LoadingState loadingState;

    public HttpLoadingHandler(LoadingState loadingState)
    {
        this.loadingState = loadingState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            loadingState.Increment();
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            loadingState.Decrement();
        }
    }
}
