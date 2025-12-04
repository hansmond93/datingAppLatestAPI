using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<MemberLike> Likes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MemberLike>()
                .HasKey(x => new {x.SourceMemberId, x.TargetMemberId}); //set the Primary key to this values

            modelBuilder.Entity<MemberLike>()
                .HasOne(x => x.SourceMember)
                .WithMany(t => t.LikedMembers)
                .HasForeignKey(x => x.SourceMemberId)
                .OnDelete(DeleteBehavior.Cascade);  //Delete the foreign key means deleting the SourceMember

            modelBuilder.Entity<MemberLike>()
                .HasOne(x => x.TargetMember)
                .WithMany(t => t.LikedByMembers)
                .HasForeignKey(x => x.TargetMemberId)
                .OnDelete(DeleteBehavior.NoAction);  

            var dateTomeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            foreach( var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach(var prop in entityType.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTime))
                    {
                        prop.SetValueConverter(dateTomeConverter);
                    }
                }
            }
        }
    }
}
