using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services.Implementations
{
    public class MoodService
    {
        private readonly ApplicationDbContext _context;

        public MoodService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await _context.Moods
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Mood?> GetMoodByIdAsync(int moodId)
        {
            return await _context.Moods
                .Include(m => m.Playlists)
                .Include(m => m.SongMoods)
                    .ThenInclude(sm => sm.Song)
                .FirstOrDefaultAsync(m => m.Id == moodId);
        }

        public async Task<List<Mood>> GetMoodsForSongAsync(int songId)
        {
            return await _context.SongMoods
                .Include(sm => sm.Mood)
                .Where(sm => sm.SongId == songId)
                .Select(sm => sm.Mood)
                .ToListAsync();
        }

        public async Task<int> GetSongCountForMoodAsync(int moodId, int userId)
        {
            return await _context.Songs
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .CountAsync();
        }

        public async Task<int> GetPlaylistCountForMoodAsync(int moodId, int userId)
        {
            return await _context.Playlists
                .Where(p => p.UserId == userId && p.MoodId == moodId)
                .CountAsync();
        }
    }
}