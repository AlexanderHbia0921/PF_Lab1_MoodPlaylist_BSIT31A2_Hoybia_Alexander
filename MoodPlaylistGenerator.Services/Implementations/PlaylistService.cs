using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services.Implementations
{
    public class PlaylistService
    {
        private readonly ApplicationDbContext _context;

        public PlaylistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Playlist>> GetUserPlaylistsAsync(int userId)
        {
            return await _context.Playlists
                .Include(p => p.Mood)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Playlist?> GetPlaylistByIdAsync(int playlistId, int userId)
        {
            return await _context.Playlists
                .Include(p => p.Mood)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);
        }

        public async Task<Playlist> CreatePlaylistAsync(string name, string? description, int userId, int moodId)
        {
            // Validate that the User exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} does not exist.", nameof(userId));
            }

            // Validate that the Mood exists
            var moodExists = await _context.Moods.AnyAsync(m => m.Id == moodId);
            if (!moodExists)
            {
                throw new ArgumentException($"Mood with ID {moodId} does not exist.", nameof(moodId));
            }

            var playlist = new Playlist
            {
                Name = name,
                Description = description,
                UserId = userId,
                MoodId = moodId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return playlist;
        }

        public async Task<bool> UpdatePlaylistAsync(int playlistId, int userId, string name, string? description, int moodId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return false;

            playlist.Name = name;
            playlist.Description = description;
            playlist.MoodId = moodId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlaylistAsync(int playlistId, int userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return false;

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddSongToPlaylistAsync(int playlistId, int songId, int userId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return false;

            var song = await _context.Songs.FindAsync(songId);
            if (song == null)
                return false;

            // Check if song is already in playlist
            if (playlist.PlaylistSongs.Any(ps => ps.SongId == songId))
                return false;

            var nextOrder = playlist.PlaylistSongs.Count > 0 
                ? playlist.PlaylistSongs.Max(ps => ps.Order) + 1 
                : 1;

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                Order = nextOrder
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(int playlistId, int songId, int userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return false;

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null)
                return false;

            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderPlaylistSongsAsync(int playlistId, int userId, Dictionary<int, int> songOrderMap)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return false;

            foreach (var playlistSong in playlist.PlaylistSongs)
            {
                if (songOrderMap.ContainsKey(playlistSong.SongId))
                {
                    playlistSong.Order = songOrderMap[playlistSong.SongId];
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Playlist> GeneratePlaylistAsync(int userId, int moodId, int songCount, string playlistName)
        {
            // Get songs for the specified mood
            var songs = await _context.Songs
                .Include(s => s.SongMoods)
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .OrderBy(x => Guid.NewGuid()) // Random selection
                .Take(songCount)
                .ToListAsync();

            if (songs.Count == 0)
                throw new InvalidOperationException("No songs available for the selected mood.");

            // Create the playlist
            var playlist = await CreatePlaylistAsync(playlistName, $"Auto-generated playlist for mood", userId, moodId);

            // Add songs to playlist
            var order = 1;
            foreach (var song in songs)
            {
                var playlistSong = new PlaylistSong
                {
                    PlaylistId = playlist.Id,
                    SongId = song.Id,
                    Order = order++
                };
                _context.PlaylistSongs.Add(playlistSong);
            }

            await _context.SaveChangesAsync();

            // Reload playlist with songs
            return await GetPlaylistByIdAsync(playlist.Id, userId) ?? playlist;
        }

        public async Task<Dictionary<int, int>> GetMoodSongCountsAsync(int userId)
        {
            var moodCounts = await _context.Songs
                .Where(s => s.UserId == userId)
                .SelectMany(s => s.SongMoods)
                .GroupBy(sm => sm.MoodId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return moodCounts;
        }

        public async Task<Playlist?> UpdatePlaylistNameAsync(int playlistId, int userId, string name)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return null;

            playlist.Name = name;
            await _context.SaveChangesAsync();

            return playlist;
        }
    }
}