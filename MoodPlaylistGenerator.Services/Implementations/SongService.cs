using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace MoodPlaylistGenerator.Services.Implementations
{
    public class SongService
    {
        private readonly ApplicationDbContext _context;
        private const string RickRollVideoId = "dQw4w9WgXcQ"; // Rick Roll video ID
        private readonly string[] AllowedAudioExtensions = {".mp3", ".wav", ".ogg", ".m4a", ".aac"};
        private readonly string[] AllowedVideoExtensions = {".mp4", ".avi", ".mov", ".wmv", ".webm"};
        private const long MaxFileSize = 100 * 1024 * 1024; // 100MB

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
            int? year, string? youTubeUrl, int userId, List<int> moodIds,
            IFormFile? mediaFile = null, string? uploadsPath = null)
        {
            // Validate that the User exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} does not exist.", nameof(userId));
            }

            // Handle file upload if provided
            string? localFilePath = null;
            string? fileType = null;
            string? mimeType = null;
            long? fileSize = null;

            if (mediaFile != null && mediaFile.Length > 0 && !string.IsNullOrEmpty(uploadsPath))
            {
                var uploadResult = await SaveMediaFileAsync(mediaFile, uploadsPath);
                localFilePath = uploadResult.FilePath;
                fileType = uploadResult.FileType;
                mimeType = uploadResult.MimeType;
                fileSize = uploadResult.FileSize;
            }

            var song = new Song
            {
                Title = title,
                Artist = artist,
                Album = album,
                Year = year,
                YouTubeUrl = youTubeUrl,
                LocalFilePath = localFilePath,
                FileType = fileType,
                MimeType = mimeType,
                FileSize = fileSize,
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
            string? album, int? year, string? youTubeUrl, List<int> moodIds,
            IFormFile? mediaFile = null, string? uploadsPath = null)
        {
            var song = await _context.Songs
                .Include(s => s.SongMoods)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return false;

            // Handle file upload if provided
            if (mediaFile != null && mediaFile.Length > 0 && !string.IsNullOrEmpty(uploadsPath))
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(song.LocalFilePath))
                {
                    await DeleteMediaFileAsync(song.LocalFilePath, uploadsPath);
                }

                // Save new file
                var uploadResult = await SaveMediaFileAsync(mediaFile, uploadsPath);
                song.LocalFilePath = uploadResult.FilePath;
                song.FileType = uploadResult.FileType;
                song.MimeType = uploadResult.MimeType;
                song.FileSize = uploadResult.FileSize;
            }

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

        // File upload helper methods
        public async Task<(string FilePath, string FileType, string MimeType, long FileSize)> SaveMediaFileAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            if (file.Length > MaxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isAudio = AllowedAudioExtensions.Contains(extension);
            var isVideo = AllowedVideoExtensions.Contains(extension);

            if (!isAudio && !isVideo)
                throw new ArgumentException("File type not supported. Allowed audio: " + string.Join(", ", AllowedAudioExtensions) + ". Allowed video: " + string.Join(", ", AllowedVideoExtensions));

            var fileType = isAudio ? "audio" : "video";
            var uploadsPath = Path.Combine(webRootPath, "uploads", fileType);
            
            // Ensure directory exists
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            var relativePath = Path.Combine("uploads", fileType, uniqueFileName).Replace("\\", "/");

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return (relativePath, fileType, file.ContentType, file.Length);
        }

        public async Task DeleteMediaFileAsync(string relativePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            var fullPath = Path.Combine(webRootPath, relativePath.Replace("/", "\\"));
            
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                    await Task.CompletedTask; // Make it async compatible
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file {fullPath}: {ex.Message}");
                }
            }
        }

        public bool IsValidMediaFile(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedAudioExtensions.Contains(extension) || AllowedVideoExtensions.Contains(extension);
        }

        public string GetMediaUrl(Song song)
        {
            // Priority: Local file > YouTube > Rick Roll fallback
            if (!string.IsNullOrEmpty(song.LocalFilePath))
            {
                return $"/{song.LocalFilePath}";
            }
            
            if (!string.IsNullOrEmpty(song.YouTubeUrl))
            {
                return song.YouTubeUrl;
            }

            // Rick Roll fallback - return YouTube embed URL
            return $"https://www.youtube.com/embed/{RickRollVideoId}";
        }

        public string GetMediaType(Song song)
        {
            if (!string.IsNullOrEmpty(song.LocalFilePath))
            {
                return song.FileType ?? "audio"; // Default to audio if type is unknown
            }
            
            return "youtube"; // YouTube or Rick Roll fallback
        }

        public bool HasLocalMedia(Song song)
        {
            return !string.IsNullOrEmpty(song.LocalFilePath);
        }
    }
}
