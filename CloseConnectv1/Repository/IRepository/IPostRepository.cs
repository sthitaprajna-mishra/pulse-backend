using CloseConnectv1.Models;
using System.Linq.Expressions;

namespace CloseConnectv1.Repository.IRepository
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<List<Post>> GetRecentAsync(int? pageNumber = null, int? pageSize = null);
        Task<List<Post>> GetPopularAsync(int? pageNumber = null, int? pageSize = null);
        Task<int> GetMaxLikes();
        Task<int> GetMaxComments();
    }
}
