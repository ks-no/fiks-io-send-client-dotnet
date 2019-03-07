using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KS.Fiks.Io.Send.Client.Tests
{
    public static class TestHelper
    {
        public static MultipartFormDataContent GetMultipartContent(HttpRequestMessage response)
        {
            return response.Content as MultipartFormDataContent;
        }

        public static async Task<string> GetPartContent(HttpRequestMessage response, string name)
        {
            foreach (var part in GetMultipartContent(response))
            {
                if (part.Headers.ContentDisposition.Name == name)
                {
                    var value = await part.ReadAsStringAsync().ConfigureAwait(false);
                    return value;
                }
            }

            throw new Exception("Could not find content");
        }

        public static string GetFilename(HttpRequestMessage response, string name)
        {
            foreach (var part in GetMultipartContent(response))
            {
                if (part.Headers.ContentDisposition.Name == name)
                {
                    return part.Headers.ContentDisposition.FileName;
                }
            }

            throw new Exception("Could not find header");
        }
    }
}