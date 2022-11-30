using System.ComponentModel.DataAnnotations;
using WebApp.Resources.Authentication;
using WebApp.Resources;
using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.DTO
{
    public class EditAccountModel
    {
        [MaxLength(100, ErrorMessageResourceType = typeof(Shared), ErrorMessageResourceName = "FieldTooLong")]
        public string Name { get; set; }
        
        [DataType(DataType.EmailAddress, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [AllowNull]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [AllowNull]
        [DataType(DataType.Password)]
        public string PasswordVerification { get; set; }
        
        public int? CompanyId { get; set; }

        public Language Language { get; set; }

        public ThemeType ThemeType { get; set; }

    }
}
