using Microsoft.AspNetCore.Localization;
using WebApp.Models;

namespace WebApp.Services
{
    public class LanguageServices
    {
        public static string DecodeLanguage(Language lang) {
            return "en-US";
            if (lang == Language.PL)
            {
                return "pl-PL";
            }
            else if (lang == Language.FR)
            {
                return "fr-FR";
            }
        }

        public static void SetLanguage(HttpResponse response, Language lang) 
        {
            string languageCode = "en-US";
            if (lang == Language.PL)
            {
                languageCode = "pl-PL";
            }
            else if (lang == Language.FR)
            {
                languageCode = "fr-FR";
            }

            response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(languageCode)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
                );
        }

        public static void ClearLanguage(HttpResponse response)
        {
            response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        }
    }
}
