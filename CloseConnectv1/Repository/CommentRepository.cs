using CloseConnectv1.Data;
using CloseConnectv1.Models;
using CloseConnectv1.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace CloseConnectv1.Repository
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> HasChildCommentsAsync(int commentId)
        {
            var result = await _db.Comments.FirstOrDefaultAsync(comment => comment.ParentCommentId == commentId);
            return result is not null;
        }

        public async Task<List<Comment>> GetAllChildCommentsAsync(int commentId)
        {
            List<Comment> childComments = await _db.Comments.Where(comment => comment.ParentCommentId == commentId).ToListAsync();
            return childComments;
        }

        public async Task<bool> IsCommentLikedAlready(int commentId, string userId)
        {
            var record = await _db.CommentLikes.FirstOrDefaultAsync(comment => comment.CommentId == commentId && comment.UserId == userId);
            return record is not null;
        }
    }
}
