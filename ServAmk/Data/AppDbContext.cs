using Microsoft.EntityFrameworkCore;
using ServAmk.Models;
using System.Data;

namespace ServAmk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<News> News => Set<News>();
        public DbSet<NewsMedia> NewsMedia => Set<NewsMedia>();
    }
}
