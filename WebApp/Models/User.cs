using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string? PasswordHashed { get; set; }
        public UserType UserType { get; set; }
        [AllowNull]
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        [DefaultValue(ThemeType.Dark)]
        public ThemeType ThemeType { get; set; }
        [DefaultValue(Language.PL)]
        public Language Language { get; set; }
        public int? itemsOnPage { get; set; }
        
        public string? ExternalGoogleId { get; set; }
        public string? ExternalFacebookId { get; set; }
    }


    public enum ThemeType { Dark, Light, Highcontrast }

    public enum Language { PL, EN, FR }

    public enum UserType { Normal, Admin, Cośjeszcze }

    public enum ExternalProvider { Google, Facebook}
}