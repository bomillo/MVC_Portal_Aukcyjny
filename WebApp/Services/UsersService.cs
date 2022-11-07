using System.Text;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class UsersService
    {
        private readonly PortalAukcyjnyContext context;

        public UsersService(PortalAukcyjnyContext context)
        {
            this.context = context;
        }

        public string HashPassword(string password) {
            var crypt = System.Security.Cryptography.SHA256.Create();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            return System.Convert.ToBase64String(crypto);
        }

        public User ValidateAndGetUser(string email, string password)
        {
            try
            {
                return context.Users.First(u => u.Email == email.ToLower() && u.PasswordHashed == HashPassword(password));
            }
            catch
            {
                return null;
            }
        }

        public bool ValidateUser(string email, string password)
        {
            return context.Users.Any(u => u.Email == email.ToLower() && u.PasswordHashed == HashPassword(password));
        }

        public void AddUser(User newUser)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == newUser.Email);

            if(user != null)
            {
                return;
            }

            context.Users.Add(newUser);
            context.SaveChanges();
        }
        public User AddUserFromExternalProvider(User newUser)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == newUser.Email);

            if (user != null)
            {
                if(newUser.ExternalFacebookId != null)
                {
                    user.ExternalFacebookId = newUser.ExternalFacebookId;
                }
                else
                {
                    user.ExternalGoogleId = newUser.ExternalGoogleId;
                }

                context.Update(user);
            }
            else
            {
                context.Users.Add(newUser);
            }
            context.SaveChanges();

            if(newUser.ExternalFacebookId != null)
            {
                return context.Users.FirstOrDefault(u => u.ExternalFacebookId == newUser.ExternalFacebookId);
            }
            else
            {
                return context.Users.FirstOrDefault(u => u.ExternalGoogleId == newUser.ExternalGoogleId);
            }
        }

        public User GetUserFromExternalProvider(string externalId, ExternalProvider provider)
        {
            if(provider == ExternalProvider.Facebook)
            {
               return context.Users.FirstOrDefault(u => u.ExternalFacebookId != null &&  u.ExternalFacebookId == externalId);

            }
            else
            {
               return context.Users.FirstOrDefault(u => u.ExternalGoogleId != null &&  u.ExternalGoogleId == externalId);
            }
        }

        public bool CreateUser(string email, string name, string password)
        {
            var userExists = context.Users.Any(u => u.Email == email.ToLower());
            if (!userExists)
            {
                context.Users.Add(new User()
                {
                    Email = email,
                    Name = name,
                    PasswordHashed = HashPassword(password),
                    ThemeType = ThemeType.Dark,
                    Language = Language.EN,
                    UserType = UserType.Normal
                });
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
