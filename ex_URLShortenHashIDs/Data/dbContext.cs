using ex_URLShortenHashIDs.Entities;
using Microsoft.EntityFrameworkCore;

namespace ex_URLShortenHashIDs.Data
{
    public class dbContext : DbContext
    {
        public virtual DbSet<ShortenedURL> ShortenedURLs { get; set; }
        public dbContext(DbContextOptions<dbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ShortenedURL>().Property(x => x.Id).HasColumnType("bigint");//.HasDefaultValueSql("NEWSEQUENTIALID()");

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
