using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Logging;
using ServiceStack;
using System.Linq.Expressions;
using MongoDB.Bson;
using System.Net;

namespace Artefacts
{
	[DataContract]
	public class QueryableResponse
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(QueryableResponse)) { SourceClass = typeof(QueryableResponse) };
		#endregion

		[DataMember]
		public int QueryId;
		
		[DataMember]
		public ObjectId[] Ids;
		
		public int Count {
			get { return Ids != null ? Ids.Length : 0; }
		}
		
		public QueryableResponse(int queryId, ObjectId[] ids)
		{
			QueryId = queryId;
			Ids = ids.ToArray();
			Log.Debug(this);
		}
	
		
	}
	
	/// <summary>
	/// Queryable response.
	/// </summary>
	/// <remarks>
	/// Try to just get this working reliably and fully functionally before worrying about optimisation and performance,
	/// but when you get that far, maybe -
	/// 	- Server returns a query Id (same as sent from client or separate) used to identify query for future requests
	/// 		- Happens immediately as server doesn't need to perform any queries to do this
	/// 	- Immediately after returning the query id (or delayed using some other algorithm) the server performs
	/// 	  the query on the stored data. Results returned are stored in a "cache" (thinking my own simple array "cache",
	/// 	  but potentially could be a SS thing) on the server
	/// 	- Whenb client requests query results (needing only to send the query Id, and page number/count if necessary)
	/// 	  server can (probably) return results that are already retrieved from the storage medium
	/// </remarks>
	[DataContract]
	public class QueryableResponse<T> : QueryableResponse, IRepository<T> where T : class
	{
		public QueryableResponse(Expression expression, int queryId, ObjectId[] ids)
			: base(queryId, ids)
		{
			Expression = expression;
		}

		protected T[] Items {
			get { return _items ?? (_items = DataStore.GetRepository<T>().GetAll(Ids).ToArray()); }
		}
		private T[] _items;
		
//		public T Get(int index, RepositoryProxy<T> repositoryProxy = null)
//		{
//			if (_items == null)
//				_items = new T[Ids.Length];
//			return _items[index] ?? _(items[index] =
//				(repositoryProxy != null ?
//					repositoryProxy.Get(Ids[index]) :
//					Provider.Get(Ids[index])));
//		}
		
//		#region IEnumerable implementations
//		public IEnumerator<T> GetEnumerator()
//		{
//			List<T> list = new List<T>(Ids.Length);
//			for (int i = 0; i < Ids.Length; i++)
//				list.Add(Get(i));
//			return list.GetEnumerator();
//		}
//		#endregion


		#region IRepository implementation
		public T Get(ObjectId id)
		{
			return Items.Single(item => ((ObjectId)item.GetId() == id));
		}
		public void /*HttpWebResponse*/ Create(BsonDocument obj)
		{
			throw new NotImplementedException();
		}
		public void /*HttpWebResponse*/ Create(T obj)
		{
			throw new NotImplementedException();
		}
		public void Update(T obj)
		{
			throw new NotImplementedException();
		}
		public void Save(T obj)
		{
			throw new NotImplementedException();
		}
		public void Delete(T obj)
		{
			throw new NotImplementedException();
		}
		public IEnumerable<T> GetAll(IEnumerable<ObjectId> objIds)
		{
			return Items;
		}
		public void CreateAll(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}
		public void UpdateAll(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}
		public void SaveAll(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}
		public void DeleteAll(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}
		#endregion
		#region IEnumerable implementation
		public IEnumerator<T> GetEnumerator()
		{
			return Items.AsQueryable<T>().GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Items.GetEnumerator();
		}
		#endregion
		#region IRepository implementation
		public IQueryable GetQuery(Expression expression)
		{
			return Items.AsQueryable().Provider.CreateQuery(expression);
		}
		public IQueryable CreateQuery(Expression expression)
		{
			return GetQuery(expression);
		}
		public IQueryable<T> CreateQuery<T>(Expression expression)
		{
			return (IQueryable<T>)CreateQuery(expression);
		}
		public object Evaluate(Expression expression)
		{
			return Items.AsQueryable().Provider.Execute(expression);
		}
		public object Execute(Expression expression)
		{
			return Evaluate(expression);
		}
		public TResult Execute<TResult>(Expression expression)
		{
			return (TResult)Execute(expression);
		}
		public bool Exists(ObjectId id)
		{
			return Items.AsQueryable().Any(a => ((ObjectId)a.GetId() == id));
		}
		public Type ElementType {
			get {
				return typeof(T);
			}
		}
		public IQueryProvider Provider {
			get {
				return Items.AsQueryable().Provider;
			}
		}
		public Expression Expression {
			get; protected set;
		}
		public IDataStore DataStore {
			get; protected set;
		}
		public IReadOnlyDictionary<int, object> QueryCache {
			get; protected set;
		}
		public IReadOnlyDictionary<int, object> ResultCache {
			get; protected set;
		}
		#endregion
	}
}

