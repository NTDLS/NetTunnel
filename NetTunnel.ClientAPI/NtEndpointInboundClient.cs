using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Payload;
using Newtonsoft.Json;

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
    }
}
