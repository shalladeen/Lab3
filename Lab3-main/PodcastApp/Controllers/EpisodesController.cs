using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PodcastApp.Data;
using PodcastApp.Models;
using PodcastApp.Services;

namespace PodcastApp.Controllers
{
    public class EpisodesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EpisodesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------------
        // GET: Episodes
        // -------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var episodes = await _context.Episodes
                                         .Include(e => e.Podcast)
                                         .OrderByDescending(e => e.ReleaseDate)
                                         .ToListAsync();
            return View(episodes);
        }

        // -------------------------------------------------------------
        // GET: Episodes/Details/{id}
        // -------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ep = await _context.Episodes
                                   .Include(e => e.Podcast)
                                   .FirstOrDefaultAsync(m => m.EpisodeID == id);
            if (ep == null) return NotFound();

            // Increment view count
            ep.PlayCount++;
            ep.NumberOfViews++;
            await _context.SaveChangesAsync();

            // Load comments (from DynamoDB)
            var commentsSvc = HttpContext.RequestServices.GetRequiredService<DynamoCommentsService>();
            ViewData["Comments"] = await commentsSvc.ListForEpisodeAsync(ep.EpisodeID.ToString());

            return View(ep);
        }

        // -------------------------------------------------------------
        // GET: Episodes/Create
        // -------------------------------------------------------------
        public IActionResult Create()
        {
            // ? Custom manual role check
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || (role != "Podcaster" && role != "Admin"))
            {
                TempData["Error"] = "You must be logged in as a Podcaster or Admin to create episodes.";
                return RedirectToAction("Login", "Users");
            }

            ViewBag.PodcastID = new SelectList(_context.Podcasts, "PodcastID", "Title");
            return View();
        }

        // -------------------------------------------------------------
        // POST: Episodes/Create
        // -------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PodcastID,Title,Description,ReleaseDate,Duration")] Episode episode,
            IFormFile? media,
            [FromServices] S3Service s3)
        {
            // ? Role check again
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || (role != "Podcaster" && role != "Admin"))
            {
                TempData["Error"] = "Unauthorized: only podcasters can add episodes.";
                return RedirectToAction("Login", "Users");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PodcastID = new SelectList(_context.Podcasts, "PodcastID", "Title", episode.PodcastID);
                return View(episode);
            }

            // ? Handle S3 upload
            if (media != null && media.Length > 0)
            {
                using var stream = media.OpenReadStream();
                var key = $"episodes/{episode.PodcastID}/{Guid.NewGuid()}_{media.FileName}";
                episode.AudioFileUrl = await s3.UploadAsync(stream, key);
            }

            episode.PlayCount = 0;
            episode.NumberOfViews = 0;
            if (episode.ReleaseDate == default)
                episode.ReleaseDate = DateTime.UtcNow;

            _context.Add(episode);
            await _context.SaveChangesAsync();

            TempData["Message"] = "?? Episode created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------------
        // GET: Episodes/Edit/{id}
        // -------------------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || (role != "Podcaster" && role != "Admin"))
            {
                TempData["Error"] = "Unauthorized: only podcasters can edit episodes.";
                return RedirectToAction("Login", "Users");
            }

            if (id == null) return NotFound();
            var ep = await _context.Episodes.FindAsync(id);
            if (ep == null) return NotFound();

            ViewBag.PodcastID = new SelectList(_context.Podcasts, "PodcastID", "Title", ep.PodcastID);
            return View(ep);
        }

        // -------------------------------------------------------------
        // POST: Episodes/Edit/{id}
        // -------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("EpisodeID,PodcastID,Title,Description,ReleaseDate,Duration,PlayCount,NumberOfViews,AudioFileUrl")] Episode episode,
            IFormFile? media,
            [FromServices] S3Service s3)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || (role != "Podcaster" && role != "Admin"))
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Users");
            }

            if (id != episode.EpisodeID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.PodcastID = new SelectList(_context.Podcasts, "PodcastID", "Title", episode.PodcastID);
                return View(episode);
            }

            if (media != null && media.Length > 0)
            {
                using var stream = media.OpenReadStream();
                var key = $"episodes/{episode.PodcastID}/{Guid.NewGuid()}_{media.FileName}";
                episode.AudioFileUrl = await s3.UploadAsync(stream, key);
            }

            _context.Update(episode);
            await _context.SaveChangesAsync();

            TempData["Message"] = "? Episode updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------------
        // GET: Episodes/Delete/{id}
        // -------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || (role != "Podcaster" && role != "Admin"))
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Users");
            }

            if (id == null) return NotFound();
            var ep = await _context.Episodes.Include(e => e.Podcast).FirstOrDefaultAsync(m => m.EpisodeID == id);
            if (ep == null) return NotFound();

            return View(ep);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ep = await _context.Episodes.FindAsync(id);
            if (ep != null)
                _context.Episodes.Remove(ep);

            await _context.SaveChangesAsync();
            TempData["Message"] = "?? Episode deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool EpisodeExists(int id)
        {
            return _context.Episodes.Any(e => e.EpisodeID == id);
        }
    }
}
