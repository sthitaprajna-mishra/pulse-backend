using CloseConnectv1.Data;
using CloseConnectv1.Models;
using CloseConnectv1.Repository.IRepository;

namespace CloseConnectv1.Repository
{
    public class DraftRepository : Repository<Draft>, IDraftRepository
    {
        private readonly ApplicationDbContext _db;

        public DraftRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
