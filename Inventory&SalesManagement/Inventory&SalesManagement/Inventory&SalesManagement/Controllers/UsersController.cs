using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory_SalesManagement.Models.Entities;
using Inventory_SalesManagement.Models; // For ViewModels
using Microsoft.EntityFrameworkCore;

// Ensure only Admins can access this controller
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly DatabaseContext _context;

    public UsersController(DatabaseContext context)
    {
        _context = context;
    }

    // 1. LIST ALL USERS
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    // 2. CREATE USER (GET)
    public IActionResult Create()
    {
        return View();
    }

    // 2. CREATE USER (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        // We manually check validity because we might want to allow empty IDs (auto-generated)
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.HashedPassword))
        {
            ModelState.AddModelError("", "Username and Password are required.");
        }

        if (ModelState.IsValid)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(user);
            }

            // NOTE: In a real production app, you should HASH the password here.
            // For now, we save it as provided based on your model.
            _context.Add(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"User '{user.Username}' created successfully!";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Failed to create user.";
        return View(user);
    }

    // 3. EDIT USER (GET)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // SECURITY: Clear the password hash before sending to View.
        // We don't want the encrypted text showing in the textbox.
        user.HashedPassword = "";

        return View(user); // Pass the Entity directly
    }

    // 3. EDIT USER (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id) return NotFound();

        // TRICK: We remove "HashedPassword" from validation.
        // Why? Because the Entity expects a password, but we want to allow 
        // a blank field (which means "Keep the old password").
        ModelState.Remove("HashedPassword");

        if (ModelState.IsValid)
        {
            try
            {
                // 1. Fetch the ORIGINAL user from the database
                var userInDb = await _context.Users.FindAsync(id);
                if (userInDb == null) return NotFound();

                // 2. Update the fields we want to change
                userInDb.Username = user.Username;
                userInDb.Role = user.Role;

                // 3. Handle Password:
                // Only update it if the user actually typed something new.
                if (!string.IsNullOrEmpty(user.HashedPassword))
                {
                    userInDb.HashedPassword = user.HashedPassword;
                }
                // If user.HashedPassword is null/empty, we touch nothing (keeping the old one).

                _context.Update(userInDb);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            TempData["Success"] = "User details updated!";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Failed to update user.";
        return View(user);
    }

    // 4. DELETE USER
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}