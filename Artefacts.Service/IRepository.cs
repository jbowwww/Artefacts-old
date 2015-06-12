using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using System.Linq.Expressions;
using System.Net;

namespace Artefacts
{
	/// <summary>
	/// Base repository interface. Inherited by <see cref="IRepository`1"/> 
	/// </summary>
	public interface IRepository : IQueryProvider, IQueryable
	{
		#region Fields & properties
		/// <summary>
		/// Gets or sets the data store.
		/// </summary>
		IDataStore DataStore { get; }

		/// <summary>
		/// Gets the query cache.
		/// </summary>
		IReadOnlyDictionary<int, object> QueryCache { get; }

		/// <summary>
		/// Gets the result cache.
		/// </summary>
		IReadOnlyDictionary<int, object> ResultCache { get; }

		#region IQueryable implementation
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		Type ElementType { get; }
		
		/// <summary>
		/// Gets the provider.
		/// </summary>
		IQueryProvider Provider { get; }
		
		/// <summary>
		/// Gets the expression.
		/// </summary>
		Expression Expression { get; }
		#endregion
		#endregion

		#region Methods
		#region IQueryProvider implementation
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		IQueryable CreateQuery(Expression expression);

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		IQueryable<T> CreateQuery<T>(Expression expression);
		
		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		/// <remarks>I think this one is only called for enumerable queries</remarks>
		object Execute(Expression expression);
		
		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		/// <returns>To be added.</returns>
		/// <remarks>I think this one is (only?) called for queries with scalar results</remarks>
		TResult Execute<TResult>(Expression expression);
		#endregion
		
		#region Artefacts.Queryable operations (called by Artefacts.Queryable's IQueryProvider-implementing code)
		/// <summary>
		/// Gets or creates an <see cref="IQueryable"/> instance for the <paramref name="expression"/> 
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>A <see cref="IQueryable"/> instance</returns>
		IQueryable GetQuery(Expression expression);
		
		/// <summary>
		/// Evaluate the specified <paramref name="expression"/>
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>The evaluated result</returns>
		object Evaluate(Expression expression);
		#endregion
		
		#region CRUD operations (only bool Exists(string id) defined in this base class), others in Generic version below
		/// <summary>
		/// Check if object with given <paramref name="id"/> exists in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="id">Object Identifier.</param>
		bool Exists(ObjectId id);

		/// <summary>
		/// Create the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		void /*HttpWebResponse*/ Create(BsonDocument obj);

		/// <summary>
		/// Get object with the specified <paramref name="id"/>
		/// </summary>
		/// <param name="id">Identifier of the obejct to get</param>
		///		T Get<T>(string id);
		///	
		/// <summary>
		/// Get object with the specified <paramref name="id"/>
		/// </summary>
		/// <param name="id">Identifier of the obejct to get</param>
		///		object Get(string id);
		///	
		/// <summary>
		/// Create the specified object in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object to create</param>
		///		void Create<T>(T obj);
		///	
		/// <summary>
		/// Create the specified object in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object to create</param>
		///		void Create(object obj);
		///	
		/// <summary>
		/// Update the specified object in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object.</param>
		///		void Update(object obj);
		///	
		/// <summary>
		/// Save the specified object to this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object.</param>
		///		void Save(object obj);
		///	
		/// <summary>
		/// Delete object specified by <paramref name="id"/> from this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="id">Object identifier.</param>
		///		void Delete(string id);
		///	
		/// <summary>
		/// Delete the specified object from this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object to delete</param>
		///		void Delete(object obj);
		#endregion
		#endregion
	}
	
	/// <summary>
	/// Generic repository. Inherits from <see cref="IRepository"/> 
	/// </summary>
	/// <remarks>
	/// TODO: Find out about Funq, starting with <code>Funq.Func<obj, bool, int, int, int, bool> predicate  );</code>
	/// </remarks>
	public interface IRepository<T> :
		IRepository, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>
		where T : class
	{
		#region Basic CRUD operations
		/// <summary>
		/// Get object with the specified <paramref name="id"/>
		/// </summary>
		/// <param name="id">Identifier of the obejct to get</param>
		T Get(ObjectId id);
		
		/// <summary>
		/// Create the specified object in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object to create</param>
		void /*HttpWebResponse*/ Create(T obj);

		/// <summary>
		/// Update the specified object in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object.</param>
		void Update(T obj);

		/// <summary>
		/// Save the specified object to this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object.</param>
		void Save(T obj);

		/// <summary>
		/// Delete the specified object from this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="obj">Object to delete</param>
		void Delete(T obj);
		#endregion
		
		#region "All" operations
		/// <summary>
		/// Gets all the specified objects in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="objs">Objects.</param>
		IEnumerable<T> GetAll(IEnumerable<ObjectId> objIds);
		
		/// <summary>
		/// Creates all the specified objects in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="objs">Objects.</param>
		void CreateAll(IEnumerable<T> objs);

		/// <summary>
		/// Updates all the specified objects in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="objs">Objects.</param>
		void UpdateAll(IEnumerable<T> objs);

		/// <summary>
		/// Updates all the specified objects in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="objs">Objects.</param>
		void SaveAll(IEnumerable<T> objs);

		/// <summary>
		/// Deletes all the specified objects in this <see cref="IRepository`1"/>
		/// </summary>
		/// <param name="objs">Objects.</param>
		void DeleteAll(IEnumerable<T> objs);
		#endregion
	}
}

