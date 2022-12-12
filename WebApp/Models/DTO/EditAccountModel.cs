using System.ComponentModel.DataAnnotations;
using WebApp.Resources.Authentication;
using WebApp.Resources;
using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.DTO
{
    public class EditAccountModel
    {
        public string? Name { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "InvalidEmail")]
        public string? Email { get; set; }


        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordNotMatch")]
        public string? OldPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordNotMatch")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordNotMatch")]
        public string? PasswordVerification { get; set; }
        
        public int? CompanyId { get; set; }

        public Language? newLanguage { get; set; }

        public ThemeType? newThemeType { get; set; }

        public int? itemsOnPage { get; set; }

    }
}
