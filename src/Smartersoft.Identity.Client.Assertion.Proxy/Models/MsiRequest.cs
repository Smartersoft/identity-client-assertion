using System.ComponentModel.DataAnnotations;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// MSI request as if this would come from CloudShell
    /// </summary>
    public class MsiRequest
    {
        /// <summary>
        /// The resource you want to get a token for
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public required string Resource { get; set; }
    }
}
