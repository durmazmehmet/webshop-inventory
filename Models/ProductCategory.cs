using System.ComponentModel.DataAnnotations;

namespace WebShopInventory.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParentCategoryId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Title { get; set; }

        [Required]
        public string Slug { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }    

    }
}
