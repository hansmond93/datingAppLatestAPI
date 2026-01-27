using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<Member> Members { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<MemberLike> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    new IdentityRole { Id = "member-id", Name = "Member", NormalizedName = "MEMBER" },
                    new IdentityRole { Id = "moderator-id", Name = "Moderator", NormalizedName = "MODERATOR" },
                    new IdentityRole { Id = "admin-id", Name = "Admin", NormalizedName = "ADMIN" }
                );

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

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

            var nullableDateTomeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
            );

            foreach ( var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach(var prop in entityType.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTime))
                    {
                        prop.SetValueConverter(dateTomeConverter);
                    }
                    else if (prop.ClrType == typeof(DateTime?))
                    {
                        prop.SetValueConverter(nullableDateTomeConverter);
                    }
                }
            }
        }
    }
}
