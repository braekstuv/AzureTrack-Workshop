using Microsoft.EntityFrameworkCore;
using RMotownFestival.Api.Domain;
using System;

namespace RMotownFestival.DAL
{
    public class MotownDbContext : DbContext
    {
        public MotownDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Artist> Artists { get; set; }
        public object Stages { get; set; }
    }
}
