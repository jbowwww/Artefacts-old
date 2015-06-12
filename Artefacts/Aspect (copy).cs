using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using MongoDB.Bson;
using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Reflection;
using System.Dynamic;
using ServiceStack.Logging;

namespace Artefacts
{
	[DataContract(IsReference = true, Namespace = "Artefacts")]
	public class Aspect : Dictionary<string, object>//, IConvertibleToBsonDocument
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Aspect)) { SourceClass = typeof(Aspect) };
		#endregion
		
		[BsonIgnore]
		public object Instance {
			get { return _instance; }
			set
			{
				Type = (_instance = value) == null ? typeof(object) : value.GetType();
				
				
//				FormatterServices.GetObjectData(Instance, Members);
			}
		}
		private object _instance;

		[BsonId, DataMember(IsRequired = true)]
		public ObjectId Id {// get; set; }
			get { return ObjectId.Parse((string)base["_id"]); }
			set { base["_id"] = value.ToString(); }
		}
		
		[BsonIgnore]
		public Artefact Artefact {
			get { return _artefact; }
			set { ArtefactId = (_artefact = value) == null ? new string("0", 24) : value.Id; }
		}
		private Artefact _artefact;
		
		[BsonRequired, DataMember(IsRequired = true)]
		public string ArtefactId {// get; set; }
			get { return ObjectId.Parse((string)base["ArtefactId"]); }
			set { base["ArtefactId"] = value.ToString(); }
		}

		[BsonIgnore]
		public Type Type {
			get { return Type.GetType(TypeName); }
			set
			{
				if (Instance != null && !value.IsAssignableFrom(Instance.GetType()))
					throw new ArgumentOutOfRangeException("TypeName", value, "Type not assignable from pre-existing instance type");
				if (value != typeof(System.Object) && value != typeof(ValueType))
				{
					Members = value.GetMembers(MemberGetBinding).Where(MemberSelector).ToArray();
					_values = Members.Select(
						(member) => value.InvokeMember(
							member.Name,
							BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.GetField | BindingFlags.GetProperty,
							null, Instance, null)).ToArray();
					
				}
				TypeName = value.FullName;
			}
		}
		
		[DataMember, DataMember(IsRequired = false)]
		public DateTime TimeCreated { get; protected set; }

		[BsonRequired, DataMember(IsRequired = true)]
		public string TypeName { get; protected set; }
		
		public BindingFlags MemberSelectBinding { get; protected set; }
		
		public Func<MemberInfo, bool> MemberSelector { get; protected set; }
		
		public MemberInfo[] Members { get; protected set; }
		
		public object[] Values { get; protected set; }

//		[BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.Document)]
		public Dictionary<string, object> Fields {
			get { return (Dictionary<string,object>)this; }
			set {
				base.Clear();
				foreach (KeyValuePair<string,object> kvp in value)
					base[kvp.Key] = kvp.Value;
			}
		}
		
		public Aspect()
		{
			;
		}

		public Aspect(Artefact artefact, object aspect, Func<MemberInfo, bool> memberSelector = null, BindingFlags memberSelectBinding = BindingFlags.Default)
		{
			TimeCreated = DateTime.Now;
			
			MemberGetBinding =
				memberSelectBinding != BindingFlags.Default ?
					memberSelectBinding : BindingFlags.Instance |
						BindingFlags.Public | BindingFlags.NonPublic |
						BindingFlags.GetField | BindingFlags.GetProperty;
			MemberSelector = memberSelector ??
				new Func<MemberInfo, bool>(
					(member) => (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
				 	 && !member.Name.StartsWith("get_") && !member.Name.StartsWith("set_")
				 	 && member.DeclaringType != typeof(Aspect) && (member.GetCustomAttribute<BsonIgnoreAttribute>() == null));
			
			Id = ObjectId.GenerateNewId();
			Artefact = artefact; 
			Instance = aspect;
			
			Log.DebugFormat("Artefact: {0}, Aspect: {1}, Members: {2}",
				new object[] { artefact, aspect, memberSelector,
				string.Join(", ", Members.Select((member) => member.Name)) });
			
			for (int i = 0; i < Members.Length; i++)
				Add(Members[i].Name, _values[i]);
		}
		
//		[BsonConstructor]
		public Aspect(SerializationInfo info, StreamingContext context)
		{
		;
//			Id = ObjectId.Parse(info.GetString("Id"));
//			ArtefactId = ObjectId.Parse(info.GetString("ArtefactId"));
//			Type = info.GetString("Type");
//			Instance = Activator.CreateInstance(Type.GetType(Type));
//			//TimeCreated = info.GetDateTime("TimeCreated");
//			MemberValues = new object[info.MemberCount];
//			MemberNames = new string[info.MemberCount];
//			Members = new MemberInfo[info.MemberCount];
//			foreach (KeyValuePair<string, object> entry in this)
//			{
//				if (!entry.Key.StartsWith("Time"))
//				{
//					Type.GetType(Type).GetField(entry.Key).SetValue(Instance, entry.Value);
//				}
//			}
		}
		
		#region IConvertibleToBsonDocument implementation
//		public BsonDocument ToBsonDocument()
//		{
//		}
		#endregion
	}
}

