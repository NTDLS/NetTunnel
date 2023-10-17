using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.Library.Payloads;
using Newtonsoft.Json;

namespace NetTunnel.ClientAPI.Management
{
    public class ConfigurationClient
    {
        private readonly Client _client;

        public ConfigurationClient(Client client)
        {
            _client = client;
        }

        public void Login(string username, string passwordHash)
        {
            string url = $"api/Configuration/{username}/{passwordHash}/Login";

            using var response = _client.Connection.GetAsync(url);
            string resultText = response.Result.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ActionResponseLogin>(resultText);
            if (result == null || result.Success == false)
            {
                throw new APIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            _client.SessionId = result.SessionId;
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
            string url = $"api/Configuration/{_client.SessionId}/Logout";

            using var response = _client.Connection.GetAsync(url);
            string resultText = response.Result.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new APIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }
    }
}
