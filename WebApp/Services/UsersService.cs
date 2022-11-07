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
    }
}
