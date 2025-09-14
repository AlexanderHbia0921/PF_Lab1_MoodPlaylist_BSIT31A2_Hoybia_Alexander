using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services.Implementations
{
    public class SongService
    {
        private readonly ApplicationDbContext _context;

        public SongService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Song>> GetUserSongsAsync(int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                    .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Song?> GetSongByIdAsync(int songId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                    .ThenInclude(sm => sm.Mood)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);
        }

        public async Task<List<Song>> SearchSongsAsync(int userId, string searchTerm)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                    .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId &&
                           (s.Title.Contains(searchTerm) ||
                            s.Artist.Contains(searchTerm) ||
                            (s.Album != null && s.Album.Contains(searchTerm))))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Song>> GetSongsByMoodAsync(int userId, int moodId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                    .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Song> CreateSongAsync(
            string title, string artist, string? album,
            int? year, string? youTubeUrl, int userId, List<int> moodIds)
        {
            // Validate that the User exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} does not exist.", nameof(userId));
            }

            var song = new Song
            {
                Title = title,
                Artist = artist,
                Album = album,
                Year = year,
                YouTubeUrl = youTubeUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Save the song first
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Only keep valid mood IDs
            var validMoodIds = await _context.Moods
                .Where(m => moodIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            Console.WriteLine("Valid Mood Ids: " + string.Join(",", validMoodIds));

            // Add SongMood links only if valid moods exist
            if (validMoodIds.Any())
            {
                foreach (var moodId in validMoodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return song;
        }

        public async Task<bool> UpdateSongAsync(
            int songId, int userId, string title, string artist,
            string? album, int? year, string? youTubeUrl, List<int> moodIds)
        {
            var song = await _context.Songs
                .Include(s => s.SongMoods)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return false;

            // Update properties
            song.Title = title;
            song.Artist = artist;
            song.Album = album;
            song.Year = year;
            song.YouTubeUrl = youTubeUrl;

            // Remove old moods
            _context.SongMoods.RemoveRange(song.SongMoods);

            // Validate new moods
            var validMoodIds = await _context.Moods
                .Where(m => moodIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            Console.WriteLine("Valid Mood Ids (Update): " + string.Join(",", validMoodIds));

            if (validMoodIds.Any())
            {
                foreach (var moodId in validMoodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSongAsync(int songId, int userId)
        {
            var song = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return false;

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<string?> ExtractVideoIdFromUrl(string youTubeUrl)
        {
            if (string.IsNullOrWhiteSpace(youTubeUrl))
                return Task.FromResult<string?>(null);

            try
            {
                var uri = new Uri(youTubeUrl);

                if (uri.Host.Contains("youtube.com"))
                {
                    var query = uri.Query;
                    if (query.Contains("v="))
                    {
                        var start = query.IndexOf("v=") + 2;
                        var end = query.IndexOf("&", start);
                        var videoId = end == -1 ? query.Substring(start) : query.Substring(start, end - start);
                        return Task.FromResult<string?>(videoId);
                    }
                }
                else if (uri.Host.Contains("youtu.be"))
                {
                    return Task.FromResult<string?>(uri.AbsolutePath.TrimStart('/'));
                }
            }
            catch (UriFormatException)
            {
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(null);
        }

        public Task<bool> IsValidYouTubeUrl(string youTubeUrl)
        {
            if (string.IsNullOrWhiteSpace(youTubeUrl))
                return Task.FromResult(false);

            try
            {
                var uri = new Uri(youTubeUrl);
                return Task.FromResult(uri.Host.Contains("youtube.com") ||
                       uri.Host.Contains("youtu.be"));
            }
            catch (UriFormatException)
            {
                return Task.FromResult(false);
            }
        }
    }
}