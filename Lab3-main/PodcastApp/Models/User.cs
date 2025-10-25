using System;
using System.ComponentModel.DataAnnotations;

namespace PodcastApp.Models
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; } = Guid.NewGuid(); // PK (GUID)

        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(4000)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Listener; // Enum: Podcaster, Listener, Admin
    }

    public enum UserRole
    {
        Podcaster = 0,
        Listener = 1,
        Admin = 2
    }
}
