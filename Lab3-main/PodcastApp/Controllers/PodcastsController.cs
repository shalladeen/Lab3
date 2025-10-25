using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PodcastApp.Data;
using PodcastApp.Models;
using PodcastApp.Services;

namespace PodcastApp.Controllers
{
    public class PodcastsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly S3Service _s3;

        public PodcastsController(ApplicationDbContext context, S3Service s3)
        {
            _context = context;
            _s3 = s3;
        }

        // ============================================================
        // GET: Podcasts
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var podcasts = await _context.Podcasts
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(podcasts);
        }

        // ============================================================
        // GET: Podcasts/Create
        // ============================================================
        public IActionResult Create()
        {
            // Only allow podcasters or admins
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || (role != "Podcaster" && role != "Admin"))
            {
                TempData["Error"] = "You must be logged in as a Podcaster to create a podcast.";
                return RedirectToAction("Login", "Users");
            }

            return View();
        }

        // ============================================================
        // POST: Podcasts/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] Podcast podcast, IFormFile? media)
        {
            // ? get the logged in user's ID
            var userIdStr = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "You must be logged in to create a podcast.";
                return RedirectToAction("Login", "Users");
            }

            // ? Convert safely to Guid
            if (!Guid.TryParse(userIdStr, out Guid creatorGuid))
            {
                TempData["Error"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "Users");
            }

            // ? assign CreatorID correctly
            podcast.CreatorID = creatorGuid;
            podcast.CreatedDate = DateTime.UtcNow;

            // ? optional: upload audio/video file to S3
            if (media != null && media.Length > 0)
            {
                using var stream = media.OpenReadStream();
                var key = $"podcasts/{creatorGuid}/{Guid.NewGuid()}_{media.FileName}";
                var s3Url = await _s3.UploadAsync(stream, key);
                podcast.Description = (podcast.Description ?? "") + $"\n?? Media: {s3Url}";
            }

            // ? save to database
            _context.Podcasts.Add(podcast);
            await _context.SaveChangesAsync();

            TempData["Message"] = "? Podcast created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Podcasts/Edit/{id}
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var podcast = await _context.Podcasts.FindAsync(id);
            if (podcast == null)
                return NotFound();

            return View(podcast);
        }

        // ============================================================
        // POST: Podcasts/Edit/{id}
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PodcastID,Title,Description,CreatorID,CreatedDate")] Podcast podcast, IFormFile? media)
        {
            if (id != podcast.PodcastID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(podcast);

            try
            {
                // Optional: upload new media
                if (media != null && media.Length > 0)
                {
                    using var stream = media.OpenReadStream();
                    var key = $"podcasts/{podcast.CreatorID}/{Guid.NewGuid()}_{media.FileName}";
                    var s3Url = await _s3.UploadAsync(stream, key);
                    podcast.Description = (podcast.Description ?? "") + $"\n(Updated Media: {s3Url})";
                }

                _context.Update(podcast);
                await _context.SaveChangesAsync();

                TempData["Message"] = "? Podcast updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Podcasts.Any(e => e.PodcastID == id))
                    return NotFound();
                else
                    throw;
            }
        }

        // ============================================================
        // GET: Podcasts/Delete/{id}
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var podcast = await _context.Podcasts
                .FirstOrDefaultAsync(m => m.PodcastID == id);

            if (podcast == null)
                return NotFound();

            return View(podcast);
        }

        // ============================================================
        // POST: Podcasts/Delete/{id}
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var podcast = await _context.Podcasts.FindAsync(id);
            if (podcast != null)
                _context.Podcasts.Remove(podcast);

            await _context.SaveChangesAsync();

            TempData["Message"] = "?? Podcast deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
