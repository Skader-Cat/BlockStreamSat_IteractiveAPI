using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using RestSharp;

namespace BlockStreamSatAPI
{
    public class Paramameter
    {
        string Name = string.Empty;
        string Value = string.Empty;
        bool isOptional = false;
    }

    public class FunctionModel
    {
        public string Name { get; set; }
        public string Desc { get; set; }

        public string URL { get; set; }

        [JsonIgnore]
        public string Method { get; set; } = null;
        public List<Paramameter> Params { get; set; }
    }

    internal class restManager
    {
        public static string request(string url, string method, Dictionary<string, string> reqParameters, Dictionary<string, string> optParameters)
        {
            RestClient client = new RestClient("https://api.blockstream.space/testnet");
            RestRequest request;
            var parameters = reqParameters.Concat(optParameters)
                                            .ToDictionary(x => x.Key, x => x.Value);
            switch (method)
            {
                case "GET":
                    request = new RestRequest(url, Method.Get);
                    break;
                case "POST":
                    request = new RestRequest(url, Method.Post);
                    break;
                case "PUT":
                    request = new RestRequest(url, Method.Put);
                    break;
                case "DELETE":
                    request = new RestRequest(url, Method.Delete);
                    break;
                default:
                    request = new RestRequest(url, Method.Head);
                    break;
            }

            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> kvp in parameters)
                {
                    request.AddParameter(kvp.Key, kvp.Value);
                }
            }

            RestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                string content = response.Content;
                return content;
            }
            else
            {
                string errorMessage = response.ErrorMessage;
                return errorMessage;
            }
        }

        public static void extractMethodFromUrl(FunctionModel func)
        {
            string method = func.URL.Split('/').ToList()[0];
            func.Method = method;
            func.URL = func.URL.Replace(method, "");
        }
    }
}
