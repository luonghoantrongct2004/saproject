using System.ComponentModel.DataAnnotations;

namespace SAProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string? Description { get; set; }
    }
}
