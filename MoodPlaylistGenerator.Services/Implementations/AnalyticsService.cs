using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Data.Entities;
using MoodPlaylistGenerator.Services.Models;

namespace MoodPlaylistGenerator.Services.Implementations
{
    public class AnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardAnalyticsAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) 
                {
                    return new DashboardViewModel
                    {
                        UserName = "Unknown User",
                        LastLogin = DateTime.UtcNow,
                        Stats = new DashboardStats(),
                        RecentSongs = new List<Song>(),
                        RecentPlaylists = new List<Playlist>(),
                        MoodAnalytics = new List<MoodAnalytic>()
                    };
                }

                var stats = await GetDashboardStatsAsync(userId);
                var recentSongs = await GetRecentSongsAsync(userId);
                var recentPlaylists = await GetRecentPlaylistsAsync(userId);
                var moodAnalytics = await GetMoodAnalyticsAsync(userId);

                return new DashboardViewModel
                {
                    Stats = stats ?? new DashboardStats(),
                    RecentSongs = recentSongs ?? new List<Song>(),
                    RecentPlaylists = recentPlaylists ?? new List<Playlist>(),
                    MoodAnalytics = moodAnalytics ?? new List<MoodAnalytic>(),
                    UserName = user.Username ?? "User",
                    LastLogin = user.LastLogin ?? user.CreatedAt
                };
            }
            catch (Exception)
            {
                // Log the exception if you have a logger
                // Return a safe default dashboard
                return new DashboardViewModel
                {
                    UserName = "User",
                    LastLogin = DateTime.UtcNow,
                    Stats = new DashboardStats(),
                    RecentSongs = new List<Song>(),
                    RecentPlaylists = new List<Playlist>(),
                    MoodAnalytics = new List<MoodAnalytic>()
                };
            }
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(int userId)
        {
            try
            {
                // Get user for days active calculation
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new DashboardStats();
                }

                var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
                
                // Get basic counts in parallel for better performance
                var totalSongsTask = _context.Songs.CountAsync(s => s.UserId == userId);
                var totalPlaylistsTask = _context.Playlists.CountAsync(p => p.UserId == userId);
                var songsThisWeekTask = _context.Songs
                    .CountAsync(s => s.UserId == userId && s.CreatedAt >= oneWeekAgo);
                var playlistsThisWeekTask = _context.Playlists
                    .CountAsync(p => p.UserId == userId && p.CreatedAt >= oneWeekAgo);

                await Task.WhenAll(totalSongsTask, totalPlaylistsTask, songsThisWeekTask, playlistsThisWeekTask);

                // Calculate unique moods used
                var uniqueMoods = 0;
                try
                {
                    uniqueMoods = await _context.SongMoods
                        .Where(sm => sm.Song.UserId == userId)
                        .Select(sm => sm.MoodId)
                        .Distinct()
                        .CountAsync();
                }
                catch
                {
                    uniqueMoods = 0;
                }

                // Calculate days active (days since account creation)
                var daysActive = (DateTime.UtcNow - user.CreatedAt).Days;

                // Get most used mood with error handling
                var mostUsedMood = "None";
                try
                {
                    mostUsedMood = await _context.SongMoods
                        .Where(sm => sm.Song.UserId == userId)
                        .GroupBy(sm => sm.Mood.Name)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefaultAsync() ?? "None";
                }
                catch
                {
                    mostUsedMood = "None";
                }

                // Calculate average playlist length with error handling
                var averagePlaylistLength = 0.0;
                try
                {
                    var playlistLengths = await _context.PlaylistSongs
                        .Where(ps => ps.Playlist.UserId == userId)
                        .GroupBy(ps => ps.PlaylistId)
                        .Select(g => g.Count())
                        .ToListAsync();
                    
                    averagePlaylistLength = playlistLengths.Count > 0 ? playlistLengths.Average() : 0;
                }
                catch
                {
                    averagePlaylistLength = 0;
                }

                return new DashboardStats
                {
                    TotalSongs = totalSongsTask.Result,
                    TotalPlaylists = totalPlaylistsTask.Result,
                    FavoriteMoods = uniqueMoods,
                    DaysActive = Math.Max(daysActive, 0),
                    SongsThisWeek = songsThisWeekTask.Result,
                    PlaylistsThisWeek = playlistsThisWeekTask.Result,
                    MostUsedMood = mostUsedMood,
                    AveragePlaylistLength = Math.Round(averagePlaylistLength, 1)
                };
            }
            catch (Exception)
            {
                // Log the exception if you have a logger
                // Return safe default stats
                return new DashboardStats();
            }
        }

        public async Task<List<Song>> GetRecentSongsAsync(int userId, int count = 5)
        {
            try
            {
                return await _context.Songs
                    .Where(s => s.UserId == userId)
                    .Include(s => s.SongMoods)
                    .ThenInclude(sm => sm.Mood)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Log the exception if you have a logger
                return new List<Song>();
            }
        }

        public async Task<List<Playlist>> GetRecentPlaylistsAsync(int userId, int count = 5)
        {
            try
            {
                return await _context.Playlists
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Mood)
                    .Include(p => p.PlaylistSongs)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Log the exception if you have a logger
                return new List<Playlist>();
            }
        }

        public async Task<List<MoodAnalytic>> GetMoodAnalyticsAsync(int userId)
        {
            try
            {
                var moodData = await _context.Moods
                    .Select(m => new MoodAnalytic
                    {
                        MoodId = m.Id,
                        MoodName = m.Name ?? "Unknown",
                        MoodDescription = m.Description ?? "",
                        SongCount = m.SongMoods.Count(sm => sm.Song != null && sm.Song.UserId == userId),
                        PlaylistCount = m.Playlists.Count(p => p != null && p.UserId == userId),
                        Emoji = GetMoodEmoji(m.Name ?? "unknown"),
                        ColorClass = GetMoodColorClass(m.Name ?? "unknown")
                    })
                    .Where(m => m.SongCount > 0 || m.PlaylistCount > 0)
                    .OrderByDescending(m => m.SongCount + m.PlaylistCount)
                    .ToListAsync();

                // If no mood analytics found, return popular moods with zero counts
                if (!moodData.Any())
                {
                    var defaultMoods = await _context.Moods
                        .Take(6)
                        .Select(m => new MoodAnalytic
                        {
                            MoodId = m.Id,
                            MoodName = m.Name ?? "Unknown",
                            MoodDescription = m.Description ?? "",
                            SongCount = 0,
                            PlaylistCount = 0,
                            Emoji = GetMoodEmoji(m.Name ?? "unknown"),
                            ColorClass = GetMoodColorClass(m.Name ?? "unknown")
                        })
                        .ToListAsync();
                    
                    return defaultMoods;
                }

                return moodData;
            }
            catch (Exception)
            {
                // Log the exception if you have a logger
                // Return empty list as fallback
                return new List<MoodAnalytic>();
            }
        }

        private static string GetMoodEmoji(string moodName)
        {
            return moodName.ToLower() switch
            {
                "happy" => "😊",
                "sad" => "😢",
                "energetic" => "⚡",
                "calm" or "relaxed" => "😌",
                "romantic" => "💝",
                "motivational" => "🔥",
                "angry" => "😠",
                "peaceful" => "🕊️",
                "excited" => "🎉",
                "nostalgic" => "📸",
                _ => "🎵"
            };
        }

        private static string GetMoodColorClass(string moodName)
        {
            return moodName.ToLower() switch
            {
                "happy" => "warning",
                "sad" => "info",
                "energetic" => "danger",
                "calm" or "relaxed" => "primary",
                "romantic" => "pink",
                "motivational" => "success",
                "angry" => "danger",
                "peaceful" => "success",
                "excited" => "warning",
                "nostalgic" => "secondary",
                _ => "primary"
            };
        }
    }
}