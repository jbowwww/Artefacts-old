using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using ServiceStack.Logging;
using MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;
using System.Diagnostics;

//using ServiceStack.Serialization;
using MongoDB.Bson.Serialization;
using System.Collections;
using ServiceStack.DataAnnotations;

namespace Artefacts
{
	/// <summary>
	/// Defines a data relation ship between the implementing class and <typeparamref name="TRelated"/>
	/// </summary>
	/// <typeparam name="TRelated"></typeparam>
	public interface IDataRelation<TParent, TRelated> 
		where TParent : class
		where TRelated : class
	{
		/// <summary>
		/// Raises the parent created event.
		/// </summary>
		/// <param name="parent">Parent.</param>
		/// <param name="relatedRepository">Related repository.</param>
		/// <param name="dataStore">Data store.</param>
		void OnInserted(IDataStore dataStore);
	}
}

