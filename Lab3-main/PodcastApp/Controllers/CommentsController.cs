using Microsoft.AspNetCore.Mvc;
using PodcastApp.Services;
using Microsoft.AspNetCore.Http;

namespace PodcastApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly DynamoCommentsService _comments;

        public CommentsController(DynamoCommentsService comments)
        {
            _comments = comments;
        }

        // -------------------------------------------------------------
        // ADD COMMENT
        // -------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string episodeId, string podcastId, string text)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "?? Please log in to post a comment.";
                return RedirectToAction("Login", "Users");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", "Episodes", new { id = episodeId });
            }

            await _comments.AddAsync(episodeId, podcastId, userId, text);
            TempData["Message"] = $"? Comment posted successfully as {username}!";
            return RedirectToAction("Details", "Episodes", new { id = episodeId });
        }

        // -------------------------------------------------------------
        // EDIT COMMENT
        // -------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string episodeId, string commentId, string newText)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "?? You must be logged in to edit comments.";
                return RedirectToAction("Login", "Users");
            }

            var success = await _comments.EditAsync(episodeId, commentId, userId, newText);

            if (!success)
                TempData["Error"] = "? You can only edit your own comments within 24 hours.";
            else
                TempData["Message"] = "? Comment updated successfully!";

            return RedirectToAction("Details", "Episodes", new { id = episodeId });
        }

        // -------------------------------------------------------------
        // LIST COMMENTS (for Admin or Debug)
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> List(string episodeId)
        {
            var items = await _comments.ListForEpisodeAsync(episodeId);
            return View(items);
        }
    }
}
