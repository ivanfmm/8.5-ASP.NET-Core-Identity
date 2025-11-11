using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Comment
    {
        /// <summary>
        /// The identifier of the article this comment belongs to.
        /// </summary>
        [Key]
        public int Id { get; set; }
        public int ArticleId { get; set; }

        /// <summary>
        /// The content of the comment.
        /// </summary>
        [Required(ErrorMessage = "Content is required.")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        /// <summary>
        /// Represents the moment the comment was posted.
        /// </summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }
    }
}
