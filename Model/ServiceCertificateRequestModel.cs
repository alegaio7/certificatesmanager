namespace CertificatesManager.Model
{
    public class ServiceCertificateRequestModel
    {
        public string CertificateName { get; set; }
        public string[] DnsNames { get; set; }
        public string[] IPAddresses { get; set; }
        public string Password { get; set; }
        public Enums.Format OutputFormat { get; set; }
    }
}