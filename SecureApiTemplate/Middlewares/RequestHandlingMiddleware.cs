using Newtonsoft.Json;
using SecureApiTemplate.Models.Core;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace SecureApiTemplate.Middlewares
{
    public class RequestHandlingMiddleware
    {
        #region Fields

        private readonly RequestDelegate requestDelegate;

        #endregion

        #region CTOR

        public RequestHandlingMiddleware(RequestDelegate requestDelegate)
        {
            this.requestDelegate = requestDelegate;
        }

        #endregion

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

            var logInfo = new LogInfo()
            {
                TraceId = traceId,
                CurlCommand = await GenerateCurlCommandAsync(context)
            };

            Log.Information($"{JsonConvert.SerializeObject(logInfo)}");

            try
            {
                await requestDelegate(context);

                if (context.Response.StatusCode >= 400 && context.Response.StatusCode <= 499)
                {
                    var errorResponse = new ErrorInfo()
                    {
                        TraceId = traceId,
                        Status = context.Response.StatusCode,
                        Title = "Bad Request",
                        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
                    };

                    Log.Error($"{JsonConvert.SerializeObject(errorResponse)}");
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, traceId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
        {
            var errorResponse = new ErrorInfo
            {
                TraceId = traceId,
                Status = StatusCodes.Status500InternalServerError,
                Title = ex.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            };

            Log.Error($"{JsonConvert.SerializeObject(errorResponse)}");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
        }

        private static async Task<string> GenerateCurlCommandAsync(HttpContext context)
        {
            var request = context.Request;

            string method = request.Method;
            string url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            #region Headers

            var headersBuilder = new StringBuilder();

            foreach (var header in request.Headers)
            {
                if (header.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase) && request.ContentLength == 0)
                    continue;

                headersBuilder.Append($" --header '{header.Key}: {header.Value}' \\\n");
            }

            string headers = headersBuilder.ToString();
            string curlCommand = $"curl --location --request {method} '{url}' \\\n{headers}";

            #endregion

            #region Body

            if (request.ContentLength > 0 && (method.Equals("PUT", StringComparison.OrdinalIgnoreCase) || method.Equals("POST", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    request.EnableBuffering();
                }
                catch (InvalidOperationException)
                {
                    Log.Warning("Could not enable buffering for the request.");
                    return curlCommand;
                }

                if (request.HasJsonContentType())
                {
                    var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                    await request.Body.ReadAsync(buffer, 0, buffer.Length);
                    var jsonBody = Encoding.UTF8.GetString(buffer);

                    if (!string.IsNullOrEmpty(jsonBody))
                    {
                        curlCommand += $" --data '{jsonBody}'";
                    }

                    request.Body.Position = 0L;
                }
                else if (request.HasFormContentType)
                {
                    long index = 0;
                    foreach (var (key, value) in request.Form)
                    {
                        if (index != 0)
                        {
                            curlCommand += "\\\n";
                        }
                        curlCommand += $"--form '{key}=\"{value}\"'";
                        index++;
                    }
                }
            }

            #endregion

            return curlCommand;
        }
    }

}
