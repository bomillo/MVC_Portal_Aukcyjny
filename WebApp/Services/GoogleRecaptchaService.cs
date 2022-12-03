
using Google.Api.Gax.ResourceNames;
using Google.Cloud.RecaptchaEnterprise.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Services
{
    public class GoogleRecaptchaService
    {
        private readonly IOptionsMonitor<GoogleRecaptchaModel> captchaConfig;

        public GoogleRecaptchaService(IOptionsMonitor<GoogleRecaptchaModel> captchaConfig)
        {
            this.captchaConfig = captchaConfig;
        }

        public async Task<bool> VerifyToken(string token)
        {

            try
            {
                var url = $"https://www.google.com/recaptcha/api/siteverify?secret={captchaConfig.CurrentValue.SecretKey}&response={token}";
            
                using(var client = new HttpClient())
                {
                    var httpResult = await client.GetAsync(url);
                    if(httpResult.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return false;
                    }

                    var responseString = await httpResult.Content.ReadAsStringAsync();


                    var googleResult = JsonConvert.DeserializeObject<GoogleCaptchaResponse>(responseString);

                    return googleResult.Success && googleResult.Score >= 0.3;
                }
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
