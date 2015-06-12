using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ObjectId = MongoDB.Bson.ObjectId;
using Artefacts;

namespace Artefacts
{
	/// <summary>
	/// Data storage for <see cref="Artefact"/> and <see cref="Aspect"/> objects
	/// </summary>
	public interface IDataStore
	{	
		/// <summary>
		/// <see cref="Artefact"/>s <see cref="IRepository`1"/>
		/// </summary>
		IRepository<Artefact> Artefacts { get; }
		
		/// <summary>
		/// <see cref="Aspect"/>s <see cref="IRepository`1"/>
		/// </summary>
//		IRepository<Aspect> Aspects { get; }
		
		/// <summary>
		/// Query cache.
		/// </summary>
		Dictionary<Type, IRepository> RepositoryCache { get; }
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="collectionName">Collection name.</param>
		/// <returns>The repository.</returns>
		IRepository GetRepository(Type type, string collectionName);
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>The repository.</returns>
		IRepository GetRepository(Type type);
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>The repository.</returns>
		IRepository<T> GetRepository<T>(string collectionName) where T : class;
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>The repository.</returns>
		IRepository<T> GetRepository<T>() where T : class;

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		object Execute(Expression expression);

		void DoDataRelations<T>(T instance);
	}
}

