using api.Models;
using System.ComponentModel.DataAnnotations;

namespace api.NewFolder
{
    public class ProductDTO
    {
        public string Name { get; set; }

        public decimal Price { get; set; }
        
        public int Quantity { get; set; }

        public int CategoryId { get; set; }
    }
}
