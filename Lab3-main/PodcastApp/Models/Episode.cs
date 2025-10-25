using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PodcastApp.Models
{
    public class Episode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EpisodeID { get; set; } // Auto-increment primary key

        [Required]
        [ForeignKey("Podcast")]
        public int PodcastID { get; set; } // FK to Podcast

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue)]
        public int Duration { get; set; } // in minutes

        [Range(0, int.MaxValue)]
        public int PlayCount { get; set; } // number of viewers/listeners

        [Url]
        [StringLength(500)]
        public string? AudioFileUrl { get; set; } // Link to S3 object

        [Range(0, int.MaxValue)]
        public int NumberOfViews { get; set; } // separate view counter if needed

        [NotMapped] // ?? EF should ignore this property (since DynamoDB handles it)
        public List<Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>> Comments { get; set; } = new();


        // Navigation property
        public Podcast? Podcast { get; set; }
    }
}
