using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class User
    {
        
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHashed { get; set; }
        public UserType UserType { get; set; }
        public bool IsCompany { get; set; }
        public Company Company { get; set; }
        public ThemeType ThemeType { get; set; }
        public Language Language { get; set; }
    }


    public enum ThemeType { Dark, Light, Gay }

    public enum Language { PL, EN, FR }

    public enum UserType { Normal, Admin, Cośjeszcze }
}
