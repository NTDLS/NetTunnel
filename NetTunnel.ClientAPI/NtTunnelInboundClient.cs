using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Exceptions;
using NetTunnel.Library.Types;
using Newtonsoft.Json;
using System.Text;

namespace NetTunnel.ClientAPI
{
    public class NtTunnelInboundClient
    {
        private readonly NtClient _client;

        public NtTunnelInboundClient(NtClient client)
        {
            _client = client;
        }

        public async Task<NtActionResponseTunnelsInbound> List()
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/List";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseTunnelsInbound>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task Add(NtTunnelInboundConfiguration tunnel)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/Add";

            var postContent = new StringContent(JsonConvert.SerializeObject(tunnel), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Start(Guid tunnelPairId)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/Start/{tunnelPairId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Stop(Guid tunnelPairId)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/Stop/{tunnelPairId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Delete(Guid tunnelPairId)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/Delete/{tunnelPairId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        /// <summary>
        /// Deletes a tunnel and notified the associated-service tunnel to delete itself as well.
        /// </summary>
        public async Task DeletePair(Guid tunnelPairId)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/DeletePair/{tunnelPairId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        /// <summary>
        /// Add a inbound endpoint to the tunnel and an outbound endpoint to the other end of the associated tunnel.
        /// </summary>
        /// <param name="tunnelPairId"></param>
        /// <param name="tunnel"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task AddEndpointInboundPair(Guid tunnelPairId, NtEndpointInboundConfiguration inboundEndpoint, NtEndpointOutboundConfiguration outboundEndpoint)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/AddEndpointInboundPair/{tunnelPairId}";

            var postContent = new StringContent(JsonConvert.SerializeObject(
                new NtEndpointPairConfiguration(inboundEndpoint, outboundEndpoint)), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        /// <summary>
        /// Add a outbound endpoint to the tunnel and an inbound endpoint to the other end of the associated tunnel.
        /// </summary>
        /// <param name="tunnelPairId"></param>
        /// <param name="tunnel"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task AddEndpointOutboundPair(Guid tunnelPairId, NtEndpointInboundConfiguration inboundEndpoint, NtEndpointOutboundConfiguration outboundEndpoint)
        {
            string url = $"api/TunnelInbound/{_client.SessionId}/AddEndpointOutboundPair/{tunnelPairId}";

            var postContent = new StringContent(JsonConvert.SerializeObject(
                new NtEndpointPairConfiguration(inboundEndpoint, outboundEndpoint)), Encoding.UTF8, "text/plain");

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
