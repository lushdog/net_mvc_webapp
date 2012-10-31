using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace K2Calendar.Models
{
    public class PostModel
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:MM\/dd\/yyyy}")]
        [Display(Name = "Date posted")]
        public DateTime PostDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:MM\/dd\/yyyy}")]
        [Display(Name = "Date taught")]
        public DateTime EventDate { get; set; }

        [Required]
        [DataType(DataType.ImageUrl)]
        [Display(Name = "Youtube key")]
        public string YoutubeId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public int RankId { get; set; }

        [ForeignKey("RankId")]
        [Display(Name = "Rank")]
        public RankModel Rank { get; set; }

        [NotMapped]
        [Required]
        [Display(Name = "Tags")]
        public string  TagsInput { get; set; }

        public virtual ICollection<TagModel> Tags { get; set; }

        [Display(Name = "Comments")]
        public virtual ICollection<CommentModel> Comments { get; set; }

        public int PosterId { get; set; }

        [Display(Name = "Posted by")]
        [ForeignKey("PosterId")]
        public UserInfoModel PostedBy { get; set; }
    }

    public class TagModel
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name="Category")]
        public string Name { get; set; }

        public ICollection<PostModel> Posts { get; set; }

    }

    public class CommentModel
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; }
        
        public DateTime TimePosted { get; set; }

        public DateTime TimeLastEdited { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Comment { get; set; }

        public UserInfoModel User { get; set; }

        public PostModel Post { get; set; }

    }
}