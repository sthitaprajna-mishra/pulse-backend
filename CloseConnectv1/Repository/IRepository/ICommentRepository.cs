using CloseConnectv1.Models;

namespace CloseConnectv1.Repository.IRepository
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<bool> IsCommentLikedAlready(int commentId, string userId);
        Task<bool> HasChildCommentsAsync(int commentId);
        Task<List<Comment>> GetAllChildCommentsAsync(int commentId);
    }
}
