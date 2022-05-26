using Envelope.Model;

namespace Envelope.Services;

public interface IQueryRepository<TEntity>
	where TEntity : IQueryEntity
{
	IQueryable<TEntity> AsQueryable();
}
