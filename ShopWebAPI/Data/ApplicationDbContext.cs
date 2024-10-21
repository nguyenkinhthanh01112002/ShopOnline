using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopWebAPI.Models;
using System.Data;

namespace ShopWebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
