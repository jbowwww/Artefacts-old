using System;
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

namespace Artefacts
{
	public class Aspect : DynamicObject, IDictionary<string, object>, ISerializable
	{
		/// <summary>
		/// Gets or sets the artefact identifier.
		/// </summary>
		/// <value>The artefact identifier.</value>
		[BsonRequired]
		public ObjectId ArtefactId { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		[BsonRequired]
		public object Value {
			get { return _value; }
			set
			{
				bool isNewValue = false;
				if (value != _value)
					isNewValue = true;
				_value = value;
				Type = _value != null ? _value.GetType() : typeof(System.Object);
				if (isNewValue)
					;
			}
		}
		[BsonIgnore]
		private object _value;

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		[BsonRequired]
		public Type Type {
			get { return _type; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				if (value != _type)
					IsTypeMemberSelected = false;
				_type = value;
			}
		}
		[BsonIgnore]
		private Type _type;

		#region IDictionary property implementations
		/// <summary>
		/// Gets the keys.
		/// </summary>
		 ICollection<string> IDictionary<string, object>.Keys {
			get { return MemberNames; }
		}

		/// <summary>
		/// Gets the values.
		/// </summary>
		public ICollection<object> IDictionary<string, object>.Values {
			get { return MemberValues; }
		}

		/// <summary>
		/// Gets or sets the <see cref="Artefacts.Aspect"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public object this[string index] {
			get
			{
				return MemberValues[index];
			}
			set {
				MemberValues[index] = value;
			}
		}
		#endregion

		#region ICollection implementation
		public void Add(KeyValuePair<string, object> item)
		{
			throw NotImplementedException();
		}
		public void Clear()
		{
			throw new NotImplementedException();
		}
		public bool Contains(KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
		public bool Remove(KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}
		public int Count {
			get {
				return MemberNames.Length;
			}
		}
		public bool IsReadOnly {
			get {
				return true;
			}
		}
		#endregion
		#region IEnumerable implementation
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion
		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		[BsonRequired]
		public object[] MemberValues
		{
			get
			{
				if (_memberValues != null)
					return _memberValues;
				_memberValues = FormatterServices.GetObjectData(_value, Members);
				IsTypeMemberSelected = true;
				return _memberValues;
			}
			private set
			{
				_memberValues = value;
			}
		}
		[BsonIgnore]
		private object[] _memberValues;

		/// <summary>
		/// Gets the members.
		/// </summary>
		[BsonIgnore]
		public MemberInfo[] Members {
			get
			{
				if (IsTypeMemberSelected)
					return _members;
				_members = Type.GetMembers().Where((mi) => MemberSelector(mi));
				IsTypeMemberSelected = true;
				return _members;
			}
		}
		[BsonIgnore]
		private MemberInfo[] _members;

		/// <summary>
		/// Gets the member names.
		/// </summary>
		[BsonIgnore]
		public string[] MemberNames {
			get { return Members.Select((memberInfo) => memberInfo.Name); }
		}

		/// <summary>
		/// The member selector.
		/// </summary>
		[BsonIgnore]
		Func<MemberInfo, bool> MemberSelector {
			get { return _memberSelector ?? DefaultMemberSelector; }
			set
			{
				if (value != _memberSelector)
					IsTypeMemberSelected = false;
				_memberSelector = value;
			}
		}
		[BsonIgnore]
		private Func<MemberInfo, bool> _memberSelector;

		/// <summary>
		/// Gets a value indicating whether this instance is type member selected.
		/// </summary>
		/// <value><c>true</c> if this instance is type member selected; otherwise, <c>false</c>.</value>
		[BsonIgnore]
		public bool IsTypeMemberSelected { get; private set; }

		/// <summary>
		/// Defaults the member selector.
		/// </summary>
		/// <returns><c>true</c>, if member selector was defaulted, <c>false</c> otherwise.</returns>
		/// <param name="MemberInfo">Member info.</param>
		private static bool DefaultMemberSelector(MemberInfo MemberInfo)
		{
			return true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Aspect"/> class.
		/// </summary>
		/// <param name="artefactId">Artefact identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="T">T.</param>
		/// <param name="memberSelector">Member selector.</param>
		/// <remarks>DynamicObject override</remarks>
		public Aspect(ObjectId artefactId, object value = null, Type type = null, Func<MemberInfo, bool> memberSelector = null)
		{
			ArtefactId = artefactId;
			_value = value;
			_type = type ?? value != null ? _value.GetType() : typeof(System.Object);
			_memberSelector = memberSelector;
			IsTypeMemberSelected = false;
		}

		#region IDictionary method implementations
		/// <summary>
		/// Add the specified key and value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void Add(string key, object value)
		{
			throw new NotImplementedException();
		}

		/// <Docs>The key to locate in the current instance.</Docs>
		/// <para>Determines whether the current instance contains an entry with the specified key.</para>
		/// <summary>
		/// Containses the key.
		/// </summary>
		/// <returns><c>true</c>, if key was containsed, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool ContainsKey(string key)
		{
			throw new NotImplementedException();
		}
		public bool Remove(string key)
		{
			throw new NotImplementedException();
		}
		public bool TryGetValue(string key, out object value)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region DynamicObject overrides
		/// <summary>
		/// Gets the dynamic member names.
		/// </summary>
		/// <returns>The dynamic member names.</returns>
		/// <remarks>DynamicObject override</remarks>
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return MemberNames;
		}

		/// <summary>
		/// Tries the get member.
		/// </summary>
		/// <returns><c>true</c>, if get member was tryed, <c>false</c> otherwise.</returns>
		/// <param name="binder">Binder.</param>
		/// <param name="result">Result.</param>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			for (int index = 0; index < MemberNames.Length; index++)
			{
				if (MemberNames[index] == binder.Name)
				{
					BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					MemberValues[index] = _type.InvokeMember(binder.Name, bf, null, _value, null);
					result = MemberValues[index];
					return true;
				}
			}
			return false;
		}
		#endregion

		#region ISerializable implementation
		/// <Docs>To be added: an object of type 'SerializationInfo'</Docs>
		/// <summary>
		/// To be added
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		/// <remarks>ISerializable implementation</remarks>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(Type);
			info.AddValue("ArtefactId", ArtefactId);

			MemberInfo[] mi = FormatterServices.GetSerializableMembers(Type, context);
			object[] values = FormatterServices.GetObjectData(Value, mi);
			for (int i = 0; i < mi.Length; i++)
				info.AddValue(mi[i].Name, values[i]);
		}
		#endregion
	}
}

