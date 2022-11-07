using System.Globalization;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Helpers
{
    public class CultureChanger : IDisposable
    {
        readonly CultureInfo originalCulture;
        readonly CultureInfo originalUICulture;

        public CultureChanger(User user)
        {
            CultureInfo culture = new CultureInfo(LanguageServices.DecodeLanguage(user.Language));

            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            originalCulture = Thread.CurrentThread.CurrentCulture;
            originalUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUICulture;
        }
    }
}
