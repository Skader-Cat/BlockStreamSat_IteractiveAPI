using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace StreamBlockSat_InterAPI.Model
{
    public class Paramameter
    {
        public string Name { get; set; } = string.Empty;

        public bool isRequired { get; set; } = false;

        [Newtonsoft.Json.JsonIgnore]
        public string Value { get; set; } = string.Empty;
    }


    public class FunctionModel
    {
        public string Name { get; set; }
        public string Desc { get; set; }

        public string URL { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string Method { get; set; } = null;
        public List<Paramameter> Params { get; set; }


        internal static void extractMethodFromUrl(FunctionModel func)
        {
            func.URL = func.URL.Trim().Replace(" ", "");
            string method = func.URL.Split('/').ToList()[0];
            func.Method = method;
        }

        internal void setParametersToURL() //https://api.blockstream.space/testnet/order/:uuid  -> заменит :uuid на значение этого параметра
        {
            foreach (var param in Params)
            {
                if (URL.Contains(param.Name))
                {
                    URL = URL.Replace($":{param.Name}", param.Value);
                }
            }
        }

        internal static string getFuncAndDescJsonFromResources()
        {
            string textFuncAndDesc = Encoding.UTF8.GetString(StreamBlockSat_InterAPI.Properties.Resources.funcAndDesc);
            return textFuncAndDesc;
        }

        internal void SetParamValue(string name, string v)
        {
            Params.Find(x => x.Name == name).Value = v;
        }
    }

    internal class RestManager
    {
        public static string request(FunctionModel func)
        {
            func.setParametersToURL();
            FunctionModel.extractMethodFromUrl(func);

            var method = func.Method;
            var url = func.URL.Replace(method, "");
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

            if (response.IsSuccessful && response.Content != null)
            {
                string content = response.Content;
                return prettifyResponse(content);
            }
            else
            {
                string responseContent = response.Content;
                string errorMessage = response.ErrorMessage;
                return prettifyResponse(responseContent + "\n" + errorMessage);
            }
        }

        private static bool isValidParameter(Paramameter param)
        {
            if (param.Value.Trim() == string.Empty)
            {
                return false;
            }
            return true;
        }

        private static string prettifyResponse(string v)
        {
            Newtonsoft.Json.Linq.JToken parsedJson = Newtonsoft.Json.Linq.JToken.Parse(v);
            return parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}
