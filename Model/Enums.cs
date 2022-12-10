namespace CertificatesManager.Model
{
    public static class Enums
    {
        public enum SignatureAlgorithm
        {
            ECDSA,
            RSA
        }

        public enum Format
        {
            PFXBase64Encoded,
            PFXRaw
        }
    }
}