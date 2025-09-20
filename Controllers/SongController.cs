using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodPlaylistGenerator.Services.Implementations;
using MoodPlaylistGenerator.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace MoodPlaylistGenerator.Controllers
{
    [Authorize]
    public class SongController : Controller
    {
        private readonly SongService _songService;
        private readonly MoodService _moodService;
        private readonly PlaylistService _playlistService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SongController(SongService songService, MoodService moodService, PlaylistService playlistService, IWebHostEnvironment webHostEnvironment)
        {
            _songService = songService;
            _moodService = moodService;
            _playlistService = playlistService;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId == 0)
            {
                throw new UnauthorizedAccessException("User is not properly authenticated or user ID is invalid.");
            }
            return userId;
        }

        public async Task<IActionResult> Index(int? filterByMood, string? searchTerm)
        {
            var userId = GetCurrentUserId();
            var songs = string.IsNullOrWhiteSpace(searchTerm) 
                ? await _songService.GetUserSongsAsync(userId)
                : await _songService.SearchSongsAsync(userId, searchTerm);

            if (filterByMood.HasValue)
            {
                songs = await _songService.GetSongsByMoodAsync(userId, filterByMood.Value);
            }

            var moods = await _moodService.GetAllMoodsAsync();

            var viewModel = new SongListViewModel
            {
                Songs = songs,
                Moods = moods,
                FilterByMood = filterByMood,
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);

            if (song == null)
                return NotFound();

            var userPlaylists = await _playlistService.GetUserPlaylistsAsync(userId);
            string? youTubeEmbedUrl = null;

            if (!string.IsNullOrWhiteSpace(song.YouTubeUrl))
            {
                var videoId = await _songService.ExtractVideoIdFromUrl(song.YouTubeUrl);
                if (!string.IsNullOrEmpty(videoId))
                {
                    youTubeEmbedUrl = $"https://www.youtube.com/embed/{videoId}";
                }
            }

            var viewModel = new SongDetailsViewModel
            {
                Song = song,
                UserPlaylists = userPlaylists,
                YouTubeEmbedUrl = youTubeEmbedUrl,
                MediaUrl = _songService.GetMediaUrl(song),
                MediaType = _songService.GetMediaType(song),
                HasLocalMedia = _songService.HasLocalMedia(song)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var moods = await _moodService.GetAllMoodsAsync();

            var viewModel = new CreateSongViewModel
            {
                AvailableMoods = moods
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSongViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate YouTube URL if provided
                if (!string.IsNullOrWhiteSpace(model.YouTubeUrl))
                {
                    var isValidUrl = await _songService.IsValidYouTubeUrl(model.YouTubeUrl);
                    if (!isValidUrl)
                    {
                        ModelState.AddModelError(nameof(model.YouTubeUrl), "Please enter a valid YouTube URL.");
                    }
                }

                // Validate media file if provided
                if (model.MediaFile != null)
                {
                    if (!_songService.IsValidMediaFile(model.MediaFile))
                    {
                        ModelState.AddModelError(nameof(model.MediaFile), "Invalid media file. Supported formats: MP3, WAV, OGG, M4A, AAC, MP4, AVI, MOV, WMV, WEBM. Max size: 100MB.");
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var userId = GetCurrentUserId();
                        var song = await _songService.CreateSongAsync(model.Title, model.Artist, model.Album, model.Year, model.YouTubeUrl, userId, model.SelectedMoodIds, model.MediaFile, _webHostEnvironment.WebRootPath);
                        
                        TempData["Success"] = "Song created successfully!";
                        return RedirectToAction(nameof(Details), new { id = song.Id });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error creating song: {ex.Message}");
                    }
                }
            }

            model.AvailableMoods = await _moodService.GetAllMoodsAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);

            if (song == null)
                return NotFound();

            var moods = await _moodService.GetAllMoodsAsync();
            var selectedMoodIds = song.SongMoods.Select(sm => sm.MoodId).ToList();

            var viewModel = new EditSongViewModel
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Year = song.Year,
                YouTubeUrl = song.YouTubeUrl,
                SelectedMoodIds = selectedMoodIds,
                AvailableMoods = moods
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditSongViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate YouTube URL if provided
                if (!string.IsNullOrWhiteSpace(model.YouTubeUrl))
                {
                    var isValidUrl = await _songService.IsValidYouTubeUrl(model.YouTubeUrl);
                    if (!isValidUrl)
                    {
                        ModelState.AddModelError(nameof(model.YouTubeUrl), "Please enter a valid YouTube URL.");
                    }
                }

                // Validate media file if provided
                if (model.MediaFile != null)
                {
                    if (!_songService.IsValidMediaFile(model.MediaFile))
                    {
                        ModelState.AddModelError(nameof(model.MediaFile), "Invalid media file. Supported formats: MP3, WAV, OGG, M4A, AAC, MP4, AVI, MOV, WMV, WEBM. Max size: 100MB.");
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var userId = GetCurrentUserId();
                        var success = await _songService.UpdateSongAsync(model.Id, userId, model.Title, model.Artist, model.Album, model.Year, model.YouTubeUrl, model.SelectedMoodIds, model.MediaFile, _webHostEnvironment.WebRootPath);

                        if (success)
                        {
                            TempData["Success"] = "Song updated successfully!";
                            return RedirectToAction(nameof(Details), new { id = model.Id });
                        }

                        ModelState.AddModelError("", "Failed to update song.");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error updating song: {ex.Message}");
                    }
                }
            }

            model.AvailableMoods = await _moodService.GetAllMoodsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _songService.DeleteSongAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Song deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete song.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}