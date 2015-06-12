using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Artefacts.FileSystem
{
	/// <summary>
	/// File.
	/// </summary>
	[DataContract(IsReference = true, Namespace = "Artefacts")]
	public class File : FileSystemEntry
	{
		#region Public fields & properties
		/// <summary>
		/// The size.
		/// </summary>
		[BsonRequired, DataMember(IsRequired = true)]
		public long Size { get; private set; }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		[BsonIgnore, IgnoreDataMember]
		public string Name {
			get { return System.IO.Path.GetFileName(Path); }
		}

		/// <summary>
		/// Gets the name without extension.
		/// </summary>
		[BsonIgnore, IgnoreDataMember]
		public virtual string NameWithoutExtension {
			get { return System.IO.Path.GetFileNameWithoutExtension(Path); }
		}

		/// <summary>
		/// Gets the extension.
		/// </summary>
		[BsonIgnore, IgnoreDataMember]
		public virtual string Extension {
			get { return System.IO.Path.GetExtension(Path); }
		}

		/// <summary>
		/// Gets or sets the file info.
		/// </summary>
		[BsonIgnore, IgnoreDataMember]
		public virtual FileInfo FileInfo {
			get
			{
				return (FileInfo)base.Info;
			}
			set
			{
				base.SetInfo(value);
				Size = value.Length;
			}
		}
		#endregion

		#region Methods
		#region Construction
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.File"/> class.
		/// </summary>
		public File()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.File"/> class.
		/// </summary>
		/// <param name="path">Path.</param>
		public File(string path)
		{
			FileInfo = new FileInfo(path);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.File"/> class.
		/// </summary>
		/// <param name="fileInfo">File info.</param>
		protected File (FileInfo fileInfo)
		{
			FileInfo = fileInfo;
		}
		#endregion

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Artefacts.FileSystem.FileSystemEntry"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Artefacts.FileSystem.FileSystemEntry"/>.</returns>
		public override string ToString()
		{
			return this.FormatString();
		}

		/// <summary>
		/// Inners the equals.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="obj">Object.</param>
		protected override bool InnerEquals(object obj)
		{
			return typeof(File).IsAssignableFrom(obj.GetType())
				&& Size == ((File)obj).Size  && base.InnerEquals(obj);
		}
		#endregion
	}
}
