using System;

namespace GoodHamburger.Web.Services;

public sealed class LoadingState
{
    private int activeRequests;

    public event Action? OnLoadingChanged;

    public bool IsLoading => activeRequests > 0;

    public void Increment()
    {
        activeRequests++;
        if (activeRequests == 1)
        {
            OnLoadingChanged?.Invoke();
        }
    }

    public void Decrement()
    {
        activeRequests = Math.Max(0, activeRequests - 1);
        if (activeRequests == 0)
        {
            OnLoadingChanged?.Invoke();
        }
    }
}
