using System.ComponentModel.DataAnnotations;
using WebApp.Resources.Authentication;
using WebApp.Resources;

namespace WebApp.Models.DTO
{
    public class ResetPasswordModel
    {
        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessageResourceType = typeof(Localization),ErrorMessageResourceName = "WeakPassword")]
        [Required(ErrorMessageResourceType = typeof(Localization),ErrorMessageResourceName = "WeakPassword")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "WeakPassword")]
        public string PasswordVerification { get; set; }
        public string Guid { get; set; }
    }
}
