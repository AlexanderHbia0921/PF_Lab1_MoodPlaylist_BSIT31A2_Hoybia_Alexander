using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodPlaylistGenerator.Services.Implementations;
using MoodPlaylistGenerator.ViewModels;
using System.Security.Claims;

namespace MoodPlaylistGenerator.Controllers
{
    [Authorize]
    public class PlaylistController : Controller
    {
        private readonly PlaylistService _playlistService;
        private readonly SongService _songService;
        private readonly MoodService _moodService;

        public PlaylistController(PlaylistService playlistService, SongService songService, MoodService moodService)
        {
            _playlistService = playlistService;
            _songService = songService;
            _moodService = moodService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        public async Task<IActionResult> Index(int? filterByMood, string? searchTerm)
        {
            var userId = GetCurrentUserId();
            var playlists = await _playlistService.GetUserPlaylistsAsync(userId);
            var moods = await _moodService.GetAllMoodsAsync();

            if (filterByMood.HasValue)
            {
                playlists = playlists.Where(p => p.MoodId == filterByMood.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                playlists = playlists.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                                   .ToList();
            }

            var viewModel = new PlaylistListViewModel
            {
                Playlists = playlists,
                Moods = moods,
                FilterByMood = filterByMood,
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id, userId);

            if (playlist == null)
                return NotFound();

            var availableSongs = await _songService.GetUserSongsAsync(userId);

            var viewModel = new PlaylistDetailsViewModel
            {
                Playlist = playlist,
                AvailableSongs = availableSongs
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var moods = await _moodService.GetAllMoodsAsync();

            var viewModel = new CreatePlaylistViewModel
            {
                AvailableMoods = moods
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlaylistViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var playlist = await _playlistService.CreatePlaylistAsync(model.Name, model.Description, userId, model.MoodId);
                
                TempData["Success"] = "Playlist created successfully!";
                return RedirectToAction(nameof(Details), new { id = playlist.Id });
            }

            model.AvailableMoods = await _moodService.GetAllMoodsAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id, userId);

            if (playlist == null)
                return NotFound();

            var moods = await _moodService.GetAllMoodsAsync();

            var viewModel = new EditPlaylistViewModel
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Description = playlist.Description,
                MoodId = playlist.MoodId,
                AvailableMoods = moods
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPlaylistViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var success = await _playlistService.UpdatePlaylistAsync(model.Id, userId, model.Name, model.Description, model.MoodId);

                if (success)
                {
                    TempData["Success"] = "Playlist updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                ModelState.AddModelError("", "Failed to update playlist.");
            }

            model.AvailableMoods = await _moodService.GetAllMoodsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _playlistService.DeletePlaylistAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Playlist deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete playlist.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var userId = GetCurrentUserId();
            var success = await _playlistService.AddSongToPlaylistAsync(playlistId, songId, userId);

            if (success)
            {
                TempData["Success"] = "Song added to playlist!";
            }
            else
            {
                TempData["Error"] = "Failed to add song to playlist. It may already be in the playlist.";
            }

            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            var userId = GetCurrentUserId();
            var success = await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId, userId);

            if (success)
            {
                TempData["Success"] = "Song removed from playlist!";
            }
            else
            {
                TempData["Error"] = "Failed to remove song from playlist.";
            }

            return RedirectToAction(nameof(Details), new { id = playlistId });
        }
    }
}