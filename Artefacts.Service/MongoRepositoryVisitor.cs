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
	class MongoRepositoryVisitor : ExpressionVisitor
	{
		public IDataStore DataStore { get; protected set; }
		
		public MongoRepositoryVisitor (IDataStore dataStore)
		{
			DataStore = dataStore;
		}
		
		protected override Expression VisitBinary(BinaryExpression b)
		{
			return base.VisitBinary(b);
		}
		
		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (typeof(IRepository).IsAssignableFrom(p.Type))
			{
				Type elementType = p.GetElementType();
				Type repositoryType =typeof(RepositoryMongo<>).MakeGenericType(elementType);
				return Expression.MakeMemberAccess(
						Expression.Constant(
							DataStore.GetRepository(elementType, p.Name),
							repositoryType),
					repositoryType.GetProperty("MongoQueryable"));	//	typeof(MongoQueryable<T>));
						
			}
			return p;
		}


		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			if (m.Member.MemberType == MemberTypes.Field || m.Member.MemberType == MemberTypes.Property

			    && typeof(Dictionary<string, object>).IsAssignableFrom(m.Expression.Type))
			{
				DataStoreMongo.Log.DebugVariable("m", m);
				return
					Expression.Convert(
						//Expression.MakeIndex(
							//Expression.Convert(
					Expression.MakeMemberAccess(
					Expression.Parameter(typeof(Aspect), "a"),
					typeof(Aspect).GetProperty("Fields", typeof(Dictionary<string, object>))),
							//	typeof(MongoDB.Bson.BsonDocument)),
//							typeof(Dictionary<string, object>).GetProperty("this", typeof(object), new Type[] { typeof(string) }),
//							new Expression[] { Expression.Constant(m.Member.Name) }),
					m.Type);
			}
			return base.VisitMemberAccess(m);
		}
	}
}

