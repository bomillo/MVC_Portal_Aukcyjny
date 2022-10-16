namespace WebApp.Models
{
    public class User
    {
        public int UserId;
        public string Name;
        public string Email;
        public string PasswordHashed;
        public UserType UserType;
        public bool IsCompany;
        public string CompanyId;
        public ThemeType ThemeType;
        public Language Language;
    }


    public enum ThemeType { Dark, Light, Gay }

    public enum Language { PL, EN, FR }

    public enum UserType { Normal, Admin, Cośjeszcze }
}
