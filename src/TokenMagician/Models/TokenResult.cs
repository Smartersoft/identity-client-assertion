using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenMagician.Models;

public class TokenResult
{
    public TokenResult() { }
    internal TokenResult(AuthenticationResult result, string? clientId, string? scope, string? tenantId)
    {
        ClientId = clientId;
        TenantId = tenantId;
        Scope = scope;
        AccessToken = result.AccessToken;
        ExpiresOn = result.ExpiresOn;
    }
    public string? ClientId { get; set; }
    public string? TenantId { get; set; }
    public string? Scope { get; set; }
    public string? AccessToken { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
}
