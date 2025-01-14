using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required, MaxLength(255)]
        public decimal Price { get; set; }
        [Required]
        public int Quantity { get; set; }

        public Category Category { get; set; }
    }
}
