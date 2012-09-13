using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace K2Calendar.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserInfoModel> Users { get; set; }
        public DbSet<RankModel> Ranks { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<TagModel> Tags { get; set; }
        public DbSet<CommentModel> Comments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostModel>()
            .HasMany(p => p.Tags).WithMany(c => c.Posts)
            .Map(t => t.MapLeftKey("PostId")
            .MapRightKey("TagId")
            .ToTable("PostTags"));

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}