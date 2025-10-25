using Microsoft.AspNetCore.Mvc;
using PodcastApp.Data;
using PodcastApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace PodcastApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Users/Register
        // ============================================================
        public IActionResult Register()
        {
            return View();
        }

        // ============================================================
        // POST: /Users/Register
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.UserID = Guid.NewGuid();

                // ✅ Ensure valid role (default = Listener)
                if (!Enum.IsDefined(typeof(UserRole), user.Role))
                    user.Role = UserRole.Listener;

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Message"] = "✅ Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // ============================================================
        // GET: /Users/Login
        // ============================================================
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                TempData["Message"] = $"You're already logged in as {HttpContext.Session.GetString("Username")}!";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ============================================================
        // POST: /Users/Login
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // ✅ Store session info
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserID", user.UserID.ToString());

            TempData["Message"] = $"🎉 Welcome back, {user.Username}!";

            // ✅ FIXED REDIRECT LOGIC
            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                UserRole.Podcaster => RedirectToAction("Index", "Podcasts"), // ✅ FIXED HERE
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ============================================================
        // GET: /Users/Logout
        // ============================================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "👋 You have been logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
