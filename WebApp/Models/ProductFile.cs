using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace WebApp.Models
{
    public class ProductFile
    {
        public ProductFile()
        {

        }

        public ProductFile(int _ProductId, string _path, string _name, string _extension, string _description)
        {
            this.ProductId = _ProductId;
            this.Path = _path;
            this.Name = _name;
            this.Extension = _extension;
            this.Description = _description;
        }

        [Key]
        public int ProductFileId { get; set; }
        public int ProductId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        [AllowNull]
        [MaxLength(200)]
        public string? Description { get; set; }

    }
}
