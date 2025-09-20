using System.ComponentModel.DataAnnotations;
using MoodPlaylistGenerator.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace MoodPlaylistGenerator.ViewModels
{
    public class CreateSongViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Album { get; set; }

        [Range(1900, 2100)]
        public int? Year { get; set; }

        [Url]
        [StringLength(500)]
        [Display(Name = "YouTube URL")]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Media File")]
        public IFormFile? MediaFile { get; set; }

        [Required]
        [Display(Name = "Moods")]
        public List<int> SelectedMoodIds { get; set; } = new();

        public List<Mood> AvailableMoods { get; set; } = new();
    }

    public class EditSongViewModel
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

        [Range(1900, 2100)]
        public int? Year { get; set; }

        [Url]
        [StringLength(500)]
        [Display(Name = "YouTube URL")]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Media File")]
        public IFormFile? MediaFile { get; set; }

        [Required]
        [Display(Name = "Moods")]
        public List<int> SelectedMoodIds { get; set; } = new();

        public List<Mood> AvailableMoods { get; set; } = new();
    }

    public class SongListViewModel
    {
        public List<Song> Songs { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public int? SelectedMoodId { get; set; }
        public int? FilterByMood { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class SongDetailsViewModel
    {
        public Song Song { get; set; } = null!;
        public List<Playlist> UserPlaylists { get; set; } = new();
        public string? YouTubeEmbedUrl { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty; // "audio", "video", or "youtube"
        public bool HasLocalMedia { get; set; }
    }

    public class SongDetailViewModel
    {
        public Song Song { get; set; } = null!;
        public string? YouTubeVideoId { get; set; }
        public List<Mood> AssignedMoods { get; set; } = new();
    }
}
