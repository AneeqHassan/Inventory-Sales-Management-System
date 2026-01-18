using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Inventory_SalesManagement.Models;
using Inventory_SalesManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Inventory_SalesManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly DatabaseContext _db;

        public AuthController(DatabaseContext db)
        {
            _db = db;
        }

        // --- YOUR EXISTING LOGIN LOGIC ---
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User userLoginAttempt)
        {
            // NOTE: I changed the parameter name to 'userLoginAttempt' to avoid confusion
            // because the model might have validation requirements we don't want to check here (like Role)

            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.Username == userLoginAttempt.Username);

            if (userInDb != null)
            {
                // Verify Password using BCrypt
                if (BCrypt.Net.BCrypt.Verify(userLoginAttempt.HashedPassword, userInDb.HashedPassword))
                {
                    var token = GenerateToken(userInDb);
                    Response.Cookies.Append("jwt_token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Set to false if testing without HTTPS
                        SameSite = SameSiteMode.Strict
                    });
                    TempData["Success"] = $"Welcome back, {userInDb.Username}!";
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.ErrorMessage = "Invalid Username or Password";
            TempData["Error"] = "Invalid username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            TempData["Info"] = "You have been logged out.";
            return RedirectToAction("Login");
        }

        // --- NEW: ONLY ADMIN CAN ADD USERS ---
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(string username, string password, string role)
        {
            // 1. Check if user exists
            if (_db.Users.Any(u => u.Username == username))
            {
                ViewBag.Message = "User already exists!";
                return View();
            }

            // 2. Hash the password manually
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // 3. Create User
            var newUser = new User
            {
                Username = username,
                HashedPassword = passwordHash,
                Role = role // "Admin" or "Sales Staff"
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            ViewBag.Message = "User created successfully!";
            return View();
        }

        // --- YOUR EXISTING TOKEN LOGIC ---
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role ?? "Public")
        };
            // Ensure this key matches Program.cs exactly!
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF32.GetBytes("class-work-5E"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                    issuer: "https://localhost:7084/",
                    audience: "https://localhost:7084/",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60), // Increased to 60 mins
                    signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
