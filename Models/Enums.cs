using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CertificatesManager
{
    public static class Enums
    {
        public enum SignatureAlgorithm { 
            ECDSA,
            RSA
        }

        public enum Format { 
            PFXBase64Encoded,
            PFXRaw
        }
    }
}
