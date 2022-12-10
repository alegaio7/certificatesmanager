using CertificatesManager.Api;
using CertificatesManager.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

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

            if (certRequest is null)
                return BadRequest("The request cannot be null");

            var loc = "";
            var country = "";
            var stdNameRegex = new Regex("^[\\w\\.\\- ]+$", RegexOptions.IgnoreCase); // for simplicity, special/accentuated chars are not allowed. modify if needed.
            var emailRegex = new Regex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", RegexOptions.IgnoreCase); // simple regex..emails could be more complicated than that

            if (string.IsNullOrEmpty(certRequest.Name))
                return BadRequest(Strings.REQUEST_NAME_CANNOT_BE_NULL);
            if (certRequest.Name.Length > Globals.CERT_REQUEST_NAME_SIZE_MAX)
                return BadRequest(string.Format(Strings.REQUEST_NAME_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_NAME_SIZE_MAX));
            if (!stdNameRegex.IsMatch(certRequest.Name))
                return BadRequest(Strings.REQUEST_NAME_HAS_INVALID_CHARS);

            if (string.IsNullOrEmpty(certRequest.Email))
                return BadRequest(Strings.REQUEST_EMAIL_CANNOT_BE_NULL);
            if (certRequest.Email.Length > Globals.CERT_REQUEST_EMAIL_SIZE_MAX)
                return BadRequest(string.Format(Strings.REQUEST_EMAIL_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_EMAIL_SIZE_MAX));
            if (!emailRegex.IsMatch(certRequest.Email))
                return BadRequest(Strings.REQUEST_EMAIL_IS_INVALID);

            if (string.IsNullOrEmpty(certRequest.Organization))
                return BadRequest(Strings.REQUEST_ORG_CANNOT_BE_NULL);
            if (certRequest.Organization.Length > Globals.CERT_REQUEST_ORG_SIZE_MAX)
                return BadRequest(string.Format(Strings.REQUEST_ORG_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_ORG_SIZE_MAX));
            if (!stdNameRegex.IsMatch(certRequest.Organization))
                return BadRequest(Strings.REQUEST_ORG_HAS_INVALID_CHARS);

            // signerCN
            if (string.IsNullOrEmpty(certRequest.SignerCN))
                return BadRequest(Strings.REQUEST_SIGNERCN_CANNOT_BE_NULL);
            if (certRequest.SignerCN.Length > Globals.CERT_REQUEST_SIGNERCN_SIZE_MAX)
                return BadRequest(string.Format(Strings.REQUEST_SIGNERCN_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_SIGNERCN_SIZE_MAX));
            if (!stdNameRegex.IsMatch(certRequest.SignerCN))
                return BadRequest(Strings.REQUEST_SIGNERCN_HAS_INVALID_CHARS);

            // SignerEmail
            if (string.IsNullOrEmpty(certRequest.SignerEmail))
                return BadRequest(Strings.REQUEST_SIGNERMAIL_CANNOT_BE_NULL);
            if (certRequest.SignerEmail.Length > Globals.CERT_REQUEST_SIGNEREMAIL_SIZE_MAX)
                return BadRequest(string.Format(Strings.REQUEST_SIGNERMAIL_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_SIGNEREMAIL_SIZE_MAX));
            if (!emailRegex.IsMatch(certRequest.SignerEmail))
                return BadRequest(Strings.REQUEST_SIGNERMAIL_IS_INVALID);

            // SignerOrganization
            if (string.IsNullOrEmpty(certRequest.SignerOrganization))
                return BadRequest(Strings.REQUEST_SIGNERORG_CANNOT_BE_NULL);
            if (certRequest.SignerOrganization.Length > Globals.CERT_REQUEST_SIGNERORG_SIZE_MAX)
                return BadRequest(string.Format(Strings.REQUEST_SIGNERORG_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_SIGNERORG_SIZE_MAX));
            if (!stdNameRegex.IsMatch(certRequest.SignerOrganization))
                return BadRequest(Strings.REQUEST_SIGNERORG_HAS_INVALID_CHARS);

            if (!string.IsNullOrEmpty(certRequest.Location))
            {
                if (certRequest.Location.Length > Globals.CERT_REQUEST_LOCATION_SIZE_MAX)
                    return BadRequest(string.Format(Strings.REQUEST_LOCATION_SIZE_EXCEEDED, Globals.CERT_REQUEST_LOCATION_SIZE_MAX));

                if (!stdNameRegex.IsMatch(certRequest.Location))
                    return BadRequest(Strings.REQUEST_LOCATION_HAS_INVALID_CHARS);

                loc = $"l={certRequest.Location};st={certRequest.Location}";
            }

            if (!string.IsNullOrEmpty(certRequest.Country))
            {
                try
                {
                    var cc = new CountryCodes(certRequest.Country);
                }
                catch (Exception)
                {
                    return BadRequest(Strings.REQUEST_COUNTRY_CODE_INVALID);
                }
                country = $"c={certRequest.Country}";
            }

            var optParams = new StringBuilder();
            if (!string.IsNullOrEmpty(loc))
                optParams.Append(loc);
            if (!string.IsNullOrEmpty(country))
            {
                if (optParams.Length > 0)
                    optParams.Append(";");
                optParams.Append(country);
            }
            if (optParams.Length > 0)
                optParams.Insert(0, ";");

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"cn={certRequest.Name};email={certRequest.Email};o={certRequest.Organization}{optParams}");

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