using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Serialize.Linq;
using Serialize.Linq.Serializers;
using Serialize.Linq.Interfaces;
using Serialize.Linq.Nodes;
using Serialize.Linq.Extensions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
//using ServiceStack.Text;
using ServiceStack.Serialization;
//using ServiceStack.Text.Common;
//using ServiceStack.Text.Json;
using System.Text;
//using ServiceStack.Text.Jsv;
//using Artefacts.Extensions;

namespace Artefacts
{
	/// <summary>
	/// Get query request.
	/// </summary>
	[DataContract]
	public class GetQueryRequest : ExpressionContext
	{
		/// <summary>
		/// The serializer.
		/// </summary>
//		public static ITypeSerializer Serializer = new JsvTypeSerializer();
//public ExpressionSerializer Serializer = new ExpressionSerializer(new Serializer());

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		[DataMember]
		public string Data { get; set; }

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		public Expression Expression {
			get
			{
				return Data == null ? Expression.Empty() : Data.FromJson<ExpressionNode>().ToExpression();//this);// Serializer.DeserializeText(Data);	// Data.FromJsv<ExpressionNode>().ToExpression();	// Data.FromCsvField().ToExpression(this);	// FromJson<ExpressionNode>()
//						(node) => (node.NodeType == ExpressionType.Convert
//					 && ((UnaryExpressionNode)node).Operand.NodeType == ExpressionType.Constant)
//					 	? ((UnaryExpressionNode)node).Operand.Type.Name.IsNullOrEmpty()
//					 		? (((ConstantExpression)((UnaryExpressionNode)node).Operand).Value == null
//					 			? Expression.Constant(default(node.Type.ToType()))
//					 			: (node.Type.IsAssignableFrom(((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value.GetType())
//					 				? Expression.Constant(((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value, node.Type)
//					 				: ((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value.GetType() == typeof(string)
//					 					? Node.Type.ToType().GetMethod("Parse", new Type[] { typeof(string)}).Invoke(
//					 						((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value)
//					 					: Expression.Constant(Convert.ChangeType(
//					 						((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value,
//					 							node.Type.ToType()), node.Type.ToType())))
//					 		: ((UnaryExpressionNode)node).Operand.Type.Name.EndsWithIgnoreCase("string")
//					 			? Node.Type.ToType().GetMethod("Parse", new Type[] { typeof(string)}).Invoke(
//					 					((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value)
//					 			: Expression.Constant(Convert.ChangeType(
//					 				((ConstantExpression)(((UnaryExpressionNode)node).Operand)).Value,
//					 				node.Type.ToType()), node.Type.ToType()));
			}	
			set
			{
				Data = value.ToExpressionNode().ToJson();	// value.ToJson();//.ToJsv(); //ExpressionExtensions.ToString(Expression);	// value.ToString();		//ToExpressionNode().
			}
		}
//				return ExpressionNode == null ? Expression.Empty() : ExpressionNode.ToExpression();
//				 Se.DeserializeFromString(Data).ToExpression();;
//				 :	((ExpressionNode)Serializer.GetParseFn<Expression>().Invoke(Data)).ToExpression();
//				.DeserializeFromString<Expression>(Data); 
//				StringBuilder sb = new StringBuilder();
//				Serializer.GetWriteFn<Expression>().Invoke(new StringWriter(sb), value.ToExpressionNode());
//				Data = sb.ToString();
//				Serializer.SerializeToString(value.ToExpressionNode());
//				ExpressionNode = value.ToExpressionNode();

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.GetQueryRequest"/> class.
		/// </summary>
		public GetQueryRequest()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.GetQueryRequest"/> class.
		/// </summary>
		/// <param name="expression">Expression.</param>
		 public GetQueryRequest(Expression expression)
		{
			Expression = expression;
		}
		
		public override ParameterExpression GetParameterExpression(ParameterExpressionNode node)
		{
			Type nodeType = ResolveType(node.Type);
			if (typeof(IRepository).IsAssignableFrom(nodeType) && node.Type.GenericArguments.Length == 1)
//				return Expression.Constant(
//					DataStore.Context.GetRepository(ResolveType(node.Type.GenericArguments[0]), node.Name),
//					ResolveType(node.Type));
				return Expression.Parameter(nodeType, node.Name);
			return base.GetParameterExpression(node);
		}
//		public override ParameterExpression GetParameterExpression(ParameterExpressionNode node)
//		{
//		return Expression.Parameter(node.Type.ToType(this), node.Name);
//			if (node.Name == "Artefacts" && typeof(IQueryable<Artefact>).IsAssignableFrom(node.Type.ToType(this)))
//				return Expression.Constant(Artefacts.ArtefactsRepositoryMongo.Context.Artefacts, typeof(IQueryable<Artefact>));
//			return node;
//		}
		
		
		public override Type ResolveType(TypeNode node)
		{
			return base.ResolveType(node);
		}
		
		public override string ToString()
		{
			return string.Concat("[GetQueryRequest: ", this.SerializeToString(), "]");
		}
	}
}

