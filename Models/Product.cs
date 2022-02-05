using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShopInventory.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class Product
    {
        [Column(Order = 0)]
        [Key]
        public int Id { get; set; }

        [Column(Order = 1)]
        [Required]
        [Display(Name = "SKU")]
        [StringLength(maximumLength: 10, MinimumLength = 5)]
        public string Code { get; set; }

        [Column(Order = 2)]
        [Required]
        [Display(Name = "Product", Prompt = "Product name")]
        public string Title { get; set; }

        [Column(Order = 3)]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Stock Quantity")]
        [Column(Order = 4)]
        [Required]
        [Range(0, long.MaxValue, ErrorMessage = "Stock can not be negative")]
        public long Stock { get; set; }

        [Display(Name = "Price")]
        [Column(Order = 5)]
        [Required]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Prive can not be negative")]
        public double Price { get; set; }

        [Display(Name = "Image")]
        [Column(Order = 6)]
        public string ImagePath { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        [NotMapped]
        [Display(Name = "Upload File")]
        public IFormFile ImageFile { get; set; }
    }
}
