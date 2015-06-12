using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Serialization;
using MongoDB.Driver.Builders;
using ServiceStack.Web;
using ServiceStack.DataAnnotations;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using ServiceStack.Logging;

namespace Artefacts
{
	/// <summary>
	/// Queryable.
	/// </summary>
	public class Queryable<T> : IOrderedQueryable<T> where T : class
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Queryable<T>)) { SourceClass = typeof(Queryable<T>) };
		#endregion

		/// <summary>
		/// Queryable enumerator
		/// </summary>
		public class QueryableEnumerator : IEnumerator<T>
		{
			#region Private fields
			internal Queryable<T> _queryable;
			internal int _index;
			#endregion

			/// <summary>
			/// Gets the current item
			/// </summary>
			/// <returns>The current <typeparamref name="T"/></returns>
			/// <remarks>IEnumerator[T] implementation</remarks>
			public T Current {
				get
				{
					if (_index < 0)
						throw new InvalidOperationException("Enumerator does not have a current item");
					return (T)_queryable[_index];
				}
			}

			/// <summary>
			/// Gets the current.
			/// </summary>
			/// <remarks>IEnumerator implementation</remarks>
			object IEnumerator.Current {
				get { return Current; }
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Artefacts.Service.Queryable`2+QueryableEnumerator"/> class.
			/// </summary>
			/// <param name="queryable">Queryable.</param>
			internal QueryableEnumerator(Queryable<T> queryable)
			{
				_queryable = queryable;
				Reset();
//				Log.Debug(this);
			}

			/// <summary>
			/// Moves the next.
			/// </summary>
			/// <returns><c>true</c>, if next was moved, <c>false</c> otherwise.</returns>
			/// <remarks>IEnumerator implementation</remarks>
			public bool MoveNext()
			{
				return ++_index < _queryable.Count;
			}

			/// <summary>
			/// Reset this instance.
			/// </summary>
			/// <remarks>IEnumerator implementation</remarks>
			public void Reset()
			{
				_index = -1;
			}

			/// <summary>
			/// Releases all resource used by the <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/> object.
			/// </summary>
			public void Dispose()
			{

			}
		}

		#region Fields & properties
		/// <summary>
		/// Gets the <see cref="Queryable`1[TArtefact].Expression"/> identifier.
		/// </summary>
		public object Id {
			get { return _id ?? (_id = Expression.Id()); }
		}
		private object _id = null;
		
		/// <summary>
		/// The time created.
		/// </summary>
		public DateTime TimeCreated {
			get; protected set;
		}

		/// <summary>
		/// The time retrieved.
		/// </summary>
		public DateTime TimeRetrieved {
			get; protected set;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is up to date.
		/// </summary>
		public bool IsUpToDate {
			get { return true; }
		}

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		public Expression Expression {
			get; protected set;
		}

		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <remarks><see cref="System.Linq.IQueryable"/> implementation</remarks>
		public Type ElementType {
			get; protected set;
		}
		
		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <remarks>IQueryable implementation</remarks>
		public IQueryProvider Provider {
			get { return (IQueryProvider)Repository; }
		}
		
		/// <summary>
		/// Sets the provider
		/// </summary>
		public IRepository<T> Repository {
			get; protected set;
		}

		/// <summary>
		/// Gets the response.
		/// </summary>
		public object Response {
			get { 
				return _response ?? (_response = Repository.Evaluate(Expression));
//				 (Expression.GetElementType() == typeof(Artefact) ?
//					(object)Provider.Execute<QueryableResponse<Artefact>>(Expression) :
//					(object)Provider.Execute<QueryableResponse<Aspect>>(Expression)));
			}
		}
		private object _response = null;
		
		/// <summary>
		/// Gets or sets the unknown response.
		/// </summary>
		public virtual QueryableResponse Result {
			get { return (QueryableResponse)Response; }
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		public int Count {
			get
			{ 
				if (Result == null)
					Log.ThrowError(string.Concat("Cannot get count for {0} with Expression \"{1}\"", this, Expression),
						new NullReferenceException("result=null, response=" + Response.ToString()));
				return Result.Count;
			}
		}

		/// <summary>
		/// Gets the <see cref="Artefacts.Service.Queryable`1[T]"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public T this[int index] {
			get { return Repository.Get(Result.Ids[index]); }		// Result.Get(index, Provider); }
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Queryable"/> class.
		/// </summary>
		/// <param name="provider">Provider.</param>
		/// <param name="expression">Expression.</param>
		/// <param name="elementType">Element type.</param>
		public Queryable(IRepository<T> repository, Expression expression = null)
		{
			Repository = repository;
			Expression = expression;
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		/// <remarks>IEnumerable implementation</remarks>
		public IEnumerator<T> GetEnumerator()
		{
			return new QueryableEnumerator(this);
		}
		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <remarks>IEnumerable implementation</remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}
	}
}
