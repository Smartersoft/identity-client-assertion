using System;
using System.Collections.Generic;
using System.Text;

namespace Smartersoft.Identity.Client.Assertion
{
    /// <summary>
    /// Info about a certificate stored in the KeyVault
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// Name of the certificate
        /// </summary>
        public string? CertificateName { get; set; }
        /// <summary>
        /// Base64Url encoded hash of certificate, used in the client assertion
        /// </summary>
        public string? Kid { get; set; }
        /// <summary>
        /// KeyId of the private key, used for signing.
        /// </summary>
        public Uri? KeyId { get; set; }
    }
}
