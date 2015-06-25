using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ServiceStack.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections;
using System.Dynamic;
using System.Reflection;

namespace Artefacts
{
	/// <summary>
	/// Artefact.
	/// </summary>
	/// <remarks>
	/// 
	/// [DataContract]: ServiceStack will serialize all (virtual or non virtual) members if DataContractAttribute is
	/// NOT applied to the container class. If it is, will only serialize those with DataMemberAttribute.
	/// If one class inherits from another, the most derived class needs the [DataContract] attribute for the serialization
	/// engine to pay attention to [DataMember] attributes in the class, or it will just serialize everything
	/// [DataMember]: EmitDefaultValue is ignored, I think.
               	/// </remarks>
	[DataContract(IsReference = true, Namespace = "Artefacts")]
	public class Artefact : DynamicObject
	{	
		BsonDocument _bsonDocument = new BsonDocument();

		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Artefact));
		#endregion

		/// <summary>
		/// Artefact data collection, <see cref="List`1"/> of <see cref="Artefact.Aspect"/>s 
		/// </summary>
		[DataContract(IsReference = true, Namespace = "Artefacts")]
		public class AspectCollection : ICollection<Aspect>, IEnumerable<Aspect>
		{
			#region Properties & fields
			/// <summary>
			/// The unique <see cref="Aspect"/>s
			/// </summary>
			private List<Aspect> _uniqueAspects;
			
			/// <summary>
			/// The <see cref="Aspect"/>s
			/// </summary>
			private Dictionary<Type, Aspect> _aspects;
			
			/// <summary>
			/// The artefact these <see cref="Aspect"/>s are associated with
			/// </summary>
			public Artefact Artefact {
				get; private set;
			}

			/// <summary>
			/// Gets the <see cref="Artefacts.Artefact+AspectCollection"/> at the specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			public Aspect this[int index] {
				get
				{
					return _uniqueAspects[index];
				}
			}

			/// <summary>
			/// Gets the <see cref="Artefacts.Aspect"/> with the specified type.
			/// </summary>
			/// <param name="type">Type.</param>
			public Aspect this[Type type] {
				get { return _aspects[type]; }
			}
			#endregion
			
			#region Construction
			/// <summary>
			/// Initializes a new instance of the <see cref="Artefacts.Artefact+AspectCollection"/> class.
			/// </summary>
			public AspectCollection()
			{
				_uniqueAspects = new List<Aspect>(4);
				_aspects = new Dictionary<Type, Aspect>(4);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Artefacts.Artefact+DataCollection"/> class.
			/// </summary>
			/// <param name="aspects">Aspects.</param>
			public AspectCollection(Artefact artefact, params object[] aspects) : this()
			{
				Artefact = artefact;
				if (aspects != null)
					AddRange(aspects);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Artefacts.Artefact+DataCollection"/> class.
			/// </summary>
			/// <param name="aspects">Aspects.</param>
			public AspectCollection(Artefact artefact, IEnumerable<object> aspects = null) : this()
			{
				Artefact = artefact;
				if (aspects != null)
					AddRange(aspects);
			}
			#endregion
			
			#region Methods
			/// <summary>
			/// Adds the range of <paramref name="aspects"/>
			/// </summary>
			/// <param name="aspects"><see cref="Aspect"/>s to add</param>
			public void AddRange(IEnumerable<object> aspects)
			{
				foreach (object aspect in aspects)
				{
					if (aspect.GetType() == typeof(Aspect))
						Add((Aspect)aspect);
					else
						for (	Type T = aspect.GetType();
						     	T != null && T != typeof(System.Object) && T != typeof(ValueType);
						     	T = T.BaseType	)
							Add(new Aspect(Artefact, aspect, T));
				}
			}

			#region ICollection implementation
			/// <summary>
			/// Gets the number of <see cref="Aspect"/>s
			/// </summary>
			/// <value>The number of <see cref="Aspect"/>s</value>
			public int Count {
				get { return _uniqueAspects.Count; }
			}

			/// <summary>
			/// Gets a value indicating whether this instance is read only.
			/// </summary>
			/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
			public bool IsReadOnly {
				get { return false; }
			}

			/// <Docs>The aspect to add to the current collection.</Docs>
			/// <para>Adds an aspect to the current collection.</para>
			/// <remarks>To be added.</remarks>
			/// <exception cref="System.NotSupportedException">The current collection is read-only.</exception>
			/// <summary>
			/// Add the specified <see cref="Aspect"/> 
			/// </summary>
			/// <param name="aspect">The <see cref="Aspect"/> to add</param>
			public void Add(Aspect aspect)
			{
				if (aspect == null)
					throw new ArgumentNullException("aspect");
				if (_uniqueAspects.Contains(aspect))
					throw new ArgumentOutOfRangeException("aspect", aspect, string.Format(
						"Aspect instance {0} already exists for {1}", aspect, Artefact.URI));
				_uniqueAspects.Add(aspect);
				_aspects.Add(aspect.Type, aspect);
//				for (Type T = aspect.Type; T != typeof(System.Object) && T != typeof(System.ValueType); T = T.BaseType)
//					_aspects.Add(T, aspect);
			}

			/// <summary>
			/// Clears all <see cref="Aspect"/>s
			/// </summary>
			public void Clear()
			{
				_uniqueAspects.Clear();
				_aspects.Clear();	
			}

			/// <Docs>The <see cref="Aspect"/> to locate in the current collection.</Docs>
			/// <para>Determines whether the current collection contains a specific <see cref="Aspect"/>.</para>
			/// <summary>
			/// Contains the specified aspect.
			/// </summary>
			/// <param name="aspect"><see cref="Aspect"/></param>
			public bool Contains(Aspect aspect)
			{
				return _uniqueAspects.Contains(aspect);
			}

			/// <Docs>The object to locate in the current collection.</Docs>
			/// <para>Determines whether the current collection contains a specific value.</para>
			/// <summary>
			/// Contains the specified aspectInstance.
			/// </summary>
			/// <param name="aspectInstance">Aspect instance.</param>
			public bool Contains(object aspectInstance)
			{
				return _uniqueAspects.Any(aspect => aspect.Instance == aspectInstance);
			}
			
			/// <summary>
			/// Copies <see cref="Aspect"/>s to an array
			/// </summary>
			/// <param name="array">Array to copy to</param>
			/// <param name="arrayIndex">Array index that will receive first <see cref="Aspect"/></param>
			public void CopyTo(Aspect[] array, int arrayIndex)
			{
				_uniqueAspects.CopyTo(array, arrayIndex);
			}

			/// <Docs>The<see cref="Aspect"/> to remove from the current collection.</Docs>
			/// <para>Removes the first occurrence of an <see cref="Aspect"/> from the current collection.</para>
			/// <summary>
			/// Remove the specified <see cref="Aspect"/>
			/// </summary>
			/// <param name="aspect">The <see cref="Aspect"/> to remove</param>
			/// <returns>True if the <see cref="Aspect"/> was found and removed</returns>
			public bool Remove(Aspect aspect)
			{
				if (!_uniqueAspects.Contains(aspect))
					return false;
				for (Type T = aspect.GetType(); T != typeof(System.Object) && T != typeof(System.ValueType); T = T.BaseType)
					_aspects.Remove(T);
				return _uniqueAspects.Remove(aspect);
			}
			#endregion

			#region IEnumerable implementation
			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			/// <returns>The enumerator.</returns>
			public IEnumerator<Aspect> GetEnumerator()
			{
				return _uniqueAspects.GetEnumerator();
			}

			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			/// <returns>The enumerator.</returns>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return (IEnumerator)GetEnumerator();
			}
			#endregion
			#endregion
		}

		#region Fields & Properties
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		[BsonId, BsonElement("Id"), DataMember(IsRequired = true)]
		public ObjectId Id { get; set; }

		/// <summary>
		/// Gets or sets the URI
		/// </summary>
		[BsonRequired, DataMember]
		public Uri URI { get; set; }

		/// <summary>
		/// Gets or sets the host ID
		/// </summary>
		[BsonRequired, DataMember]
		public Host Host;

		/// <summary>
		/// Gets or sets the time created.
		/// </summary>
		[BsonRequired, DataMember]
		public DateTime TimeCreated {
			get { return Id.CreationTime; }
			set
			{
				if (Id.CreationTime != value)
					throw new ArgumentOutOfRangeException("TimeCreated", value, "Does not match Id.CreationTime");
			}
		}

		/// <summary>
		/// Gets or sets the time checked.
		/// </summary>
		[BsonRequired, DataMember]
		public DateTime TimeChecked { get; set; }

		/// <summary>
		/// Gets or sets the time updated.
		/// </summary>
		[BsonRequired, DataMember]
		public DateTime TimeModified { get; set; }

		/// <summary>
		/// The fields.
		/// </summary>
		[BsonRequired, DataMember]
		public readonly Dictionary<string, object> Fields;

		/// <summary>
		/// Aspects collection. See type <see cref="Artefacts.Artefact.AspectCollection"/> 
		/// </summary>
		[BsonRequired, DataMember]
		public AspectCollection Aspects { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Artefacts.Artefact"/> with the specified type.
		/// </summary>
		/// <param name="type">Type.</param>
		public Aspect this[Type type] {
			get { return Aspects == null ? null : Aspects[type]; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Artefact"/> class.
		/// </summary>
		/// <remarks>
		/// For deserialization
		///  - So I think but other classes might have had trouble with anything less than public default c'tor??
		/// 	Try commenting this out see what happens
		/// </remarks>
		private Artefact()
		{
			Fields = new Dictionary<string, object>();
			Aspects = new AspectCollection(this);
			Log.Debug(this);
		}

		public Artefact(object value)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if (!value.GetType().IsClass)
				throw new ArgumentOutOfRangeException("value", "Not a class type");
			BindingFlags bf = BindingFlags.Public | BindingFlags.Instance
				| BindingFlags.GetField | BindingFlags.GetProperty;
			foreach (MemberInfo member in value.GetType().GetMembers(bf))
			{
				// This isn't the right way to do it because if value is another
				// class instance it should create another Artefact
//				Fields[member.Name] = member.GetPropertyOrField(value);
				// this should work automatically though?
				_bsonDocument[member.Name] = BsonValue.Create(value);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Artefact"/> class.
		/// </summary>
		/// <param name="uri">Artefact URI</param>
		/// <param name="aspects"></param>
		public Artefact(Uri uri, params object[] aspects)
		{
			Id = ObjectId.GenerateNewId();
			TimeChecked = TimeModified = TimeCreated;
			URI = uri;
			Host = Host.Current;
			Fields = new Dictionary<string, object>();
			Aspects = new AspectCollection(this, aspects);
			Log.Debug(this);
		}
		#endregion

		#region Methods

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _bsonDocument.Names;//`.Keys.AsEnumerable();
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
//			result = null;
//			if (!Fields.ContainsKey(binder.Name))
//				return false;
//			result = Fields[binder.Name];
			result = BsonTypeMapper.MapToDotNetValue(_bsonDocument[binder.Name]);
			return true;
		}

		/// <summary>
		/// Tries the set member.
		/// </summary>
		/// <returns><c>true</c>, if set member was tryed, <c>false</c> otherwise.</returns>
		/// <param name="binder">Binder.</param>
		/// <param name="value">Value.</param>
		/// <remarks>
		/// Would this be the right spot to track/create (am i creating?) an object graph
		/// ie you would (if value is a non-primitive class type) create a new Artefact
		/// in this method 
		/// </remarks>
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
//			Fields[binder.Name] = value != null && value.GetType().IsClass
//				? new Artefact(value) : value;
			_bsonDocument[binder.Name] = BsonValue.Create(value);
			return true;
		}

		/// <summary>
		/// Tries the create instance.
		/// </summary>
		/// <returns><c>true</c>, if create instance was tryed, <c>false</c> otherwise.</returns>
		/// <param name="binder">Binder.</param>
		/// <param name="args">Arguments.</param>
		/// <param name="result">Result.</param>
		/// <remarks>
		/// Is this where I could put a call to storage->add(object) instead of storage->update/save()
		/// because this method called when a new Artefact is created?
		/// 		ie a = new Artefact(typed value client side)
		/// </remarks>
		public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
		{
			return base.TryCreateInstance(binder, args, out result);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Artefacts.Artefact"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Artefacts.Artefact"/>.</returns>
		public override string ToString()
		{
			return string.Format("[Artefact: {0}: {1} aspects{2}]", URI, Aspects == null ? "Aspects == null!" : Aspects.Count.ToString(),
				Aspects == null || Aspects.Count == 0 ? string.Empty : string.Concat(", primary type \"", Aspects.First().TypeName, "\""));
		}
		#endregion
	}
}

