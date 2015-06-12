using System;
using System.Collections.Generic;
using Artefacts;
using System.Linq.Expressions;
using System.Linq;
using MongoDB.Driver.Linq;
using System.Collections;
using System.Reflection;

namespace Artefacts
{
	/// <summary>
	/// Mongo query visitor.
	/// </summary>
	public class MongoQueryVisitor<T> : Artefacts.ExpressionVisitor where T : class
	{
		/// <summary>
		/// The repository.
		/// </summary>
		public RepositoryMongo<T> Repository { get; protected set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.MongoQueryVisitor"/> class.
		/// </summary>
		public MongoQueryVisitor(RepositoryMongo<T> repository)
		{
			Repository = repository;
		}
		
		/// <summary>
		/// Visits the parameter.
		/// </summary>
		/// <param name="p">P.</param>
		/// <returns>The parameter.</returns>
		/// <remarks>Deal with embedded IQueryable nodes representing (not)cached queries?</remarks>
		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (typeof(IEnumerable).IsAssignableFrom(p.Type))
			{
//				return Expression.MakeMemberAccess(
//					Expression.Constant(Repository, typeof(RepositoryMongo<T>)),
//					typeof(RepositoryMongo<T>).GetProperty("MongoQueryable"));
				Type rootType = p.GetRootElementType();
				Type repositoryType =typeof(RepositoryMongo<>).MakeGenericType(rootType);
				return Expression.MakeMemberAccess(
					Expression.Constant(
						Repository.DataStore.GetRepository(rootType, p.Name),
						repositoryType),
					repositoryType.GetProperty("MongoQueryable"));	//	typeof(MongoQueryable<T>));
			}
			return base.VisitParameter(p);
		}
		
		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
//			if (m.Method.Name == "SelectMany"	// TODO: Narrow down m.Method.DeclaringType filter, to just MongoQueryable<> ? after testing
//			 && (	m.Method.DeclaringType == typeof(MongoQueryable<>)
//			 	 ||	m.Method.DeclaringType == typeof(Enumerable)
//			 	 || m.Method.DeclaringType == typeof(Queryable))
//			 && m.Method.IsGenericMethod) //m.Method.GetGenericArguments().Length == 2)
//			{
//				Type inType = m.Method.GetGenericArguments()[0];
//				Type outType = m.Method.GetGenericArguments()[1];
//				Type enumerableType = typeof(List<>).MakeGenericType(outType);
//				IList list = (IList)typeof(List<>).MakeGenericType(outType).GetConstructor(new Type[] {}).Invoke(new object[] {});
//				//ConstructorInfo ci = enumerableType.GetConstructor(new Type[] {});
////				IEnumerable enumerable;// = (IEnumerable)ci.Invoke(new object[] {});
//				MethodInfo mi = typeof(Queryable).GetMethod("Select",
//					new Type[] {
//						typeof(IQueryable<>).MakeGenericType(inType),
//						typeof(Expression<>).MakeGenericType(typeof(Func<>).MakeGenericType(outType)) });
//					//System.Linq.Expressions.Expression`1[System.Func`2[TSource,TResult]])}
//				// typeof(Queryable).GetMethod("Select", new Type[] { inType, outType });
////				Expression call = Expression.Call(mi, m.Arguments);
//				IEnumerable r = ((IQueryable<T>)Repository.Execute(m.Arguments[0])).Select((Expression)m.Arguments[1]);
//				//(IEnumerable)Repository.Execute(call);
//				foreach (IEnumerable enumerable in r)
//					{
//						list.Add(enumerable);
//					}
//				return Expression.Constant(list.AsQueryable());
//			}
			
			if (m.Method.IsGenericMethod && typeof(IEnumerable).IsAssignableFrom(m.Type) &&
				m.Method.GetParameters().Length > 0 &&
				typeof(IEnumerable<T>).IsAssignableFrom(m.Method.GetParameters()[0].ParameterType) &&
				!m.Type.IsAssignableFrom(m.Arguments[0].Type) &&
//				m.Arguments[0].Type.GetGenericArguments()[0] == typeof(T)
				m.Method.GetParameters()[1].ParameterType.IsClass)
			return Expression.Convert(
					Expression.Call(
					m.Method,
					new List<Expression>(
					new Expression[] { Expression.Constant(
							Repository.CreateQuery(
								this.Visit(m.Arguments[0])))
				}).AsEnumerable()
									.Concat(m.Arguments.Skip(1).AsEnumerable())),
					m.Type);
						
//			{
//				Type returnType = ((LambdaExpression)((UnaryExpression)m.Arguments[1]).Operand).ReturnType;
//				if (m.Method.Name == "SelectMany")
////				 && typeof(IEnumerable).IsAssignableFrom(m.Arguments[0].Type)
////				 && typeof(IEnumerable).IsAssignableFrom(m.Arguments[1].Type))
//				 //&& typeof(IEnumerable).IsAssignableFrom(m.Method.GetParameters()[1].ParameterType))
//				{
//					Type newElementType = returnType.GetGenericArguments()[0];// m.Method.GetParameters()[1].ParameterType.GetGenericArguments()[0];
//					IList many = (IList)typeof(List<>).MakeGenericType(newElementType)
//						.GetConstructor(new Type[] { typeof(int) })
//						.Invoke(new object[] { 8 });
//
//					IQueryable<T> original =
//						m.Arguments[0].NodeType == ExpressionType.Constant
//					 && ((ConstantExpression)m.Arguments[0]).Value.GetType() == typeof(MongoQueryable<T>)
//					 ? ((IQueryable<T>)((ConstantExpression)m.Arguments[0]).Value)
//					 : ((IQueryable<T>)Repository.CreateQuery(m.Arguments[0]));
//					IEnumerable originalSelection;
//					foreach (T instance in original)
//					{
//						originalSelection = (IEnumerable)m.Method.Invoke(instance, new object[] { m.Arguments[1]});
//						foreach (object value in originalSelection)
//							many.Add(value);
//					}
//					return Expression.Constant(Repository.CreateQuery(Expression.Constant(many, m.Type)));
//				}

			else if (m.NodeType == ExpressionType.Equal && m.Arguments.Count == 1
			 && m.Object.Type == typeof(Type) && m.Arguments[0].Type == typeof(Type))
			 	{
			 	throw new ApplicationException("TODO");
			 	}
			 	
// Check for SelectMany call
// TODO: Move this up and/or remove after coding section at top of func
			
				return base.VisitMethodCall(m);
				
		}
	}
}

