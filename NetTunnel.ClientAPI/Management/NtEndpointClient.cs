using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Payload;
using Newtonsoft.Json;

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

        /*
        public void Store(string username, string passwordHash)
        {
            string url = $"api/Configuration/{username}/{passwordHash}/Login";

            var postContent = new StringContent(JsonConvert.SerializeObject(document), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = response.Result.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new APIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }         
        */

        public void Logout()
        {
            string url = $"api/Security/{_client.SessionId}/Logout";

            using var response = _client.Connection.GetAsync(url);
            string resultText = response.Result.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }
    }
}
