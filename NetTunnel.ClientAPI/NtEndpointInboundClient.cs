using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Types;
using Newtonsoft.Json;
using System.Text;

namespace NetTunnel.ClientAPI
{
    public class NtEndpointInboundClient
    {
        private readonly NtClient _client;

        public NtEndpointInboundClient(NtClient client)
        {
            _client = client;
        }

        public async Task<NtActionResponseEndpointsInbound> List(Guid tunnelPairId)
        {
            string url = $"api/EndpointInbound/{_client.SessionId}/List/{tunnelPairId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseEndpointsInbound>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task Add(Guid tunnelPairId, NtEndpointInboundConfiguration tunnel)
        {
            string url = $"api/EndpointInbound/{_client.SessionId}/Add/{tunnelPairId}";

            var postContent = new StringContent(JsonConvert.SerializeObject(tunnel), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Delete(Guid tunnelPairId, Guid endpointPairId)
        {
            string url = $"api/EndpointInbound/{_client.SessionId}/Delete/{tunnelPairId}/{endpointPairId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }
    }
}
