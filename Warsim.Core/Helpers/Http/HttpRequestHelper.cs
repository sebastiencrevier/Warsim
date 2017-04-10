using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Warsim.Core.Helpers.Http
{
    public class HttpRequestHelper
    {
        public static async Task<HttpResponseMessage> PostAsync(string url, object postData, string token = "")
        {
            using (var client = new HttpClient())
            {
                if (token != "") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                var content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                //response.EnsureSuccessStatusCode();

                return response;
            }
        }

        public static async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> parameters, string token = "")
        {
            using (var client = new HttpClient())
            {
                if (token != "") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                if (parameters == null) parameters = new Dictionary<string, string>();

                var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync(url, content);

                response.EnsureSuccessStatusCode();

                return response;
            }
        }

        public static async Task<HttpResponseMessage> PostUrlEncodedAsync(string url, IEnumerable<KeyValuePair<string, string>> postData)
        {
            using (var httpClient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(postData))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    var response = await httpClient.PostAsync(url, content);

                    //response.EnsureSuccessStatusCode();

                    return response;
                }
            }
        }
        public static async Task<HttpResponseMessage> GetAsync(string url, string token = "")
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);

                // Add token if specified
                if (token != "") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                var response =
                    await client.GetAsync(url)
                        .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response;
            }
        }

        public static async Task<HttpResponseMessage> PostObjectAsync(string url, object obj, string token = "")
        {
            using (var client = new HttpClient())
            {
                if (token != "") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                var content = "";
                if (obj != null) content = JsonConvert.SerializeObject(obj);

                var response =
                    await
                        client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                            .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response;
            }
        }

        public static async Task<HttpResponseMessage> DeleteAsync(string url, string token = "")
        {
            using (var client = new HttpClient())
            {
                if (token != "") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                var response = await client.DeleteAsync(url).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response;
            }
        }

        public static HttpStatusCode GetStatusCode(HttpResponseMessage message)
        {
            return message.StatusCode;
        }

        public static async Task<string> GetContent(HttpResponseMessage message)
        {
            try
            {
                return await message.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                throw new Exception("cannot read content of message in - GerCOntent");
            }
        }

        public static async Task<HttpResponseMessage> PostBodyContentAsync(string url, byte[] bytes, string token = "")
        {
            using (var client = new HttpClient())
            {
                if (token != "") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                var response =
                    await
                        client.PostAsync(url, new ByteArrayContent(bytes))
                            .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response;
            }
        }
    }
}