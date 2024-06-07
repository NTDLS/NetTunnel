using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Exceptions;
using NetTunnel.Service;
using Newtonsoft.Json;
using System.Text;

namespace NetTunnel.ClientAPI
{
    public class NtSecurityClient
    {
        private readonly NtClient _client;

        public NtSecurityClient(NtClient client)
        {
            _client = client;
        }

        public async Task Login(string username, string passwordHash)
        {
            string url = $"api/Security/{username}/{passwordHash}/Login";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
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

        public async Task<NtActionResponseUsers> ListUsers()
        {
            string url = $"api/Security/{_client.SessionId}/ListUsers";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseUsers>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task ChangeUserPassword(NtUser user)
        {
            string url = $"api/Security/{_client.SessionId}/ChangeUserPassword";

            var postContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task DeleteUser(NtUser user)
        {
            string url = $"api/Security/{_client.SessionId}/DeleteUser";

            var postContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task CreateUser(NtUser user)
        {
            string url = $"api/Security/{_client.SessionId}/CreateUser";

            var postContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "text/plain");

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

