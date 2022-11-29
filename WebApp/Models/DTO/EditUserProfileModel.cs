using System.ComponentModel.DataAnnotations;
using WebApp.Resources.Authentication;
using WebApp.Resources;

namespace WebApp.Models.DTO
{
    public class EditUserProfileModel
    {
        [MaxLength(100, ErrorMessageResourceType = typeof(Shared), ErrorMessageResourceName = "FieldTooLong")]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        public string PasswordVerification { get; set; }
        public Language Language { get; set; }

        internal static EditUserProfileModel FromUser(User user)
        {
            EditUserProfileModel model = new EditUserProfileModel();
            model.Language = user.Language;
            model.Email = user.Email;
            model.Name = user.Name;
            return model;
        }
    }
}
