using CloseConnectv1.Data;
using CloseConnectv1.Models;
using CloseConnectv1.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace CloseConnectv1.Repository
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        private readonly ApplicationDbContext _db;

        public PostRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<int> GetMaxComments()
        {
            return await _db.Posts.MaxAsync(post => post.NumberOfComments);
        }

        public async Task<int> GetMaxLikes()
        {
            return await _db.Posts.MaxAsync(post => post.NumberOfLikes);
        }

        public async Task<List<Post>> GetPopularAsync(int? pageNumber = null, int? pageSize = null)
        {
            List<Post> posts = new();

            if (pageNumber is not null && pageSize is not null)
            {
                posts = await _db.Posts.Where(post => post.PopularityScore > 0).OrderByDescending(post => post.PopularityScore)
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();
            }
            else
            {
                posts = await _db.Posts.OrderByDescending(post => post.PopularityScore).ToListAsync();
            }

            return posts;
        }

        public async Task<List<Post>> GetRecentAsync(int? pageNumber = null, int? pageSize = null)
        {
            List<Post> posts = new();

            if (pageNumber is not null && pageSize is not null)
            {
                posts = await _db.Posts.OrderByDescending(post => post.CreateDate)
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();
            }
            else
            {
                posts = await _db.Posts.OrderByDescending(post => post.CreateDate).ToListAsync();
            }

            return posts;
        }
    }
}
