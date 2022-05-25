namespace CertificatesManager
{
    public class SignCertificateRequestModel
    {
        public string CertificateName { get; set; }
        public string Base64IssuerCertificate { get; set; }
        public string IssuerCertificatePassword { get; set; }
        public string NewCertificatePassword { get; set; }
        public int DaysValid { get; set; }
        public Enums.Format OutputFormat { get; set; }
    }
}