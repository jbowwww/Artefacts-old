using System;
using System.Collections;
using System.Collections.Generic;
using ServiceStack.Logging;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using System.Net;

namespace Artefacts
{
	/// <summary>
	/// Repository.
	/// </summary>
	public abstract class Repository<T> : IRepository<T> where T : class
	{
		#region Static members;
		/// <summary>
		/// Gets the log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Repository<T>));

		/// <summary>
		/// Tries the get from cache.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <param name="result">Result.</param>
		/// <returns>
		/// <c>true</c>, if <see cref="expression"/> was found in <see cref="QueryCache"/>, <c>false</c> otherwise
		/// </returns>
		private static bool TryGetFromCache(Dictionary<int, object> cache, Expression expression, out object result)
		{
			int hash = expression.GetHashCode();
			if (cache.ContainsKey(hash))
			{
				result = (IQueryable)cache[hash];
				Log.Debug("Return from cache: " + result);
				return true;
			}
			result = null;
			return false;
		}

		/// <summary>
		/// Adds to cache.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <param name="result">Result.</param>
		private static void AddToCache(Dictionary<int, object> cache, Expression expression, object result)
		{
			int hash = expression.GetHashCode();
			if (cache.ContainsKey(hash))
				Log.ThrowError(new InvalidOperationException(string.Format(
					"Hash code already exists in cache (Hash={0},Expression=\"{1}\",Cached=\"{2}\")",
					hash, expression, cache[hash])));
			cache[hash] = result;
			Log.Debug("Add to cache: " + result + " for expression " + expression);
		}
		#endregion

		#region Fields & properties
		/// <summary>
		/// Gets or sets the data store.
		/// </summary>
		public IDataStore DataStore { get; protected set; }
		
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		public Type ElementType { get { return typeof(T); } }
		
		/// <summary>
		/// Gets the provider.
		/// </summary>
		public IQueryProvider Provider { get { return this; } }
		
		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		public Expression Expression { get; protected set; }
		
		/// <summary>
		/// Gets or sets the query cache.
		/// </summary>
		/// <remarks>
		/// Caches query results of any type (recently changed to Dictionary<int,object>)
		/// so it will store (various or one?) implementation of IQueryable instances as well
		/// as primitive type instances (such as int Count). <see cref="Visitor"/> can provide
		/// functionality of traversing expression tree and replacing expressions that have
		/// cached results with that result as an Expression.Constant()
		/// </remarks>
		protected Dictionary<int, object> QueryCache {
			get; set;
		}
		
		/// <summary>
		/// Gets the query cache.
		/// </summary>
		IReadOnlyDictionary<int, object> IRepository.QueryCache {
			get { return (IReadOnlyDictionary<int, object>)QueryCache; }
		}
		
		/// <summary>
		/// Gets or sets the results cache.
		/// </summary>
		/// <remarks>
		/// Caches query results of any type (recently changed to Dictionary<int,object>)
		/// so it will store (various or one?) implementation of IQueryable instances as well
		/// as primitive type instances (such as int Count). <see cref="Visitor"/> can provide
		/// functionality of traversing expression tree and replacing expressions that have
		/// cached results with that result as an Expression.Constant()
		/// </remarks>
		protected Dictionary<int, object> ResultCache {
			get; set;
		}
		
		/// <summary>
		/// Gets the result cache.
		/// </summary>
		IReadOnlyDictionary<int, object> IRepository.ResultCache {
			get { return (IReadOnlyDictionary<int, object>)QueryCache; }
		}
		
		/// <summary>
		/// Gets or sets the visitor.
		/// </summary>
		public ExpressionVisitor Visitor {
			get; protected set;
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Repository`1"/> class.
		/// </summary>
		/// <param name="provider">Provider.</param>
		/// <param name="expression">Expression.</param>
		protected Repository(IDataStore dataStore, Expression expression)
		{
			DataStore = dataStore;
			Expression = expression;
			QueryCache = new Dictionary<int, object>();
			ResultCache = new Dictionary<int, object>();
		}
		
		#region IQueryProvider operation implementations
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		public IQueryable CreateQuery(Expression expression)
		{
			bool foundCached = false;
			object query;
			Log.DebugVariable("expression", expression);
			foundCached = TryGetFromCache(QueryCache, expression, out query);
			if (!foundCached && Visitor != null)
			{
				expression = Visitor.Visit(expression);
				Log.DebugVariable("Visited(e)", expression);
				foundCached = TryGetFromCache(QueryCache, expression, out query);
			}
			if (!foundCached)
				query = GetQuery(expression);
			Log.DebugVariable("query", query);
			AddToCache(QueryCache, expression, query);
			return (IQueryable)query;
		}

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return (IQueryable<TElement>)CreateQuery(expression);
		}

		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		/// <remarks>I think this one is only called for enumerable queries</remarks>
		public object Execute(Expression expression)
		{
			bool foundCached = false;
			object result;
			Log.DebugVariable("expression", expression);
			foundCached = TryGetFromCache(ResultCache, expression, out result);
			if (!foundCached && Visitor != null)
			{
				expression = Visitor.Visit(expression);
				Log.DebugVariable("Visited(e)", expression);
				foundCached = TryGetFromCache(ResultCache, expression, out result);
			}
			if (!foundCached)
				result = Evaluate(expression);
			Log.DebugVariable("foundCached", foundCached);
			Log.DebugVariable("result", result);
			if (!foundCached)
				AddToCache(ResultCache, expression, result);
			return result;
		}

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		/// <returns>To be added.</returns>
		/// <remarks>I think this one is (only?) called for queries with scalar results</remarks>
		public TResult Execute<TResult>(Expression expression)
		{
			return (TResult)Execute(expression);
		}
		#endregion

		#region IRepository implementation (abstract members)
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Repository`1"/> class.
		/// </summary>
		/// <param name="expression">Expression.</param>
		public abstract IQueryable GetQuery(Expression expression);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Repository`1"/> class.
		/// </summary>
		/// <param name="expression">Expression.</param>
		public abstract object Evaluate(Expression expression);
		#endregion
		
		#region IRepository<T> implementation
		public abstract bool Exists(ObjectId id);
		public abstract T Get(ObjectId id);
		public abstract void /*HttpWebResponse*/ Create(BsonDocument obj);
		public abstract void /*HttpWebResponse*/ Create(T obj);
		public abstract void Update(T obj);
		public abstract void Save(T obj);
		public abstract void Delete(ObjectId id);
		public abstract void Delete(T obj);
		#region "All" Operations
		public virtual IEnumerable<T> GetAll(IEnumerable<ObjectId> objIds)
		{
			List<T> list = new List<T>(objIds.Count());
			foreach (ObjectId objId in objIds)
				list.Add(this.Get(objId));
			return list;
		}
		public virtual void CreateAll(IEnumerable<T> objs)
		{
			foreach (T obj in objs)
				this.Create(obj);
		}
		public virtual void UpdateAll(IEnumerable<T> objs)
		{
			foreach (T obj in objs)
				this.Update(obj);
		}
		public virtual void SaveAll(IEnumerable<T> objs)
		{
			foreach (T obj in objs)
				this.Save(obj);
		}
		public virtual void DeleteAll(IEnumerable<ObjectId> objIds)
		{
			foreach (ObjectId id in objIds)
				this.Delete(id);
		}
		public virtual void DeleteAll(IEnumerable<T> objs)
		{
			foreach (T obj in objs)
				this.Delete(obj);
		}	
		#endregion
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return (IEnumerator<T>)GetEnumerator();
		}
		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public virtual IEnumerator GetEnumerator()
		{
			return CreateQuery(Expression.Parameter(typeof(IRepository<T>),
				typeof(T) == typeof(Artefact) ? "Artefacts" : typeof(T).FullName.Replace(".", "_"))).GetEnumerator();
		}
		#endregion
	}
}

