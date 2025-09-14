using System.ComponentModel.DataAnnotations;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.ViewModels
{
    public class CreatePlaylistViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Mood")]
        public int MoodId { get; set; }

        public List<Mood> AvailableMoods { get; set; } = new();
    }

    public class EditPlaylistViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Mood")]
        public int MoodId { get; set; }

        public List<Mood> AvailableMoods { get; set; } = new();
    }

    public class PlaylistDetailsViewModel
    {
        public Playlist Playlist { get; set; } = null!;
        public List<Song> AvailableSongs { get; set; } = new();
        public int? SongToAdd { get; set; }
    }

    public class PlaylistListViewModel
    {
        public List<Playlist> Playlists { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public int? FilterMoodId { get; set; }
        public int? FilterByMood { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GeneratePlaylistViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Playlist Name")]
        public string PlaylistName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Select Mood")]
        public int SelectedMoodId { get; set; }

        [Required]
        [Range(5, 50)]
        [Display(Name = "Number of Songs")]
        public int SongCount { get; set; } = 10;

        public List<Mood> AvailableMoods { get; set; } = new();
        public Dictionary<int, int> MoodSongCounts { get; set; } = new();
    }

    public class PlaylistDetailViewModel
    {
        public Playlist Playlist { get; set; } = null!;
        public List<PlaylistSong> Songs { get; set; } = new();
        public bool CanEdit { get; set; }
    }

    public class EditPlaylistNameViewModel
    {
        public int PlaylistId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;
    }
}
