using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Payload.Response;
using Newtonsoft.Json;

namespace NetTunnel.ClientAPI.Management
{
    public class NtSecurityClient
    {
        private readonly NtClient _client;

        public NtSecurityClient(NtClient client)
        {
            _client = client;
        }

        public void Login(string username, string passwordHash)
        {
            string url = $"api/Security/{username}/{passwordHash}/Login";

            using var response = _client.Connection.GetAsync(url);
            string resultText = response.Result.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<NtActionResponseLogin>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            _client.SessionId = result.SessionId;
        }

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
