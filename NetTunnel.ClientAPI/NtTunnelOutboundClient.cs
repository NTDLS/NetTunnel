namespace NetTunnel.ClientAPI
{
    public class NtTunnelOutboundClient
    {
        /*
        private readonly NtClient _client;

        public NtTunnelOutboundClient(NtClient client)
        {
            _client = client;
        }

        public async Task<NtActionResponseTunnelsOutbound> List()
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/List";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseTunnelsOutbound>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        public async Task Add(NtTunnelOutboundConfiguration tunnel)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/Add";

            var postContent = new StringContent(JsonConvert.SerializeObject(tunnel), Encoding.UTF8, "text/plain");

            using var response = _client.Connection.PostAsync(url, postContent);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Start(Guid tunnelId)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/Start/{tunnelId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Stop(Guid tunnelId)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/Stop/{tunnelId}";

            using var response = await _client.Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        public async Task Delete(Guid tunnelId)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/Delete/{tunnelId}";

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
        /// <param name="tunnelId"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task DeletePair(Guid tunnelId)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/DeletePair/{tunnelId}";

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
        /// <param name="tunnelId"></param>
        /// <param name="tunnel"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task UpsertEndpointInboundPair(Guid tunnelId, NtEndpointConfiguration inboundEndpoint, NtEndpointOutboundConfiguration outboundEndpoint)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/UpsertEndpointInboundPair/{tunnelId}";

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
        /// <param name="tunnelId"></param>
        /// <param name="tunnel"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task UpsertEndpointOutboundPair(Guid tunnelId, NtEndpointConfiguration inboundEndpoint, NtEndpointOutboundConfiguration outboundEndpoint)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/UpsertEndpointOutboundPair/{tunnelId}";

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
        /// Delete an endpoint from the tunnel and an endpoint from the other end of the associated tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task DeleteEndpointPair(Guid tunnelId, Guid endpointId)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/DeleteEndpointPair/{tunnelId}/{endpointId}";

            using var response = _client.Connection.GetAsync(url);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }

        /// <summary>
        /// Delete an endpoint from the tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        /// <exception cref="NtAPIResponseException"></exception>
        public async Task DeleteEndpoint(Guid tunnelId, Guid endpointId)
        {
            string url = $"api/TunnelOutbound/{_client.SessionId}/DeleteEndpoint/{tunnelId}/{endpointId}";

            using var response = _client.Connection.GetAsync(url);
            string resultText = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponse>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }
        }
        */
    }
}
