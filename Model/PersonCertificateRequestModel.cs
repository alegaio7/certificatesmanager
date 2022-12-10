using System.ComponentModel.DataAnnotations;

namespace CertificatesManager.Model
{
    public class PersonCertificateRequestModel
    {
        /// <summary>
        /// Required: The name of the person which the certificate will be issued to. 
        /// The name is embedded in the certificate's subject field as 'cn'.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Required: The email of the person which the certificate will be issued to.
        /// The email is embedded in the certificate's subject field as 'email'.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// The password used when exporting the certificate to a .pfx format.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Optional: The location of the person which the certificate will be issued to.
        /// The location is embedded in the certificate's subject field as 'l'.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Optional: The two-letter country code of the person which the certificate will be issued to.
        /// The country is embedded in the certificate's subject field as 'c'.
        public string Country { get; set; }

        /// <summary>
        /// Optional: The person's organization name.
        /// The organization name is embedded in the certificate's subject field as 'o'.
        [Required]
        public string Organization { get; set; }

        /// <summary>
        /// Required: The common name of the certificate's issuer, usually the company domain name.
        /// The common name is embedded in the certificate's issuer field as 'cn'.
        /// </summary>
        [Required]
        public string SignerCN { get; set; }

        /// <summary>
        /// Required: The certificate issuer's email address.
        /// The email is embedded in the certificate's issuer field as 'email'.
        /// </summary>
        [Required]
        public string SignerEmail { get; set; }

        /// <summary>
        /// Required: The certificate issuer organization's name, usually the company domain name.
        /// The organization is embedded in the certificate's issuer field as 'o'.
        /// </summary>
        [Required]
        public string SignerOrganization { get; set; }

        /// <summary>
        /// The output format of the certificate. It can be a raw pfx file, or a base64 encoded version.
        /// </summary>
        public Enums.Format OutputFormat { get; set; }

        /// <summary>
        /// The algorithm used to create the signing key. RSA and ECDSA are supported.
        /// </summary>
        public Enums.SignatureAlgorithm SignatureAlgorithm { get; set; }
    }
}