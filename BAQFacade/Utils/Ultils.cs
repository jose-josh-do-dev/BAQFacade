using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BAQFacade.Utils
{
    public class EpiSettings
    {
        public string Host { get; set; }
        public string Company { get; set; }
        public string Instance { get; set; }
        public string ApiVersion { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string AuthBAQ { get; set; }
    }

    public static class EpiUtils
    {
        /// <summary>
        /// Encodes a string to base64, using default encoding.
        /// </summary>
        /// <param name="str">String to encode.</param>
        /// <returns>Encdoded string.</returns>
        public static string Base64Encode(string str)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(str));
        }

        /// <summary>
        /// Decodes a string from base64, using default encoding.
        /// </summary>
        /// <param name="str">base64 encoded string.</param>
        /// <returns>Decoded string.</returns>
        public static string Base64Decode(string str)
        {
            return Encoding.Default.GetString(Convert.FromBase64String(str));
        }

        /// <summary>
        /// Generates Password Hash
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        /// <summary>
        /// Generates Password Hash String
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /// <summary>
        /// Validates that a given session GUID is valid and extends its life by 10 minutes.
        /// </summary>
        /// <param name="guid">Token</param>
        /// <param name="baqID">BAQ ID</param>
        /// <param name="_path">Epicor URL</param>
        /// <param name="_user">Epicor User</param>
        /// <param name="msg">Out message from validation</param>
        /// <returns></returns>
        public static bool ValidSession(string guid, string baqID, string _path, string _user, string _authBAQ, out string msg)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                var restClient = new RestSharp.RestClient(_path);
                var request = new RestSharp.RestRequest($"BaqSvc/{_authBAQ}");

                request.Parameters.Add(new RestSharp.Parameter("TokenID", guid, RestSharp.ParameterType.QueryString));
                request.Parameters.Add(new RestSharp.Parameter("BAQID", baqID, RestSharp.ParameterType.QueryString));
                request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");

                IRestResponse response = restClient.Execute(request, Method.GET);
                msg = response.Content;
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                    case System.Net.HttpStatusCode.Unauthorized:
                    default:
                        {
                            msg = $"Token: {guid} is not Authorized to Execute BAQ: {baqID}";
                            return false;
                        }
                    case System.Net.HttpStatusCode.OK:
                        {
                            dynamic val = Newtonsoft.Json.JsonConvert.DeserializeObject(msg);
                            if (val.value.Count > 0)
                                return true;
                            else
                            {
                                msg = $"Token: {guid} is not Authorized to Execute BAQ: {baqID}";
                                return false;
                            }
                        }
                }
            }
            msg = $"Token: {guid} is not Authorized to Execute BAQ: {baqID}";
            return false;
        }

    }
}
