using System;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using ServiceStack.Logging;

namespace Artefacts
{
	/// <summary>
	/// Repository mongo.
	/// </summary>
	/// <remarks>TODO: Everything!</remarks>
	public class RepositoryMongo<T> : Repository<T> where T : class
	{
	
		#region Properties & fields
		/// <summary>
		/// Sets the collection.
		/// </summary>
		public MongoCollection<T> MongoCollection { get; protected set; }

		/// <summary>
		/// Gets or sets the mongo queryable.
		/// </summary>
		public MongoQueryable<T> MongoQueryable { get; protected set; }
	
		/// <summary>
		/// Gets or sets the mongo provider.
		/// </summary>
		public MongoQueryProvider MongoProvider { get; protected set; }
		#endregion
	
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.RepositoryMongo"/> class.
		/// </summary>
		public RepositoryMongo(IDataStore dataStore, Expression expression, MongoCollection collection) :
			base(dataStore, expression)
		{
			Visitor = new MongoQueryVisitor<T>(this);
			MongoCollection = (MongoCollection<T>)collection;
			MongoProvider = new MongoQueryProvider(MongoCollection);
			MongoQueryable = new MongoQueryable<T>(MongoProvider);
		}
		
		#region IQueryProvider /Queryable related operations
		/// <summary>
		/// Gets the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="expression">Expression.</param>
		public override IQueryable GetQuery(Expression expression)
		{
			Type elementType;
			bool foundElementType;
			foundElementType = expression.TryGetElementType(out elementType);
			if (!foundElementType)
				throw new ArgumentOutOfRangeException("expression", expression, "");
			// TODO: Mod TryGetElementType to get elementtype of argument to method/property/etc if outer expression is not an element type
			return new MongoQueryable<T>(MongoProvider, expression);
		}
		
		/// <summary>
		/// Evaluate the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		public override object Evaluate(Expression expression)
		{
			object result;
			Type elementType;
			bool foundElementType = expression.TryGetElementType(out elementType);
			if (!foundElementType)
				throw new ArgumentOutOfRangeException("expression", expression, "");
			result = MongoProvider.Execute(expression);
			if (typeof(IEnumerable<Artefact>).IsAssignableFrom(result.GetType()))
				result = new QueryableResponse(expression.Id(), ((IEnumerable<Artefact>)result).Select<Artefact, string>((a) => a.Id).ToArray());
			else if (typeof(IEnumerable<Aspect>).IsAssignableFrom(result.GetType()))
				result = new QueryableResponse(expression.Id(), ((IEnumerable<Aspect>)result).Select<Aspect, string>((a) => a.Id).ToArray());
			return result;
		}
		#endregion
		
		#region Methods
		/// <summary>
		/// Exists the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override bool Exists(string id)
		{
			return MongoCollection.FindOneByIdAs(typeof(T), BsonValue.Create(id)) != null;
		}
		
		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override T Get(string id)
		{
			return MongoCollection.FindOneByIdAs<T>(new BsonObjectId(id));
		}
		
		/// <summary>
		/// Create the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		public override void Create(T obj)
		{
			MongoCollection.Insert(obj);
		}
		
		/// <summary>
		/// Update the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		public override void Update(T obj)
		{
			Log.ThrowError(new NotImplementedException());
		}
		
		/// <summary>
		/// Save the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		public override void Save(T obj)
		{
			MongoCollection.Save(obj);
		}
		
		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override void Delete(string id)
		{
			Log.ThrowError(new NotImplementedException());
		}
		
		/// <summary>
		/// Delete the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		public override void Delete(T obj)
		{
			Log.ThrowError(new NotImplementedException());
		}
		#endregion
	}
}

using System;

namespace ArtefactService
{
	public class EmptyClass
	{
		public EmptyClass()
		{
		}
	}
}

