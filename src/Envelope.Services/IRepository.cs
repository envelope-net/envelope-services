﻿using Envelope.Model;

namespace Envelope.Services;

public interface IRepository<TEntity>
	where TEntity : IEntity
{
	//void Add(TEntity entity);

	//ValueTask AddAsync(TEntity entity, CancellationToken cancellationToken = default);

	//void Remove(TEntity entity);

	//Task<int> SaveAsync(
	//	CancellationToken cancellationToken = default,
	//	[CallerMemberName] string memberName = "",
	//	[CallerFilePath] string sourceFilePath = "",
	//	[CallerLineNumber] int sourceLineNumber = 0);

	//Task<int> SaveAsync(
	//	SaveOptions? options,
	//	CancellationToken cancellationToken = default,
	//	[CallerMemberName] string memberName = "",
	//	[CallerFilePath] string sourceFilePath = "",
	//	[CallerLineNumber] int sourceLineNumber = 0);
}
