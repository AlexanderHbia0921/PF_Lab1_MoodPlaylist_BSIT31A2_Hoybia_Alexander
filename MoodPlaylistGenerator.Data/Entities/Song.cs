using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistGenerator.Data.Entities
{
    public class Song
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Album { get; set; }
        
        public int? Year { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? YouTubeUrl { get; set; }
        
        // Local media file properties
        [StringLength(500)]
        public string? LocalFilePath { get; set; }
        
        [StringLength(50)]
        public string? FileType { get; set; } // "audio" or "video"
        
        [StringLength(20)]
        public string? MimeType { get; set; }
        
        public long? FileSize { get; set; } // File size in bytes
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public int UserId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public List<SongMood> SongMoods { get; set; } = new();
        public List<PlaylistSong> PlaylistSongs { get; set; } = new();
    }
}