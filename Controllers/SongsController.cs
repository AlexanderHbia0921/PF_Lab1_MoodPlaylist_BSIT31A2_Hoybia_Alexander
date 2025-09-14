using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MoodPlaylistGenerator.Services.Implementations;
using MoodPlaylistGenerator.ViewModels;

namespace MoodPlaylistGenerator.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly SongService _songService;
        private readonly MoodService _moodService;

        public SongsController(SongService songService, MoodService moodService)
        {
            _songService = songService;
            _moodService = moodService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User is not properly authenticated.");
            }
            return userId;
        }

        public async Task<IActionResult> Index(int? moodId, string? search)
        {
            var userId = GetCurrentUserId();
            var songs = await _songService.GetUserSongsAsync(userId);
            var moods = await _moodService.GetAllMoodsAsync();

            // Filter by mood if selected
            if (moodId.HasValue)
            {
                songs = await _songService.GetSongsByMoodAsync(userId, moodId.Value);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                songs = songs.Where(s => 
                    s.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Artist.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var viewModel = new SongListViewModel
            {
                Songs = songs,
                Moods = moods,
                SelectedMoodId = moodId,
                SearchTerm = search ?? ""
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);
            
            if (song == null)
                return NotFound();

            var viewModel = new SongDetailViewModel
            {
                Song = song,
                YouTubeVideoId = await _songService.ExtractVideoIdFromUrl(song.YouTubeUrl ?? ""),
                AssignedMoods = song.SongMoods.Select(sm => sm.Mood).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateSongViewModel
            {
                AvailableMoods = await _moodService.GetAllMoodsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSongViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableMoods = await _moodService.GetAllMoodsAsync();
                return View(model);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                await _songService.CreateSongAsync(
                    model.Title, 
                    model.Artist, 
                    null, // album
                    null, // year
                    model.YouTubeUrl, 
                    userId, 
                    model.SelectedMoodIds);

                TempData["SuccessMessage"] = "Song added successfully!";
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while adding the song.");
                model.AvailableMoods = await _moodService.GetAllMoodsAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);
            
            if (song == null)
                return NotFound();

            var viewModel = new EditSongViewModel
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                YouTubeUrl = song.YouTubeUrl,
                SelectedMoodIds = song.SongMoods.Select(sm => sm.MoodId).ToList(),
                AvailableMoods = await _moodService.GetAllMoodsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSongViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableMoods = await _moodService.GetAllMoodsAsync();
                return View(model);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                var success = await _songService.UpdateSongAsync(
                    model.Id, 
                    userId, 
                    model.Title, 
                    model.Artist, 
                    null, // album
                    null, // year
                    model.YouTubeUrl, 
                    model.SelectedMoodIds);

                if (!success)
                    return NotFound();

                TempData["SuccessMessage"] = "Song updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the song.");
                model.AvailableMoods = await _moodService.GetAllMoodsAsync();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _songService.DeleteSongAsync(id, userId);
            
            if (!success)
                return NotFound();

            TempData["SuccessMessage"] = "Song deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
