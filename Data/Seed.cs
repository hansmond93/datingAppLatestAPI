using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);


            if (members == null) 
            {
                Console.WriteLine("No members to seed");
                return;
            }

            foreach (var member in members)
            {
                var user = new AppUser
                {
                    DisplayName = member.DisplayName,
                    Email = member.Email,
                    UserName = member.Email,
                    Id = member.Id,
                    ImageUrl = member.ImageUrl,
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

                var result = await userManager.CreateAsync(user, "Pa$$w0rd");

                if (!result.Succeeded)
                {
                    Console.WriteLine(result.Errors.First().Description);
                }

                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                DisplayName = "admin",
                Email = "admin@test.com",
                UserName = "admin@test.com"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);

        }
    }
}
