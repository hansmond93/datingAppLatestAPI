using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);


            if (members == null) 
            {
                Console.WriteLine("No members to seed");
                return;
            }

            using var hmac = new HMACSHA512();

            foreach (var member in members)
            {
                var user = new AppUser
                {
                    DisplayName = member.DisplayName,
                    Email = member.Email,
                    Id = member.Id,
                    ImageUrl = member.ImageUrl,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
                    PasswordSalt = hmac.Key,
                    Member = new Member
                    {
                        Id = member.Id,
                        City = member.City,
                        Country = member.Country,
                        Gender = member.Gender,
                        DisplayName = member.DisplayName,
                        Created = member.Created,
                        DateOfBirth = member.DateOfBirth,
                        Description = member.Description,
                        ImageUrl = member.ImageUrl,
                        LastActive = member.LastActive
                    }

                };

                user.Member.Photos.Add(new Photo
                {
                    Url = member.ImageUrl!,
                    MemberId = member.Id,
                });

                context.Users.Add(user);
            }

            await context.SaveChangesAsync();

        }
    }
}
