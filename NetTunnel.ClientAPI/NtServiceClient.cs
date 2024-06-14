using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Exceptions;
using NetTunnel.Library.Types;
using Newtonsoft.Json;
using System.Text;

namespace NetTunnel.ClientAPI
{
    public class oldNtServiceClient
    {
        private readonly NtClient _client;

        public oldNtServiceClient(NtClient client)
        {
            _client = client;
        }

        public async Task<NtActionResponseStatistics> GetStatistics()
        {
            string url = $"api/Service/{_client.SessionId}/GetStatistics";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseStatistics>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task<NtActionResponseServiceConfiguration> GetConfiguration()
        {
            string url = $"api/Service/{_client.SessionId}/GetConfiguration";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseServiceConfiguration>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task PutConfiguration(NtServiceConfiguration tunnel)
        {
            string url = $"api/Service/{_client.SessionId}/PutConfiguration";

            var postContent = new StringContent(JsonConvert.SerializeObject(tunnel), Encoding.UTF8, "text/plain");

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

