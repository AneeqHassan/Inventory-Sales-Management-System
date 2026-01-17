using Inventory_SalesManagement.Models.Entities;

namespace Inventory_SalesManagement.Models
{
    public static class DbSeeder
    {
        // Notice we are passing 'DatabaseContext' here, not 'ApplicationDbContext'
        public static void SeedAdmin(DatabaseContext db)
        {
            // Check if any users exist. If not, create the Admin.
            if (!db.Users.Any())
            {
                var admin = new User
                {
                    Username = "admin",
                    // We hash the password "admin123" immediately
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                };

                db.Users.Add(admin);
                db.SaveChanges();
            }
        }
    }
}
