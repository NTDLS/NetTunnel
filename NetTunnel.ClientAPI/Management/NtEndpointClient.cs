using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.EndPoint;
using NetTunnel.Library.Types;
using Newtonsoft.Json;
using System.Text;

namespace NetTunnel.ClientAPI.Management
{
    public class NtEndpointClient
    {
        private readonly NtClient _client;

        public NtEndpointClient(NtClient client)
        {
            _client = client;
        }

        public async Task<NtActionResponseEndpoints> List()
        {
            string url = $"api/Endpoint/{_client.SessionId}/List";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseEndpoints>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task AddOutgoing(NtEndpoint endpoint)
        {
            string url = $"api/Endpoint/{_client.SessionId}/AddOutgoing";

            var postContent = new StringContent(JsonConvert.SerializeObject(endpoint), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task AddIncomming(NtIncommingEndpoint endpoint)
        {
            string url = $"api/Endpoint/{_client.SessionId}/AddOutgoing";

            var postContent = new StringContent(JsonConvert.SerializeObject(endpoint), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }
    }
}
