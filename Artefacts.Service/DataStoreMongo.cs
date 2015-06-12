using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ServiceStack.Logging;
using ServiceStack;
using System.Reflection;
using System.Collections;

namespace Artefacts
{
	/// <summary>
	/// Data store mongo.
	/// </summary>
	/// <remarks>
	/// TODO: Utilise <see cref="DataStore.RepositoryCache"/> and <see cref="DataStore.QueryCache"/>
	/// </remarks>
	public class DataStoreMongo : DataStore
	{
		#region Static members
		/// <summary>
		/// Gets the log.
		/// </summary>
		public new static IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Artefact)) { SourceClass = typeof(DataStoreMongo) };

		/// <summary>
		/// Logs the write concern result.
		/// </summary>
		/// <param name="wcr">Wcr.</param>
		private static void LogWriteConcernResult(WriteConcernResult wcr)
		{
			if (wcr.LastErrorMessage != null)
				Log.Error(" ! Error !\n\t" + wcr.LastErrorMessage);
			else
				Log.Debug("WriteConcernResult: " + wcr.Response.FormatString());//.Response.ToDictionary().ToJson());		//.FormatString());	//ToJson());
		}
		#endregion
		
		#region Properties & fields
		MongoCollection _mcArtefacts;
		MongoCollection _mcAspects;
		MongoQueryProvider _qpArtefacts;
		MongoQueryProvider _qpAspects;
		ExpressionVisitor _visitor;
		
		/// <summary>
		/// Sets the client.
		/// </summary>
		public MongoClient Client {
			get; protected set;
		}
		
		/// <summary>
		/// Sets the server.
		/// </summary>
		public MongoServer Server {
			get; protected set;
		}
		
		/// <summary>
		/// Sets the database.
		/// </summary>
		public MongoDatabase Database {
			get; protected set;
		}

		/// <summary>
		/// Gets the connection string.
		/// </summary>
		public string ConnectionString {
			get; private set;
		}
		#endregion
	
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.DataStoreMongo"/> class.
		/// </summary>
		public DataStoreMongo(string connectionString = "mongodb://localhost:27017", string databaseName = "Artefacts")
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				Log.ThrowError(new ArgumentOutOfRangeException("connectionString", connectionString, "string.isNullOrWhiteSpace"));
			if (string.IsNullOrWhiteSpace(databaseName))
				Log.ThrowError(new ArgumentOutOfRangeException("connectionString", databaseName, "string.isNullOrWhiteSpace"));
			
			ConnectionString = connectionString;
			Client = new MongoClient(ConnectionString);
			Server = Client.GetServer();
			if (Server == null)
				Log.ThrowError(new ApplicationException(string.Format("Could not get MongoServer from MongoClient \"{0}\"", Client)));
			Database = Server.GetDatabase(databaseName);
			if (Database == null)
				Log.ThrowError(new ApplicationException(string.Format("Could not get MongoDatabase \"{0}\" from MongoServer \"{1}\"", databaseName, Server)));
			Artefacts = GetRepository<Artefact>("Artefacts");
			Visitor = new MongoRepositoryVisitor(this);

			Log.Debug(this.FormatString());
		}

		#region Methods
		#region MongoDB Operations
		/// <summary>
		/// Gets the or create collection.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns>The or create collection.</returns>
		/// <remarks>
		/// Don't use for the moment: does not return MongoCollection but MongoCollection<BsonDocument> which I wasn't planning on
		/// Which may yet be useful once I consider how to deal with aspects of arbitrary types, queries on those types, etc
		/// </remarks>
		private MongoCollection GetOrCreateCollection(string name)
		{
			// See remarks above
			EnsureCollection(name);
			return Database.GetCollection(name);
		}
		
		/// <summary>
		/// Gets the or create collection.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">Collection element type</typeparam>
		/// <returns>The or create collection.</returns>
		private MongoCollection<T> GetOrCreateCollection<T>(string name)
		{
			EnsureCollection(name);
			return Database.GetCollection<T>(name);
		}
		
		/// <summary>
		/// Gets the or create collection.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">Collection element type</typeparam>
		/// <returns>The or create collection.</returns>
		private MongoCollection GetOrCreateCollection(Type type, string name)
		{
			EnsureCollection(name);
			return Database.GetCollection(type, name);
		}
		
		/// <summary>
		/// Creates the collection.
		/// </summary>
		/// <param name="name">Name.</param>
		private void EnsureCollection(string name)
		{
			if (!Database.CollectionExists(name))
			{
				if (!Database.CreateCollection(name).Ok)
					Log.ThrowError(new ApplicationException(string.Format("Could not create collection \"{0}\": {1}", name, Database.GetLastError().ErrorMessage)));
				Log.InfoFormat("Created collection \"{0}\"", new [] { name });
			}
			Log.DebugFormat("Pre-existing collection \"{0}\"", new[] { name });
		}
		#endregion
		
		/// <summary>
		/// Gets the repository.
		/// </summary>
		/// <param name="typeName">Type name.</param>
		/// <param name="collectionName">Collection name.</param>
		/// <param name="type">Type.</param>
		/// <returns>The repository.</returns>
		/// <remarks>IDataStore implementation (overrides from abstract base DataStore)</remarks>
		protected override IRepository CreateRepository(Type type, string collectionName)
		{
			MongoCollection collection = GetOrCreateCollection(type, collectionName);
			if (collection == null)
				Log.ThrowError(new ApplicationException(string.Format("Could not get or create MongoCollection \"{1}\" from MongoDatabase \"{2}\"", type.FullName, collection, Database)));
			IRepository repository = (IRepository)typeof(RepositoryMongo<>).MakeGenericType(type)//.CreateInstance();
				.GetConstructor(new Type[] { typeof(IDataStore), typeof(Expression), typeof(MongoCollection) })
				.Invoke(new object[] { this, Expression.Parameter(typeof(MongoCollection), collectionName), collection });
//				Expression.Parameter(typeof(IRepository<>).MakeGenericType(type)
			Log.DebugVariable("repository", repository);
			return repository;
		}
		#endregion
	}
}

