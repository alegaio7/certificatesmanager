using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CertificatesManager
{
    public class PersonCertificateRequestModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string Location { get; set; }
        public string Country { get; set; }
        [Required]
        public string Organization { get; set; }
        [Required]
        public string SignerCN { get; set; }
        [Required]
        public string SignerEmail { get; set; }
        [Required]
        public string SignerOrganization { get; set; }

        public Enums.Format OutputFormat { get; set; }
        public Enums.SignatureAlgorithm SignatureAlgorithm { get; set; }
    }
}
