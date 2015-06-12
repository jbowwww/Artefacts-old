using System;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Serialization;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization;
//using MongoDB.Bson.Serialization.Serializers;
//using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Linq;
using ObjectId = MongoDB.Bson.ObjectId;
using Artefacts;
using System.Reflection;
using System.Runtime.Serialization;
using System.CodeDom;
using MongoDB.Driver.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Artefacts
{
	/// <summary>
	/// Artefacts repository proxy.
	/// </summary>
	public sealed class DataStoreProxy : DataStore
	{
		private ServiceClientBase _client;
		
		#region Methods
		#region Construction
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.ArtefactsRepositoryProxy"/> class.
		/// </summary>
		/// <param name="uri">URI.</param>
		/// <remarks>
		///				Expression.Empty();
		///				Expression.Variable(typeof(IArtefactsRepository), "Repository");
		///			Expression.Constant(this, typeof(IArtefactsRepository));
		///				Expression.Variable(typeof(ArtefactsRepositoryMongo), "Repository");
		///Expression.Constant(this, typeof(IArtefactsRepository));
		///				CreateQuery<Artefact>(Expression.Variable(typeof(object), "Artefacts"));			
		///					Expression.Property(this.Expression, typeof(IArtefactsRepository), "Artefacts"));
		/// 
		/// </remarks>
		public DataStoreProxy(string uri)
		{
			_client = new JsvServiceClient(uri)
			{
				RequestFilter = request => { request.UserAgent = "ArtefactAgent"; Log.Debug("Request: " + request.RequestUri + ": " + request.ToString()); },
				AlwaysSendBasicAuthHeader = true
			};
				                                     //GetRequestStream().ReadFully().FromAsciiBytes());//.FormatString());
//			_client.ResponseFilter += (response) => Log.Debug("Response: " + response.ReadToEnd());//.FormatString());
			Artefacts = (IRepository<Artefact>)GetRepository(typeof(Artefact), "Artefacts");
//			Aspects = (IRepository<Aspect>)GetRepository(typeof(Aspect), "Aspects");
			Log.Debug(this.FormatString());
		}
		#endregion
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>The repository.</returns>
		/// <remarks>IDataStore implementation (overrides from abstract base DataStore)</remarks>
		protected override IRepository CreateRepository(Type type, string collectionName)
		{
			return (IRepository)((IRepository)typeof(RepositoryProxy<>).MakeGenericType(type)
				.GetConstructor(new Type[] { typeof(IServiceClient), typeof(IDataStore), typeof(Expression) })
				.Invoke(new object[] { _client, this, Expression.Parameter(
					typeof(Repository<>).MakeGenericType(type), SanitiseCollectionName(collectionName)) }));
		}

		#region IArtefactsRepository implementation
		/// <summary>
		/// Get the specified predicate.
		/// </summary>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="TAspect">The 1st type parameter.</typeparam>
//		public IEnumerable<TAspect> Get<TAspect>(Expression<Func<TAspect, bool>> predicate)
//		{
//			predicate = (Expression<Func<TAspect, bool>>)Visitor.Visit(predicate);
//			Log.DebugVariable("predicate", predicate.ToExpressionNode().ToString());
//			Expression<Func<Aspect, bool>> aspectFilter = (aspect) => aspect.TypeName == typeof(TAspect).FullName;
//			Expression<Func<Aspect, TAspect>> aspectSelector = (aspect) => (TAspect)aspect.Instance;
//			object aspects = CreateQuery<TAspect>(
//				Expression.Call(((MethodInfo)(typeof(System.Linq.Queryable).GetMember("Where")[0])).MakeGenericMethod(typeof(TAspect)),
//					Expression.Call(((MethodInfo)(typeof(System.Linq.Queryable).GetMember("Select")[0])).MakeGenericMethod(typeof(Aspect), typeof(TAspect)),
//						Expression.Call(((MethodInfo)(typeof(System.Linq.Queryable).GetMember("Where")[0])).MakeGenericMethod(typeof(Aspect)),
//							Expression.Parameter(typeof(System.Linq.IQueryable<Aspect>), DataStore.GetCollectionName(typeof(TAspect))),
//							aspectFilter), aspectSelector), predicate));
//			Log.DebugVariable("(return)", aspects);
//			return (IEnumerable<TAspect>)aspects; 
//		}
		#endregion
		#endregion
	}
}

