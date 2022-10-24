using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; }
        [MaxLength(10)]
        [MinLength(10)]
        [Required]
        public string NIP { get; set; }
    }
}
