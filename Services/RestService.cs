using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace tardis.Services
{
    internal class RestService
    {
        private static readonly HttpClient client = new HttpClient();
        private IConfiguration _config;

        public RestService(IConfiguration config)
        {
            this._config = config;
        }

        public async Task<string> GetNeighborsAsync(string hash)
        {
            var responseString = await client.GetStringAsync(_config["ServerSettings:restApiURL"] + "/" + hash + ".json");

            return responseString;
        }

        public async Task<string> GetGroupAndNodeAsync(string groupId, string nodeName)
        {
            var responseString = await client.GetStringAsync(_config["ServerSettings:restApiURL"] + "/group/" + groupId + "/node/" + nodeName);

            return responseString;
        }

        public async Task<string> UpdateNodeStatusAsync(string groupId, string nodeName, string status)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _config["ServerSettings:restApiURL"] + "/group/" + groupId + "/node/" + nodeName + "/status/" + status);
            var response = await client.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }
    }
}