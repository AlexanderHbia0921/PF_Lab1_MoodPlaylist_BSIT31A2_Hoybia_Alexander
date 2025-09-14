using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistGenerator.Data.Entities
{
    public class Mood
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string Color { get; set; } = "#3B82F6"; // Default blue
        
        // Navigation properties
        public List<SongMood> SongMoods { get; set; } = new();
        public List<Playlist> Playlists { get; set; } = new();
    }
}