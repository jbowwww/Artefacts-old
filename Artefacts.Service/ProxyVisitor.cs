using System;
using System.Linq.Expressions;
//using Artefacts.Extensions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace Artefacts
{
	public class ProxyVisitor : ExpressionVisitor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.ProxyVisitor"/> class.
		/// </summary>
		public ProxyVisitor()
		{
		}
		
//		protected override Expression VisitBinary(BinaryExpression b)
//		{
//			if (b.NodeType == ExpressionType.TypeIs || (b.NodeType == ExpressionType.Equal))
//			{
//				if (b.Left.Type == typeof(Type))
//				{
//					MemberInfo mi = typeof(Type).GetProperty("FullName");
//					if (b.Right.Type == typeof(Type))
//						return Expression.Call(
//							b.Method,
//							Expression.MakeMemberAccess(b.Left, mi),
//							Expression.MakeMemberAccess(b.Right, mi));
//					else
//						return b;
//				}
//				else return b;
//			}
//			return b;
//		}
		
		protected override Expression VisitTypeIs(TypeBinaryExpression b)
		{
			PropertyInfo pFullName = typeof(Type).GetProperty("FullName");
			return Expression.Equal(
				Expression.MakeMemberAccess(b.Expression, pFullName),
				Expression.Constant(b.TypeOperand.FullName, typeof(string)));
		}
		
		/// <summary>
		/// Visits the method call.
		/// </summary>
		/// <returns>The method call.</returns>
		/// <param name="m">M.</param>
		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m.Arguments.Count >= 1 && m.Arguments[0] != null)
			{
				if (m.Method.DeclaringType.Equals(typeof(System.Linq.Enumerable))
				 ||	m.Method.DeclaringType.Equals(typeof(System.Linq.Queryable)))
				{
					if (typeof(IEnumerable).IsAssignableFrom(m.Method.ReturnType))
					{
					
						if (m.Method.Name.Equals("Where")
						 && m.Arguments.Count == 2
						 && m.Arguments[1].NodeType == ExpressionType.Lambda)//.Type.IsDelegate())
							return Expression.Call(m.Method, VisitExpressionList(m.Arguments));
							// new Expression[] { Expression.GetDelegateType(})
					}
				}
			}
			else if (m.Arguments.Count == 0 && m.Object != null && m.Method.Name == "ToString" && m.Method.ReturnType == typeof(string))
			{
				if (m.Object.NodeType == ExpressionType.Constant)
					return Expression.Constant(m.Method.Invoke(((ConstantExpression)m.Object).Value, new object[] {}), m.Method.ReturnType);
			}
		
			else
			{
				Expression obj = m.Object != null ? this.Visit(m.Object) : null;
				IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
				if ((_previousVisit == null || _previousVisit.NodeType != ExpressionType.Lambda)
				 && (obj == null || obj.NodeType == ExpressionType.Constant)
				 && args == null /*|| (args.All((e) => e.NodeType == ExpressionType.Constant)*/
				  && args.Count() > 0
				  && !typeof(IEnumerable).IsAssignableFrom(args.ElementAt(0).Type)
				  && !typeof(IQueryable).IsAssignableFrom(args.ElementAt(0).Type))
					return this.Visit(Expression.Constant(
						m.Method.DeclaringType.InvokeMember(
							m.Method.Name,
							BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod |
								(obj == null ? BindingFlags.Static : BindingFlags.Instance),
							null,
							obj == null ? null : ((ConstantExpression)obj).Value,
							args == null ? null : args.Cast<ConstantExpression>().Select((c) => c.Value).ToArray()),
							m.Method.ReturnType));
				if (obj != m.Object || args != m.Arguments)
					return Expression.Call(obj, m.Method, args);
				return m;
			
			}	
			return base.VisitMethodCall(m);
		}
		
		/// <summary>
		/// Visits the constant.
		/// </summary>
		/// <returns>The constant.</returns>
		/// <param name="c">C.</param>
		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (c.Type.IsPrimitive || c.Type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public) != null)
				return Expression.Convert(
					Expression.Call(
						c.Type.GetMethod("Parse", new Type[] { typeof(string) }),
						Expression.Constant(c.Value.ToString())), c.Type);
			
			if (c.Type.Equals(typeof(Type)))
				return Expression.Convert(
					Expression.Call(
					typeof(Type).GetMethod("GetType", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
				                         null, new Type[] { typeof(string) }, null),
					Expression.Convert(Expression.Constant(((Type)c.Value).FullName, typeof(string)), typeof(string))), typeof(Type));
						

			// Old - remove??
//			if (typeof(Repository<>).IsAssignableFrom(c.Type))
//				return Expression.Parameter(c.Type, "Aspects");
//			
			return base.VisitConstant(c);
		}
		
		
		protected override Expression VisitNew(NewExpression nex)
		{
//				if (nex.Type.FullName == "System.String" && nex.Arguments.Count == 1 && nex.Arguments[0].Type == typeof(String))
//					return Expression.Constant(nex.Arguments[0], typeof(String));
//				else
//				{
//					return Expression.New(nex.Constructor, this.VisitExpressionList(nex.Arguments));
//				}
			return base.VisitNew(nex);
		}
	}
}

