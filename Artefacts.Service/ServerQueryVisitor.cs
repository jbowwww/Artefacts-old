using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using MongoDB.Bson;

namespace Artefacts
{
	/// <summary>
	/// Server query visitor.
	/// </summary>
	/// <remarks>
	/// Replaces parameters named "Repository" of type <see cref="IArtefactsRepository"/> with the instance
	/// supplied to the constructor
	/// </remarks>
	public class ServerQueryVisitor : ExpressionVisitor
	{
		/// <summary>
		/// Determines if is repository place holder the specified e.
		/// </summary>
		/// <returns><c>true</c> if is repository place holder the specified e; otherwise, <c>false</c>.</returns>
		/// <param name="e">E.</param>
		public static bool IsRepositoryPlaceHolder(ParameterExpression e)
		{
			return typeof(IRepository<Artefact>).IsAssignableFrom(e.Type) && e.Name.Equals("Repository");
		}

		/// <summary>
		/// Gets the repository.
		/// </summary>
		public IDataStore DataStore { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.ServerQueryVisitor"/> class.
		/// </summary>
		/// <param name="repository">Repository.</param>
		public ServerQueryVisitor(IDataStore dataStore)
		{
			DataStore = dataStore;
		}

		/// <summary>
		/// Visit the specified exp.
		/// </summary>
		/// <param name="exp">Exp.</param>
		public override Expression Visit(Expression exp)
		{
			Expression reducedExp, longExp = base.Visit(exp);
			reducedExp = longExp != null && longExp.CanReduce ? longExp.Reduce() : longExp;
			return reducedExp;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
//			if (c.Type == typeof(ArtefactsRepository) && ((ArtefactsRepository)c.Value).Name == "Artefacts")
//				return Expression.Constant(this, typeof(ArtefactsRepository));
			
			if (typeof(IQueryable<Artefact>).IsAssignableFrom(c.Type))// && p.Name == "Artefacts")
				return Expression.Convert(Expression.Constant(c.Value), c.Type);	// typeof(IQueryable<Artefact>));
			if (c.Type.Equals(typeof(Type)))
				return Expression.Convert(
					Expression.Call(
					typeof(Type).GetMethod("GetType", new Type[] { typeof(string) }),
					Expression.Convert(Expression.Constant(((Type)c.Value).FullName, typeof(string)), typeof(string))),
					typeof(Type));
			return c;
		}
		
		/// <summary>
		/// Visits the parameter.
		/// </summary>
		/// <returns>The parameter.</returns>
		/// <param name="p">P.</param>
		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (p.Name == "Artefacts" && p.GetElementType() == typeof(Artefact))
				return Expression.Constant(DataStore.Artefacts);//, typeof(IQueryable<Artefact>));
			else if (p.Type.IsGenericType && p.Type.GetGenericTypeDefinition() == typeof(IQueryable<>))
				return Expression.Constant(DataStore.GetRepository(p.GetElementType()));
//			else if (!p.Type.HasElementType)
//				return Expression.Parameter(typeof(BsonDocument), p.Name);
//			else if (p.Name.StartsWith("Aspect_") && p.GetElementType() == typeof(Aspect))
//				return Expression.Constant(Repository.GetQueryable<Aspect>(p.Name));
			return p;
		}		

		/// <summary>
		/// Visits the member access.
		/// </summary>
		/// <returns>The member access.</returns>
		/// <param name="m">M.</param>
		protected override Expression VisitMemberAccess(MemberExpression m)
		{
//			if (m.Expression != null
//			 && (m.Member.MemberType == System.Reflection.MemberTypes.Field
//			 	 || m.Member.MemberType == System.Reflection.MemberTypes.Property))
//				return
//					Expression.Convert(
//						Expression.Call(
//							this.Visit(m.Expression),
//							typeof(BsonDocument).GetMethod("GetValue", new Type[] { typeof(string) }),
//							Expression.Constant(m.Member.Name)),
//						m.Type);
//			if (m.Equals(typeof(Artefact).GetMember("Data")))
//			{
//				if (m.Expression.NodeType != ExpressionType.Constant)
//					throw new ArgumentException("Can only access \"Data\" member on an artefact expressed as a constant");
//				Artefact artefact = (Artefact)((ConstantExpression)m.Expression).Value;
//				ParameterExpression aspect = Expression.Parameter(typeof(Aspect), "aspect");
//				Expression<Func<Aspect, bool>> matchId = Expression.Lambda<Func<Aspect, bool>>(
//					Expression.Equal(
//						Expression.MakeMemberAccess(aspect, typeof(Aspect).GetField("ArtefactId")),
//						aspect), aspect);
//				return Expression.Call(
//						typeof(System.Linq.Queryable).GetMethod("Where",
//							new Type[] {
//								typeof(IQueryable<Aspect>),
//								typeof(Expression<Func<Aspect, bool>>) }),
//						Expression.Constant(Repository.Aspects), matchId);
//			}
			return base.VisitMemberAccess(m);
		}
	}
}

//				return Expression.Constant(

//							Expression.Constant(Repository.Aspects, typeof(IQueryable<Aspect>)),
////							Expression<bool(Aspect)> =
//Expression.Lambda()
//								(Aspect aspect) => aspect.ArtefactId == artefact.Id));
//								
////								Expression.MakeMemberAccess(
////						((UnaryExpression)m.Expression), typeof(Aspect).GetMember("ArtefactId")[0])) );
//						}
////				return Expression.New(
////					typeof(MongoQueryable<Aspect>).GetConstructor(
////						new type[] { typeof(IQueryProvider) }),Repository.Aspects))
