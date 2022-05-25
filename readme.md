# Certificates Manager

Certificates Manager (CM) is a REST API project for requesting self-signed certificates, either for personal use, for services or for creating CA certificates.

## Running the code

You may use the certificates manager as a standalone console application, as a service (with the help of [Topshelf](http://topshelf-project.com/) or by hosting it as a REST service in IIS)

The project as is only works as a console application.

## Endpoints

CM exposes REST 3 endpoints in the **api/certificates** route:

 - CreateSignedCertificate: Creates a self-signed certificate with RSA key of 2048 bits and SHA-256 digest.
 - CreateSelfSignedCertificateForPerson: The same as CreateSignedCertificate, but the certificate created includes the email, name, country, location and organization attributes. The key usage specified is DigitalSignature. The certificate can be created either with RSA o ECDSA.
 - GetSelfSignedCertificateForService: The same as CreateSignedCertificate, but the certificate created includes DNS names and key usages useful for services (like CrlSign or KeyCertSign). The certificate can be created either with RSA o ECDSA.

The 3 endpoints can return the raw bytes encoded in Base64, or a .pfx file.

## Examples
**Request a certificate for a person**

    public class PersonCertificateRequestModel
    {
        public enum FormatEnum {
            PFXBase64Encoded,
            PFXRaw
        }
        public enum SignatureAlgorithmEnum {
            ECDSA,
            RSA
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string Organization { get; set; }
        public string SignerCN { get; set; }
        public string SignerEmail { get; set; }
        public string SignerOrganization { get; set; }
        public FormatEnum OutputFormat { get; set; }
        public SignatureAlgorithmEnum SignatureAlgorithm { get; set; }
    }
    
    public async Task<byte[]> RequestSelfSignedCertificate(string name, string email, string password, PersonCertificateRequestModel.SignatureAlgorithmEnum alg) {
	    var client = new System.Net.Http.HttpClient();
	    client.BaseAddress = new Uri("http://localhost:45521");
	    var request = new PersonCertificateRequestModel() {
		    Email = email,
		    Name = name,
		    Password = password,
		    Country = "Argentina",
		    Location = "Buenos Aires",
		    Organization = "My company,
		    SignerCN = "My Company - Root Cert",
		    SignerEmail = "info@mycompany.com",
		    SignerOrganization = "My Company",
		    OutputFormat = PersonCertificateRequestModel.FormatEnum.PFXRaw,
		    SignatureAlgorithm = 	PersonCertificateRequestModel.SignatureAlgorithmEnum.RSA
	    };
	    var json = JsonConvert.SerializeObject(req);
	    var content = new StringContent(json, Encoding.UTF8, "application/json");
	    var response = await 	client.PostAsync("api/Certificates/CreateSelfSignedCertificateForPerson", content);
	    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
		    using (var ms = new MemoryStream()) {
		        var s = await response.Content.ReadAsStreamAsync();
		        s.CopyTo(ms);
		        return ms.ToArray();
		    }
		} else {
			throw new Exception(response.StatusCode.ToString());
		}
	}

## Requirements/dependencies
- .NET Core 6

## License

MIT
