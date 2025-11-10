using System.ComponentModel.DataAnnotations;
namespace Blog.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        //[Required(ErrorMessage = "Author name is required.")]
        [StringLength(50, ErrorMessage = "Author name cannot exceed 50 characters.")]
        [Display(Name = "Author Name")]
        public string? AuthorName { get; internal set; }

        //[Required(ErrorMessage = "Author email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? AuthorEmail { get; internal set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [DataType(DataType.MultilineText)]
        [StringLength(140)]
        public string Content { get; set; }


        [Display(Name = "Published Date")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset PublishedDate { get; internal set; }
        public bool edit { get; internal set; }  
    }
}
