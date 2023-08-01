using System.ComponentModel.DataAnnotations;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class MsiRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Resource { get; set; }
    }
}
