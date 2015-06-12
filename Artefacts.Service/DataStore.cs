using System;
using ServiceStack.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artefacts;
using ObjectId = MongoDB.Bson.ObjectId;

namespace Artefacts
{
	/// <summary>
	/// Artefacts repository proxy.
	/// </summary>
	public abstract class DataStore : IDataStore//, IEnumerable<Artefact>//, IDictionary<string, Artefact>
	{
		#region Static members;
		/// <summary>
		/// Gets the log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(DataStore)) { SourceClass = typeof(DataStore) };
		
		/// <summary>
		/// Gets the <see cref="DataStore"/> context for the executing assembly
		/// </summary>
		[ThreadStatic]
		public static IDataStore Context;// { get; private set; }
		
		/// <summary>
		/// Gets the name of the collection.
		/// </summary>
		/// <returns>The collection name.</returns>
		/// <param name="type">Type.</param>
		public static string GetCollectionName(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			return type == typeof(Artefact) ? "Artefacts" :
				SanitiseCollectionName(type.FullName);
		}
		
		/// <summary>
		/// Sanitises the name of the collection.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns>The collection name.</returns>
		public static string SanitiseCollectionName(string name)
		{
			return name.TrimStart("Artefacts.").Replace(".", "_");
		}
		#endregion

		#region Fields and properties
		/// <summary>
		/// Gets the <see cref="Artefact"/>s
		/// </summary>
		public IRepository<Artefact> Artefacts {
			get; protected set;
		}
		
		/// <summary>
		/// Gets or sets the <see cref="Aspect"/>s
		/// </summary>
//		public IRepository<Aspect> Aspects {
//			get; protected set;
//		}
		
		/// <summary>
		/// The query cache.
		/// </summary>
		public Dictionary<Type, IRepository> RepositoryCache {
			get; protected set;
   		}
		
		/// <summary>
		/// Gets or sets the visitor.
		/// </summary>
		public ExpressionVisitor Visitor { get; protected set; }
		#endregion

		#region Methods
		#region Construction
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.ArtefactsRepositoryProxy"/> class.
		/// </summary>
		/// <param name="uri">URI.</param>
		/// <remarks>
		///
		/// </remarks>
		protected DataStore()
		{
			Context = this;
			RepositoryCache = new Dictionary<Type, IRepository>(8);
		}
		#endregion
		
		#region IDataStore implementation
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="collectionName">Collection name.</param>
		/// <returns>The repository.</returns>
		protected abstract IRepository CreateRepository(Type type, string collectionName);
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="collectionName">Collection name.</param>
		/// <remarks>TODO: Make this abstract and GR(string,string) virtual calling this (makes more sense I think)</remarks>
		/// <returns>The repository.</returns>
		public IRepository GetRepository(Type type, string collectionName)
		{
			IRepository repository;
			if (RepositoryCache.ContainsKey(type))
			{
				repository = RepositoryCache[type];
//				Log.Debug(repository + " returned from cache");
			}
			else
			{
				repository = CreateRepository(type, collectionName);
//				Log.Debug(repository + " added to cache");
				RepositoryCache[type] = repository;
			}
			return repository;
		}
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>The repository.</returns>
		public IRepository GetRepository(Type type)
		{
			return GetRepository(type, GetCollectionName(type));
		}
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>The repository.</returns>
		public IRepository<T> GetRepository<T>(string collectionName) where T : class
		{
			return (IRepository<T>)GetRepository(typeof(T), collectionName);
		}
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>The repository.</returns>
		public IRepository<T> GetRepository<T>() where T : class
		{
			return (IRepository<T>)GetRepository(typeof(T), GetCollectionName(typeof(T)));
		}
		#endregion

		#region IQueryProvider implementation
		/// <summary>
		/// Gets the query.
		/// </summary>
		/// <param name="query">Query.</param>
		/// <returns>The query.</returns>
		public IQueryable CreateQuery(Expression query)
		{
			Type elementType;
			if (!query.TryGetElementType(out elementType))
				throw new ArgumentOutOfRangeException("query", query, "");
			Log.DebugVariable("query", query);
			Expression queryVisited = Visitor == null ? query : Visitor.Visit(query);
			if (queryVisited != query)
				Log.DebugVariable("queryVisited", queryVisited);
			return GetRepository(elementType).CreateQuery(queryVisited);
		}
		
		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		public object Execute(Expression expression)
		{
//			Type elementType;
//			if (!expression.TryGetElementType(out elementType))
//				throw new ArgumentOutOfRangeException("query", expression, "");
			Log.DebugVariable("query", expression);
			Expression queryVisited = Visitor == null ? expression : Visitor.Visit(expression);
			if (queryVisited != expression)
				Log.DebugVariable("queryVisited", queryVisited);
			return GetRepository(expression.GetRootElementType()).Execute(queryVisited);
		}
		#endregion

		/// <summary>
		/// Dos the data relations.
		/// </summary>
		/// <param name="instance">Instance.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <remarks>DataStore level?? (Trying for new IDataRelation thing)DataStore level?? (Trying for new IDataRelation thing)</remarks>
		public void DoDataRelations<T>(T instance)
		{
			BindingFlags methodBF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;
			IEnumerable<Type> dataRelations = typeof(T).GetInterfaces().Where(ifType => ifType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IDataRelation<,>)));
			foreach (Type ifType in dataRelations)
				ifType.InvokeMember(ifType.GetMethods()[0].Name, methodBF, null, instance, new object[] { this });
		}

//		#region IArtefactsRepository implementation
//		/// <summary>
//		/// Get the specified predicate.
//		/// </summary>
//		/// <param name="predicate">Predicate.</param>
//		/// <typeparam name="TAspect">The 1st type parameter.</typeparam>
//		public IEnumerable<TAspect> Get<TAspect>(Expression<Func<TAspect, bool>> predicate)
//		{
//			predicate = (Expression<Func<TAspect, bool>>)Visitor.Visit(predicate);
//			Log.DebugVariable("predicate", predicate);
//
//			Expression<Func<Aspect, bool>> aspectFilter = (aspect) => aspect.TypeName == typeof(TAspect).FullName;
//			Expression<Func<Aspect, TAspect>> aspectSelector = (aspect) => (TAspect)aspect.Instance;
//			object aspects = CreateQuery<TAspect>(
//				Expression.Call(((MethodInfo)(typeof(System.Linq.Queryable).GetMember("Where")[0])).MakeGenericMethod(typeof(TAspect)),
//					Expression.Call(((MethodInfo)(typeof(System.Linq.Queryable).GetMember("Select")[0])).MakeGenericMethod(typeof(Aspect), typeof(TAspect)),
//						Expression.Call(((MethodInfo)(typeof(System.Linq.Queryable).GetMember("Where")[0])).MakeGenericMethod(typeof(Aspect)),
//							Expression.Parameter(typeof(System.Linq.IQueryable<Aspect>), GetCollectionName(typeof(TAspect))),
//							aspectFilter), aspectSelector), predicate));
//			Log.DebugVariable("(return)", aspects);
//			return (IEnumerable<TAspect>)aspects; 
//		}
//		#endregion
		#endregion
	}
}

