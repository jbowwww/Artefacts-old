using System;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Artefacts;
using System.Net;
using System.Reflection;
using MongoDB.Bson;

namespace Artefacts
{
	/// <summary>
	/// Artefacts repository proxy.
	/// </summary>
	public class RepositoryProxy<T> : Repository<T> where T : class
	{
		/// <summary>
		/// Gets or sets the client.
		/// </summary>
		public IServiceClient Client { get; protected set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.RepositoryProxy`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="provider">Provider.</param>
		/// <param name="expression">Expression.</param>
		public RepositoryProxy(IServiceClient client, IDataStore dataStore, Expression expression)
			: base(dataStore, expression)
		{
			Client = client;
			Visitor = new ProxyVisitor();
		}

		#region implemented abstract members of Repository()<T>)
		#region Artefacts.Queryable/IQueryProvider related operation implementations
		/// <summary>
		/// Gets the query.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <param name="query">Query.</param>
		/// <param name="Query">Query.</param>
		/// <returns>The query.</returns>
		public override IQueryable GetQuery(Expression expression)
		{	
//			Type repositoryType = typeof(IRepository).IsAssignableFrom(expression.Type) ? expression.Type : null;
			Type elementType = expression.GetElementType();
			Type queryableType = typeof(Queryable<>).MakeGenericType(elementType);
			ConstructorInfo ci = queryableType.GetConstructor(new Type[] {
				typeof(IRepository<>).MakeGenericType(elementType),		/*repositoryType ?? typeof(T)),*/
				typeof(Expression) });
			object queryable = ci.Invoke(new object[] { DataStore.GetRepository(elementType), expression });
			return (IQueryable)queryable;
		}

		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>To be added.</returns>
		public override object Evaluate(Expression expression)
		{
			object result = null;
			if (expression.NodeType == ExpressionType.Constant)
				result = ((ConstantExpression)expression).Value;		// is this still relevant or obsoleted? Think it was used for Count property
			else
			{
				// Can this be done in base.Execute() somehow?? To clean up this derived class
				if (expression.NodeType == ExpressionType.Call)
				{
					MethodCallExpression m = (MethodCallExpression)expression;
					if (m.Method.IsGenericMethod && m.Method.Name == "Count" && m.Arguments.Count == 1 && m.Arguments[0].IsEnumerable())
					{
						expression = m.Arguments[0];
//						Log.DebugVariable("Execute(e)", expression);
						result = Client.Get<QueryableResponse>(new GetQueryRequest(expression)).Count;
//						Log.DebugVariable("result", result);
					}
					else if (m.Method.IsGenericMethod && m.Method.Name == "As" && m.Method.DeclaringType == typeof(Aspect))
					{
						expression = m.Arguments[0];
//						Log.DebugVariable("Execute(e)", expression);
						result = Client.Get<QueryableResponse>(new GetQueryRequest(expression));
//						Log.DebugVariable("result", result);
					}
				}
				if (result == null)
					result = Client.Get<QueryableResponse>(new GetQueryRequest(expression));
			}
			return result;
		}
		#endregion

		/// <summary>
		/// Exists the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override bool Exists(ObjectId id)
		{
//			RepositoryCommandResponse response = Client.Get<RepositoryCommandResponse>(
//				new RepositoryCommand(RepositoryCommand.Command.Exists) { InstanceId = id });
//			return response.HasResult ? (bool)response.Result : false;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		public override void /*HttpWebResponse*/ Create(BsonDocument obj)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override T Get(ObjectId id)
		{
			T obj = Client.Get<T>(new GetArtefactById(id));
			Log.DebugFormat("id = {0}, return obj = {1}", new object[] { id, obj });
			return obj;
		}
		public override void /*HttpWebResponse*/ Create(T obj)
		{
//			HttpWebResponse response = 
			Client.Post(new Artefact(default(Uri), obj));
//			Log.DebugFormat("obj = {0}", new object[] { obj });		//, return response = {1}", , response });
//			return response;
//			return null;
		}
		public override void Update(T obj)
		{
//			RepositoryCommandResponse response = Client.Get<RepositoryCommandResponse>(
//				new RepositoryCommand(RepositoryCommand.Command.Update) { Instance = obj });
			throw new NotImplementedException();
		}
		public override void Save(T obj)
		{
//			RepositoryCommandResponse response = Client.Get<RepositoryCommandResponse>(
//				new RepositoryCommand(RepositoryCommand.Command.Save) { Instance = obj });
			throw new NotImplementedException();
		}
		public override void Delete(ObjectId id)
		{
//			RepositoryCommandResponse response = Client.Get<RepositoryCommandResponse>(
//				new RepositoryCommand(RepositoryCommand.Command.Delete) { InstanceId = id });
			throw new NotImplementedException();
		}
		public override void Delete(T obj)
		{
//			RepositoryCommandResponse response = Client.Get<RepositoryCommandResponse>(
//				new RepositoryCommand(RepositoryCommand.Command.Delete) { Instance = obj });
			throw new NotImplementedException();
		}
		#endregion
	}
}

