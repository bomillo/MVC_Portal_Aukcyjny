using System.ComponentModel.DataAnnotations;
using WebApp.Resources.Authentication;
using WebApp.Resources;

namespace WebApp.Models.DTO
{
    public class RegisterUserModel
    {
        [MaxLength(100, ErrorMessageResourceType = typeof(Shared), ErrorMessageResourceName = "FieldTooLong")]
        [MinLength(5, ErrorMessageResourceType = typeof(Shared), ErrorMessageResourceName = "FieldTooShort")]
        [Required(ErrorMessageResourceType = typeof(Shared), ErrorMessageResourceName = "FieldRequired")]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "InvalidEmail")]
        [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessageResourceType = typeof(Localization),ErrorMessageResourceName = "WeakPassword")]
        [Required(ErrorMessageResourceType = typeof(Localization),ErrorMessageResourceName = "WeakPassword")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "WeakPassword")]
        public string PasswordVerification { get; set; }
    }
}
