using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrightIdeas.Models {
    public class Idea {

        [Key]
        public int IdeaId { get; set; }

        [Required (ErrorMessage = "An Idea is required.")]
        [MinLength (5, ErrorMessage = "Idea must be at least 5 characters long.")]
        [Display (Name = "Idea:")]
        public string Title { get; set; }
        public int? UserId { get; set; }
        public User? Creator { get; set; }
        public List<Like>? Likes { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}