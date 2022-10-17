using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class ApiKey {
        
        public User User { get; set; }
        [Key]
        public string Key { get; set; }
    }
}
