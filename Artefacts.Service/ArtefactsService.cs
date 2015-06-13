using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using ServiceStack;
using ServiceStack.Logging;
using MongoDB.Bson;
using System.Net;

namespace Artefacts
{
	/// <summary>
	/// Artefacts service.
               	/// </summary>
	public class ArtefactsService : IService
	{
		#region Static members
		/// <summary>
		/// Gets the log.
		/// </summary>
		public static IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(ArtefactsService));
		#endregion

		#region Fields & Properties
		/// <summary>
		/// Gets the server.
		/// </summary>
		public Host Server { get; private set; }

		/// <summary>
		/// Gets the repository.
		/// </summary>
		public IDataStore DataStore { get; private set; }

		/// <summary>
		/// Gets the plugins.
		/// </summary>
		public List<Assembly> Plugins { get; private set; }
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.ArtefactsService"/> class
		/// with a new Guid and the name "New Domain"
		/// </summary>
		/// <remarks>Call when creating the service for a new domain</remarks>
		public ArtefactsService(string connString = "mongodb://localhost:27017", string dbName = "Artefacts")
		{
			Server = Host.Current;
			DataStore = new DataStoreMongo(connString, dbName);
			List<string> PluginPaths = new List<string>();//new [] { "Serialize.Linq.dll", "System.Core.dll" });
			foreach (string pluginAssembly in Directory.GetFiles("../../../bin/Debug/Plugins/", "*.dll", SearchOption.AllDirectories))
				PluginPaths.Add(pluginAssembly);

			Plugins = new List<Assembly>();
			foreach (string pluginPath in PluginPaths)
			{
				Log.Debug("Plugin assembly path: " + pluginPath);
				Plugins.Add(Assembly.LoadFile(pluginPath));
			}
			Log.Debug(this.FormatString());			//			Log.Debug(this.SerializeToString());
		}
		#endregion
	
		#region Methods
		/// <summary>
		/// Post the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Post(Artefact artefact)
		{
			Log.DebugVariable("artefact", artefact);
//			HttpWebResponse response = RequestContext.Instance.GetDto<HttpWebResponse>();
			object response = 0;
			DataStore.Artefacts.Create(artefact);
//			response.Prepare();
//			Log.Debug(response.SerializeToString());
			return null;
		}

		/// <summary>
		/// Get the specified artefactId.
		/// </summary>
		/// <param name="artefactId">Artefact identifier.</param>
		public Artefact Get(GetArtefactById artefactId)
		{
			Log.DebugVariable("artefactId", artefactId.Id);
			Artefact artefact = DataStore.Artefacts.Get(artefactId.Id);
			Log.DebugVariable("artefact", artefact);
			return artefact;
		}

		/// <summary>
		/// Get the specified query.
		/// </summary>
		/// <param name="query">Query.</param>
		public object Get(GetQueryRequest query)
		{
			object resultFixed = null;
//try
//			{
//			Log.DebugVariable("query.Data", query == null ? "Query is null!" : query.Expression.ToExpressionNode().ToString());// JsvFormatter.Format(query.Data));
//			Expression expression = query.Expression;
			Log.DebugVariable("query.Expression", query.Expression);
			object result = DataStore.Execute(query.Expression);
			Log.DebugVariable("result", result);
			if (result == null)
				resultFixed = result;
			else if (typeof(IEnumerable<Artefact>).IsAssignableFrom(result.GetType()))
				resultFixed = new QueryableResponse(query.Expression.Id(), ((IEnumerable<Artefact>)result).Select<Artefact, ObjectId>((a) => a.Id).ToArray());
			else if (typeof(IEnumerable<Aspect>).IsAssignableFrom(result.GetType()))
				resultFixed = new QueryableResponse(query.Expression.Id(), ((IEnumerable<Aspect>)result).Select<Aspect, ObjectId>((a) => a.Id).ToArray());

			if (resultFixed != result)
				Log.DebugVariable("resultFixed", resultFixed);
//			}
//			catch (Exception ex)
//			{
//				Log.Error("", ex.FormatString());
//				throw new WebServiceException("",ex);
//			}
			return resultFixed;
		}
		#endregion
	}
}

