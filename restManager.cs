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
        public string Name { get; set; } = string.Empty;

        public bool isRequired { get; set; } = false;

        [JsonIgnore]        
        public string Value { get; set; } = string.Empty;
    }

    public class FunctionModel
    {
        public string Name { get; set; }
        public string Desc { get; set; }

        public string URL { get; set; }

        [JsonIgnore]
        public string Method { get; set; } = null;
        public List<Paramameter> Params { get; set; }


        public static void extractMethodFromUrl(FunctionModel func)
        {
            func.URL = func.URL.Trim().Replace(" ", "");
            string method = func.URL.Split('/').ToList()[0];
            func.Method = method;
            func.URL = func.URL.Replace(method, "");
        }

        internal void SetParamValue(string name, string v)
        {
            Params.Find(x => x.Name == name).Value = v;
        }
    }

    internal class restManager
    {
        public static string request(FunctionModel func)
        {
            var method = func.Method;
            var url = func.URL;
            var parameters = func.Params;

            RestClient client = new RestClient("https://api.blockstream.space/testnet");
            RestRequest request;

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
                foreach (var param in parameters)
                {
                    if (isValidParameter(param))
                    {
                        request.AddParameter(param.Name, param.Value);
                    }
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

        private static bool isValidParameter(Paramameter param)
        {
            if(param.Value.Trim() == "")
            {
                return false;
            }
            return true;
        }
    }
}
