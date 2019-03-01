using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KS.Fiks.Io.Send.Client.Tests
{
    public static class TestHelper
    {
        public static MultipartContent GetMultipartContent(HttpRequestMessage response)
        {
            return response.Content as MultipartContent;
        }

        public static async Task<string> GetPartContent(HttpRequestMessage response, string name)
        {
            foreach (var part in GetMultipartContent(response))
            {
                if (part.Headers.Contains("name") && part.Headers.GetValues("name").Contains(name))
                {
                    var value = await part.ReadAsStringAsync().ConfigureAwait(false);
                    return value;
                }
            }

            throw new Exception("Could not find content");
        }

        public static string GetPartHeader(HttpRequestMessage response, string name, string headerField)
        {
            foreach (var part in GetMultipartContent(response))
            {
                if (part.Headers.Contains("name") && part.Headers.GetValues("name").Contains(name))
                {
                    return part.Headers.GetValues(headerField).FirstOrDefault();
                }
            }

            throw new Exception("Could not find header");
        }
    }
}