using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Send.Client.Tests
{
    public static class TestHelper
    {
        public static MultipartFormDataContent GetMultipartContent(HttpRequestMessage request)
        {
            return request.Content as MultipartFormDataContent;
        }

        public static async Task<string> GetPartContent(HttpRequestMessage request, string name)
        {
            foreach (var part in GetMultipartContent(request))
            {
                if (part.Headers.ContentDisposition.Name == name)
                {
                    var value = await part.ReadAsStringAsync().ConfigureAwait(false);
                    return value;
                }
            }

            throw new Exception($"Could not find content: {name}");
        }

        public static string GetFilename(HttpRequestMessage request, string name)
        {
            foreach (var part in GetMultipartContent(request))
            {
                if (part.Headers.ContentDisposition.Name == name)
                {
                    return part.Headers.ContentDisposition.FileName;
                }
            }

            throw new Exception($"Could not find header: {name}");
        }

        public static X509Certificate2 GetDummyCert(TimeProvider timeProvider = null)
        {
            timeProvider ??= TimeProvider.System;

            return new CertificateRequest(
                new X500DistinguishedName("CN=Test"),
                RSA.Create(),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            ).CreateSelfSigned(
                timeProvider.GetUtcNow(),
                timeProvider.GetUtcNow().AddDays(1)
            );
        }

        public static KontoOffentligNokkel GetDummyPublicKey(TimeProvider timeProvider = null)
        {
            var cert = GetDummyCert(timeProvider);

            return new KontoOffentligNokkel
            {
                IssuerDN = cert.Issuer,
                Nokkel = cert.ExportCertificatePem(),
                Serial = cert.SerialNumber,
                SubjectDN = cert.Subject,
                ValidFrom = cert.NotBefore,
                ValidTo = cert.NotAfter
            };
        }
    }
}