using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenMagician.Models;

public class CertificateInfo
{
    internal CertificateInfo(Smartersoft.Identity.Client.Assertion.CertificateInfo? certificateInfo)
    {
        CertificateName = certificateInfo?.CertificateName;
        KeyHash = certificateInfo?.Kid;
        KeyUri = certificateInfo?.KeyId;
        ExpiresOn = certificateInfo?.ExpiresOn;
    }
    public string? CertificateName { get; set; }
    public string? KeyHash { get; set; }
    public Uri? KeyUri { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
}
