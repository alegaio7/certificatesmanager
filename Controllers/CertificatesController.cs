using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertificatesManager.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CertificatesController : ControllerBase
    {
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ILogger<CertificatesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult CreateSelfSignedCertificateForPerson([FromBody] PersonCertificateRequestModel certRequest)
        {
            AsymmetricAlgorithm alg = default;
            CertificateRequest request = default;
            X509SignatureGenerator generator = default;
            X509Certificate2 certWithKey = default;

            var loc = "";
            var country = "";
            if (!string.IsNullOrEmpty(certRequest.Location))
                loc = $"l={certRequest.Location};st={certRequest.Location}";

            if (!string.IsNullOrEmpty(certRequest.Country))
                country = $"c={certRequest.Country}";

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"cn={certRequest.Name};email={certRequest.Email};o={certRequest.Organization};{loc};{country}");

            if (certRequest.SignatureAlgorithm == Enums.SignatureAlgorithm.ECDSA)
            {
                var ecdsa = ECDsa.Create(); // generate asymmetric key pair
                alg = ecdsa;
                request = new CertificateRequest(distinguishedName, ecdsa, HashAlgorithmName.SHA256);
                generator = X509SignatureGenerator.CreateForECDsa(ecdsa);
            }
            else
            {
                var rsa = RSA.Create(); // generate asymmetric key pair
                alg = rsa;
                request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                generator = X509SignatureGenerator.CreateForRSA(rsa, RSASignaturePadding.Pkcs1);
            }

            // set basic certificate contraints
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));

            // key usage: Digital Signature
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

            X500DistinguishedName issuer = new X500DistinguishedName($"cn={certRequest.SignerCN};email={certRequest.SignerEmail};o={certRequest.SignerOrganization};");

            Span<byte> serialNumber = stackalloc byte[8];
            RandomNumberGenerator.Fill(serialNumber);

            using (var cert = request.Create(issuer, generator, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(5), serialNumber))
            {
                cert.FriendlyName = certRequest.Name;

                if (certRequest.SignatureAlgorithm == Enums.SignatureAlgorithm.ECDSA)
                    certWithKey = cert.CopyWithPrivateKey((ECDsa)alg);
                else
                    certWithKey = cert.CopyWithPrivateKey((RSA)alg);

                var b = certWithKey.Export(X509ContentType.Pkcs12, certRequest.Password);
                if (certRequest.OutputFormat == Enums.Format.PFXBase64Encoded)
                    return Ok(Convert.ToBase64String(b));
                else
                    return File(b, "application/octet-stream", "certificate.pfx");
            }
        }

        [HttpPost]
        public IActionResult GetSelfSignedCertificateForService([FromBody] ServiceCertificateRequestModel certRequest)
        {
            try
            {
                if (certRequest is null)
                    throw new NullReferenceException("Request is null");

                SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();

                if (certRequest.IPAddresses != null)
                {
                    foreach (var ip in certRequest.IPAddresses)
                    {
                        sanBuilder.AddIpAddress(IPAddress.Parse(ip));
                    }
                }
                //sanBuilder.AddIpAddress(IPAddress.Loopback);
                //sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);

                if (certRequest.DnsNames != null)
                {
                    foreach (var dns in certRequest.DnsNames)
                    {
                        sanBuilder.AddDnsName(dns);
                    }
                }
                //sanBuilder.AddDnsName("localhost");
                //sanBuilder.AddDnsName(Environment.MachineName);

                X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={certRequest.CertificateName}");

                using (RSA rsa = RSA.Create(2048))
                {
                    var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
                    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, true));
                    request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
                    /*
                    request.CertificateExtensions.Add(
                        new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                    request.CertificateExtensions.Add(
                       new X509EnhancedKeyUsageExtension(
                           new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                    // 1.3.6.1.5.5.7.3.1 = Server auth.
                    // 1.3.6.1.4.1.43054.6 Certificate policy for document signing
                    // 1.3.6.1.5.5.7.3.8 Time staimping
                    // 1.3.6.1.5.5.7.3.1 Server Authentication
                    // 1.3.6.1.5.5.7.3.2 Client Authentication 
                    */

                    request.CertificateExtensions.Add(sanBuilder.Build());

                    using (var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650))))
                    {
                        certificate.FriendlyName = certRequest.CertificateName;

                        //var x509 = new X509Certificate2(certificate.Export(X509ContentType.Pfx, certRequest.Password), certRequest.Password, X509KeyStorageFlags.MachineKeySet);

                        var b = certificate.Export(X509ContentType.Pkcs12, certRequest.Password);
                        if (certRequest.OutputFormat == Enums.Format.PFXBase64Encoded)
                            return Ok(Convert.ToBase64String(b));
                        else
                            return File(b, "application/octet-stream", "certificate.pfx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error @ {nameof(GetSelfSignedCertificateForService)}");
                throw;
            }
        }

        [HttpPost]
        public IActionResult CreateSignedCertificate([FromBody] SignCertificateRequestModel certRequest)
        {
            try
            {
                if (certRequest is null)
                    throw new NullReferenceException("Request is null");

                X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={certRequest.CertificateName}");

                using (RSA rsa = RSA.Create(2048))
                {
                    var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.DigitalSignature, false));
                    //request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.4.1.43054.6") }, false));

                    request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

                    using (var x509Issuer = new X509Certificate2(Convert.FromBase64String(certRequest.Base64IssuerCertificate), certRequest.IssuerCertificatePassword))
                    {
                        using (X509Certificate2 xsignedcert = request.Create(x509Issuer, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(certRequest.DaysValid), GenerateSerialNumber()))
                        {
                            var certWithKey = xsignedcert.CopyWithPrivateKey(rsa);
                            var hp = certWithKey.HasPrivateKey;
                            var b = certWithKey.Export(X509ContentType.Pkcs12, certRequest.NewCertificatePassword);
                            if (certRequest.OutputFormat == Enums.Format.PFXBase64Encoded)
                                return Ok(Convert.ToBase64String(b));
                            else
                            {
                                return File(b, "application/octet-stream", "certificate.pfx");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error @ {nameof(CreateSignedCertificate)}");
                throw;
            }
        }

        /*
         * https://stackoverflow.com/questions/48196350/generate-and-sign-certificate-request-using-pure-net-framework
          CA/Browser Forum's Baseline Requirements (v1.7.2) only says "CAs SHALL generate non-sequential Certificate serial numbers greater than zero (0)
        containing at least 64 bits of output from a CSPRNG.", which is the guidance for public CAs. If you're a private CA: Pure random,
        incrementing integer, high 4 bytes are an integer, low 8 are random, whatever you like (just don't be more than 20 bytes long).
         */

        private byte[] GenerateSerialNumber()
        {
            var root = "NBD1"; // NoobitAR DigitalDocs v1
            var rnd = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
            var r = new byte[4];
            using (var ms = new MemoryStream())
            {
                ms.Write(System.Text.Encoding.UTF8.GetBytes(root), 0, root.Length);
                rnd.NextBytes(r);
                ms.Write(r, 0, r.Length);
                return ms.ToArray();
            }
        }
    }
}