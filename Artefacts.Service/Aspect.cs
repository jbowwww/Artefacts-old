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
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection;
using System.Dynamic;
using ServiceStack.Logging;
using System.ComponentModel;
using System.Text;
using ServiceStack;

namespace Artefacts
{	
	[DataContract(IsReference = true, Namespace = "Artefacts")]
	public class Aspect : IEquatable<Aspect>
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Aspect));
		#endregion

		#region Fields & properties
		[BsonIgnore, IgnoreDataMember]
		public object Instance {
			get {
				// If this aspect was deserialized from data storage, create instance & populate from fields
				if (_instance == null && _members == null && Fields != null)
				{
					_instance = Activator.CreateInstance(Type);
//					_instance.
					for (int i = 0; i < Fields.Count; i++)
					{
						string memberName = Fields.Keys.ElementAt(i);
						object memberValue = Fields.Values.ElementAt(i);
						MemberInfo[] members = Type.GetMember(memberName, MemberTypes.Field | MemberTypes.Property, MemberSetBinding);
						if (members == null || members.Length == 0)
							throw new MissingMemberException(Type.FullName, memberName);
						MemberInfo member = members[0];
						Type memberType = member.GetMemberReturnType();
						Type.InvokeMember(memberName, MemberSetBinding, null, _instance, new object[] {
							memberType.IsEnum ?
								Enum.Parse(memberType, (string)memberValue) :
								Convert.ChangeType(memberValue, memberType) });
					}
				}
				return _instance;
			}
			set {
				// Clear current type, member, and fields information, and rebuild using new instance
				if (_instance != value)
				{
					Fields.Clear();
					_instance = value;
					if (_type == null)
						_type = _instance == null ? typeof(object) : _instance.GetType();
					_members = _type.GetMembers(MemberGetBinding).Where(MemberSelector).ToArray();
					_values = _members.Select((member) => _type.InvokeMember(member.Name, MemberGetBinding, null, _instance, null)).ToArray();
					for (int i = 0; i < _members.Length; i++)
						Fields.Add(_members[i].Name, _values[i]);
					TypeName = _type.FullName;
					AssemblyName = _type.Assembly.FullName;
				}
			}
		}
		private object _instance;

		[BsonId, DataMember(IsRequired = true, Order = 1)]
		public ObjectId Id {
			get;
			protected set;
		}

		[BsonIgnore, IgnoreDataMember]
		public Artefact Artefact {
			get
			{
				return _artefact;
			}
			protected set
			{
				_artefact = value;
				if (_artefact != null)
					ArtefactId = _artefact.Id;
			}
		}
		private Artefact _artefact;
		
		[BsonRequired, DataMember(IsRequired = false, Order = 2)]
		public ObjectId ArtefactId {
			get;
			protected set;
		}

//		[BsonIgnore, IgnoreDataMember]
		[BsonRequired, DataMember(IsRequired = false, Order = 5)]
		public DateTime TimeCreated {
			get { return Id.CreationTime; }
			set
			{
				if (Id.CreationTime != value)
					throw new ArgumentOutOfRangeException("TimeCreated", value, "Does not match Id.CreationTime");
			}
		}

//		[BsonIgnore]
		[BsonRequired, DataMember(IsRequired = false, Order = 6)]
		public DateTime TimeModified {
			get; set;
		}

		[BsonIgnore, IgnoreDataMember]
		public Type Type {
			get { return _type ?? (_type = Assembly.Load(AssemblyName).GetType(TypeName, true)); }		// TODO: AssemblyName? Load assembly if type load fails??
			protected set { _type = value; }
		}
		private Type _type;
		
		[BsonRequired, DataMember(IsRequired = true, Order = 3)]
		public string TypeName {
			get; set;
		}
		
		[BsonRequired, DataMember(IsRequired = true, Order = 4)]
		public string AssemblyName {
			get; protected set;
		}

		[BsonIgnore, IgnoreDataMember]
		private MemberInfo[] _members;

		[BsonIgnore, IgnoreDataMember]
		private object[] _values;

		[BsonRequired, DataMember(IsRequired = true, Order = 9)]
		public Dictionary<string, object> Fields
		{
			get; set;
		}
		
		[DataMember(EmitDefaultValue = false, Order = 7)]
		[BsonIgnoreIfDefault, BsonDefaultValue(BindingFlags.Instance|BindingFlags.DeclaredOnly|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.GetProperty)]
		public BindingFlags MemberGetBinding {
			get; set;
		}
		
		[DataMember(EmitDefaultValue = false, Order = 8)]
		[BsonIgnoreIfDefault, BsonDefaultValue(BindingFlags.Instance|BindingFlags.DeclaredOnly|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.SetField|BindingFlags.SetProperty)]
		public BindingFlags MemberSetBinding {
			get; set;
		}
		
		[BsonIgnore, IgnoreDataMember]
		public Func<MemberInfo, bool> MemberSelector {
			get; protected set;
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Aspect"/> class.
		/// </summary>
		public Aspect()
		{
			Id = ObjectId.Empty;
			Fields = new Dictionary<string, object>(8);
			BindingFlags commonFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
			MemberGetBinding = commonFlags | BindingFlags.GetField | BindingFlags.GetProperty;
			MemberSetBinding = commonFlags | BindingFlags.SetField | BindingFlags.SetProperty;
			MemberSelector = DefaultMemberSelector;

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Aspect"/> class.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <param name="aspect">Aspect.</param>
		/// <param name="memberSelector">Member selector.</param>
		/// <param name="memberSelectBinding">Member select binding.</param>
		public Aspect(Artefact artefact, object aspect,
			Type type = null,
			Func<MemberInfo, bool> memberSelector = null,
			BindingFlags memberSelectBinding = BindingFlags.Default)
		: this()
		{
			Id = ObjectId.GenerateNewId();
			Fields = new Dictionary<string, object>(8);
			if (memberSelector != null)
				MemberSelector = memberSelector;
			if (memberSelectBinding != BindingFlags.Default)
			{
				BindingFlags commonFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
				MemberGetBinding = commonFlags | BindingFlags.GetField | BindingFlags.GetProperty;
				MemberSetBinding = commonFlags | BindingFlags.SetField | BindingFlags.SetProperty;
			}
			Artefact = artefact;	// if not null sets ArtefactId property
			Type = type;			// if null sets type to Instance.GetType() when Instance property is set
			Instance = aspect;
		}
		#endregion

		#region Methods

		public bool DefaultMemberSelector(MemberInfo member)
		{
			return (
				(member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property) &&
				!member.Name.StartsWith("get_") && !member.Name.StartsWith("set_") &&
				!member.Name.Equals("Item") && !member.Name.EndsWith(">k__BackingField") &&
						member.DeclaringType != typeof(Aspect) &&
						(member.GetCustomAttribute<BsonIgnoreAttribute>() == null) &&
						(member.GetCustomAttribute<BsonIgnoreAttribute>() == null));
		}

		/// <summary>
		/// Ases the bson document.
		/// </summary>
		/// <returns>The bson document.</returns>
		public BsonDocument AsBsonDocument()
		{
			return this.ToBsonDocument<Aspect>();
		}

		/// <summary>
		/// Determines whether the specified <see cref="Artefacts.Aspect"/> is equal to the current <see cref="Artefacts.Aspect"/>.
		/// </summary>
		/// <param name="other">The <see cref="Artefacts.Aspect"/> to compare with the current <see cref="Artefacts.Aspect"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="Artefacts.Aspect"/> is equal to the current
		/// <see cref="Artefacts.Aspect"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// IEquatable implementation
		/// Am I currently using this? What was it's intention? (exactly?) Should I consider similar on Artefact class?
		/// </remarks>
		public bool Equals(Aspect other)
		{
			return Id == other.Id || 
				(object.ReferenceEquals(_instance, other._instance) && Type.Equals(other.Type)) ||
				Fields.Equals(other.Fields);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Artefacts.Aspect"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Artefacts.Aspect"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(string.Format("[Aspect: {0}, Type=\"{1}\"", ArtefactId, TypeName));
			foreach (KeyValuePair<string, object> field in Fields)
				sb.AppendFormat(", {0}={1}", field.Key, field.Value);
			return sb.ToString();
		}
		#endregion
	}
	
	public static class Aspect_Extensions
	{
		/// <summary>
		/// As the specified aspect.
		/// </summary>
		/// <param name="aspect">Aspect.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T As<T>(this Aspect aspect)
		{
			if (aspect == null)
				return default(T);
			if (!typeof(T).IsAssignableFrom(aspect.Type))
				throw new InvalidCastException(string.Format("Could not assign aspect instance of type \"{0}\" to type \"{1}\"", aspect.Type, typeof(T)));
			return (T)aspect.Instance;
		}
	}
}

