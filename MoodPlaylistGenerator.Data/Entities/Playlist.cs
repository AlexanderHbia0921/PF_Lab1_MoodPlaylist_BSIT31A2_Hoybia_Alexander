using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistGenerator.Data.Entities
{
    public class Playlist
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int UserId { get; set; }
        public int MoodId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Mood Mood { get; set; } = null!;
        public List<PlaylistSong> PlaylistSongs { get; set; } = new();
    }
}