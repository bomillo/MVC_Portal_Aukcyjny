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

            if (user != null && user.ExternalProvider == ExternalProvider.None)
            {
                user.ExternalProvider = newUser.ExternalProvider;
                user.ExternalId = newUser.ExternalId;

                context.Update(user);
            }
            else
            {
                context.Users.Add(newUser);
            }
            context.SaveChanges();
            return context.Users.FirstOrDefault(u => u.ExternalId == newUser.ExternalId && u.ExternalProvider == newUser.ExternalProvider);
        }

        public User GetUserFromExternalProvider(string externalId, ExternalProvider provider)
        {
            return context.Users.FirstOrDefault(u => u.ExternalProvider == provider && u.ExternalId ==externalId);
        }
    }
}
