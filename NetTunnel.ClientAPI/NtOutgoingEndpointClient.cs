using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Types;
using Newtonsoft.Json;
using System.Text;

namespace NetTunnel.ClientAPI
{
    public class NtOutgoingEndpointClient
    {
        private readonly NtClient _client;

        public NtOutgoingEndpointClient(NtClient client)
        {
            _client = client;
        }

        public async Task<NtActionResponseOutgoingEndpoints> List()
        {
            string url = $"api/OutgoingEndpoint/{_client.SessionId}/List";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseOutgoingEndpoints>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task Add(NtOutgoingEndpoint endpoint)
        {
            string url = $"api/OutgoingEndpoint/{_client.SessionId}/Add";

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
