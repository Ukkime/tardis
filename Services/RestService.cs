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

        public RestService(IConfiguration config) {
            this._config = config;
        }

        public async Task<string> GetNeighborsAsync(string hash)
        {
            /*
                 {
                    "Name": "Grupo de Trabajo 1",
                    "NodeCount": 3,
                    "Id": "5e884898da280f36e7c310dd233371204884883bfe2a5094b5e3b3ebc3d60f20",
                    "NeighborNodes": [
                      {
                        "Name": "Santi",
                        "LastCommunication": "2024-01-17T13:26:45Z",
                        "Status": "Disponible"
                      },
                      {
                        "Name": "Ruben",
                        "LastCommunication": "2024-01-16T15:30:00Z",
                        "Status": "Concentrado"
                      },
                      {
                        "Name": "Quique",
                        "LastCommunication": "2024-01-17T12:00:00Z",
                        "Status": "Ocupado"
                      }
                    ]
                }
             */

            var responseString = await client.GetStringAsync(_config["ServerSettings:restApiURL"] + "/" + hash + ".json");

            return responseString;
        }
    }
}
