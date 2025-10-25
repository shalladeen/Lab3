using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PodcastApp.Models
{
    public class Podcast
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PodcastID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public Guid CreatorID { get; set; }   // ? Changed to Guid

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<Episode>? Episodes { get; set; }
    }
}
