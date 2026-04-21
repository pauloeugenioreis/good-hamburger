using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;

namespace GoodHamburger.Web.Services;

public abstract class BaseApiClient
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    protected readonly HttpClient HttpClient;

    protected BaseApiClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected async Task<ApiResult<TResponse>> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        object? request = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendRequestAsync(method, path, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<TResponse>.Failure(await ReadErrorAsync(response, cancellationToken), response.StatusCode);
        }

        if (response.Content.Headers.ContentLength == 0 && response.StatusCode == HttpStatusCode.NoContent)
        {
            return ApiResult<TResponse>.Failure("A resposta da API não continha conteúdo.", response.StatusCode);
        }

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);

        return payload is null
            ? ApiResult<TResponse>.Failure("Não foi possível desserializar a resposta da API.", response.StatusCode)
            : ApiResult<TResponse>.Success(payload, response.StatusCode);
    }

    protected async Task<ApiResult<IReadOnlyList<TItem>>> SendListAsync<TItem>(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendRequestAsync(method, path, null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<IReadOnlyList<TItem>>.Failure(await ReadErrorAsync(response, cancellationToken), response.StatusCode);
        }

        var payload = await response.Content.ReadFromJsonAsync<List<TItem>>(JsonOptions, cancellationToken);

        return payload is null
            ? ApiResult<IReadOnlyList<TItem>>.Failure("Não foi possível desserializar a lista da API.", response.StatusCode)
            : ApiResult<IReadOnlyList<TItem>>.Success(payload, response.StatusCode);
    }

    protected async Task<ApiResult<bool>> SendNoContentAsync(
        HttpMethod method,
        string path,
        object? request = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendRequestAsync(method, path, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<bool>.Failure(await ReadErrorAsync(response, cancellationToken), response.StatusCode);
        }

        return ApiResult<bool>.Success(true, response.StatusCode);
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method,
        string path,
        object? request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path);

        if (request is not null)
        {
            message.Content = JsonContent.Create(request, options: JsonOptions);
        }

        return await HttpClient.SendAsync(message, cancellationToken);
    }

    protected static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(errorContent))
        {
            return $"A API retornou {(int)response.StatusCode} ({response.StatusCode}).";
        }

        try
        {
            // Try to parse as ProblemDetails or standard validation JSON
            using var document = JsonDocument.Parse(errorContent);
            var root = document.RootElement;

            // Look for "errors" dictionary (FluentValidation/ModelState)
            if (root.TryGetProperty("errors", out var errorsProperty) && errorsProperty.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var property in errorsProperty.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var error in property.Value.EnumerateArray())
                        {
                            messages.Add(error.GetString() ?? "");
                        }
                    }
                }

                if (messages.Count > 0)
                {
                    return string.Join(" | ", messages.Where(m => !string.IsNullOrWhiteSpace(m)));
                }
            }

            // Look for "detail" property (standard ProblemDetails)
            if (root.TryGetProperty("detail", out var detailProperty))
            {
                return detailProperty.GetString() ?? errorContent;
            }

            // Look for "message" property
            if (root.TryGetProperty("message", out var messageProperty))
            {
                return messageProperty.GetString() ?? errorContent;
            }

            // Fallback: If it's an object and we haven't returned yet, check if it's a flat dictionary of errors
            if (root.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var error in property.Value.EnumerateArray())
                        {
                            messages.Add(error.GetString() ?? "");
                        }
                    }
                    else if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        messages.Add(property.Value.GetString() ?? "");
                    }
                }

                if (messages.Count > 0)
                {
                    return string.Join(" | ", messages.Where(m => !string.IsNullOrWhiteSpace(m)));
                }
            }
        }
        catch
        {
            // Fallback to raw content if JSON parsing fails
        }

        return errorContent;
    }

    protected static string BuildPath(string basePath, IReadOnlyDictionary<string, string?> query)
    {
        var builder = new StringBuilder(basePath);
        var hasQuery = false;

        foreach (var (key, value) in query)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.Append(hasQuery ? '&' : '?');
            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
            hasQuery = true;
        }

        return builder.ToString();
    }
}
