using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace API.Data
{
    public class LikesRepository(AppDbContext context) : ILikesRepository
    {
        public void AddLike(MemberLike like)
        {
            context.Likes.Add(like);
        }

        public void DeleteLike(MemberLike like)
        {
            context.Likes.Remove(like);
        }

        public async Task<IReadOnlyList<string>> GetCurrentMemberLikedIds(string memberId)
        {
            return await context.Likes.Where(m=> m.SourceMemberId == memberId).Select(x => x.TargetMemberId).ToListAsync();
        }

        public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
        {
            return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
        }

        public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
        {
            var query = context.Likes.AsQueryable();
            IQueryable<Member> result;

            switch (likesParams.Predicate)
            {
                case "liked":
                    result = query.Where(x => x.SourceMemberId == likesParams.MemberId).Select(x => x.TargetMember).AsQueryable();
                    break;
                case "likedBy":
                    result = query.Where(x => x.TargetMemberId == likesParams.MemberId).Select(x => x.SourceMember).AsQueryable();
                    break;
                default: //mutual likes
                    var likedIds = await GetCurrentMemberLikedIds(likesParams.MemberId);

                    result = query
                        .Where(x => x.TargetMemberId == likesParams.MemberId && likedIds.Contains(x.SourceMemberId))
                        .Select(x => x.SourceMember)
                        .AsQueryable();
                    break;
            }

            return await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<bool> SaveAllChanges()
        {
            return await context.SaveChangesAsync() > 0;
        }

    }
}
