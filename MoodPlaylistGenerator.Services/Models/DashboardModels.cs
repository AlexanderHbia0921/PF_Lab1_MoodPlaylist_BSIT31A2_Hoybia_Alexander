using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services.Models
{
    public class DashboardViewModel
    {
        public DashboardStats Stats { get; set; } = new();
        public List<Song> RecentSongs { get; set; } = new();
        public List<Playlist> RecentPlaylists { get; set; } = new();
        public List<MoodAnalytic> MoodAnalytics { get; set; } = new();
        public string UserName { get; set; } = string.Empty;
        public DateTime LastLogin { get; set; }
    }

    public class DashboardStats
    {
        public int TotalSongs { get; set; }
        public int TotalPlaylists { get; set; }
        public int FavoriteMoods { get; set; }
        public int DaysActive { get; set; }
        public int SongsThisWeek { get; set; }
        public int PlaylistsThisWeek { get; set; }
        public string MostUsedMood { get; set; } = string.Empty;
        public double AveragePlaylistLength { get; set; }
    }

    public class MoodAnalytic
    {
        public int MoodId { get; set; }
        public string MoodName { get; set; } = string.Empty;
        public string MoodDescription { get; set; } = string.Empty;
        public int SongCount { get; set; }
        public int PlaylistCount { get; set; }
        public string Emoji { get; set; } = "🎵";
        public string ColorClass { get; set; } = "primary";
    }
}