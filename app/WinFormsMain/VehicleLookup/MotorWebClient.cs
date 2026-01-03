using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WinFormsMain.VehicleLookup
{
    public class MotorWebClient
    {
        private readonly HttpClient _httpClient;
        private readonly MotorWebOptions _options;
        private readonly ILogger<MotorWebClient> _logger;

        public MotorWebClient(HttpClient httpClient, IOptions<MotorWebOptions> options, ILogger<MotorWebClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        public async Task<MotorWebResponse?> LookupAsync(string rego, string state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rego))
            {
                throw new ArgumentException("Rego is required", nameof(rego));
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException("State is required", nameof(state));
            }

            var requestUri = new Uri($"lookup?plate={WebUtility.UrlEncode(rego)}&state={WebUtility.UrlEncode(state)}", UriKind.Relative);

            try
            {
                using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("MotorWeb lookup returned 404 for {Rego} ({State})", rego, state);
                    return null;
                }

                if (response.StatusCode == (HttpStatusCode)429)
                {
                    _logger.LogWarning("MotorWeb rate limit reached for {Rego} ({State})", rego, state);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var payload = await JsonSerializer.DeserializeAsync<MotorWebResponse>(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return payload;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "MotorWeb lookup timed out for {Rego} ({State})", rego, state);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "MotorWeb lookup failed for {Rego} ({State})", rego, state);
                return null;
            }
        }
    }
}
