using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Linq;

namespace Artefacts.FileSystem
{
	public class FileSystemEntry
	{
		public Drive Drive;
		public string Path;	// the property however confirm this, check if maybe NH uses a c'tor with SerializationInfo & context params
		public FileAttributes Attributes;
		public virtual DateTime CreationTime { get; set; }
		public virtual DateTime LastAccessTime { get; set; }
		public virtual DateTime LastWriteTime { get; set; }
		public virtual Directory Directory { get; set; }
		
		public virtual System.IO.FileSystemInfo FileSystemInfo {
			get { return _fileSystemInfo; }
			set
			{
				if (_fileSystemInfo != null && !_fileSystemInfo.FullName.Equals(value.FullName))
					throw new InvalidOperationException("Cannot set FileSystemEntry.FileSystemInfo to an instance with a different Path value due to hash code requirements");
				_fileSystemInfo = value;
				Path = _fileSystemInfo.FullName;
				Attributes = _fileSystemInfo.Attributes;
				CreationTime = _fileSystemInfo.CreationTime;
				LastAccessTime = _fileSystemInfo.LastAccessTime;
				LastWriteTime = _fileSystemInfo.LastWriteTime;
				if (FileSystemAgent.Singleton != null)
				{
					if (FileSystemAgent.Singleton.Drives != null)
						Drive = FileSystemAgent.Singleton.Drives.FromPath(Path);
					if (FileSystemAgent.Singleton.Directories != null)
						Directory = (Directory)FileSystemAgent.Singleton.Directories.FromPath(Path);
				}
			}
		}
		private System.IO.FileSystemInfo _fileSystemInfo;
		
		protected FileSystemEntry(System.IO.FileSystemInfo fileSystemInfo)
		{
			FileSystemInfo = fileSystemInfo;
		}
		
		protected FileSystemEntry() {}		
		
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;
			FileSystemEntry fse = (FileSystemEntry)obj;
			return /*Drive == fse.Drive && */ Path.Equals(fse.Path);
		}

		public override int GetHashCode()
		{
			return string.Concat(GetType().FullName, ":", Path).GetHashCode();			// Convert.ToInt32(Path);
		}			/*  _hashCode.HasValue ? _hashCode.Value : (_hashCode = .... ).Value */

		public override string ToString()
		{
			return string.Concat(string.Format(string.Concat(
				"[FileSystemEntry: Drive=#{0} Path={1} Directory=#{2} Attributes={3} CreationTime={4} AccessTime={5} ModifyTime={6}]\n"),
				Drive != null ? Drive.Id.HasValue ? Drive.Id.Value.ToString() : "int?" : "(null)", Path,
				Directory != null ? Directory.Id.ToString() : "(null)", Attributes, CreationTime, LastAccessTime, LastWriteTime), base.ToString().Indent());
		}
	}
}

//		public override string ToString()
//		{
//			int r = 0;
//			return ToString(ref r);
//		}

//		public virtual string ToString(ref int subClassLevel)
//		{
			
//			Type T = GetType();
			// = "[" + subClassLevel > 0 ? base.ToString(subClassLevel + 1);
			
//			string r = base.ToString(ref subClassLevel);
//			subClassLevel++;
//			for (int i = 0; i < subClassLevel; i++)
//				r += "  ";
//			r += string.Format("[" + GetType().Name + ": Drive={0} Path=\"{1}\" ModifyTime={2} ... ",
//					Drive.Id, Path, ModifyTime.ToShortTimeString());
//			//= base.ToString(10 - subClassLevel) + GetType().Name;
//			
////			r += (string)(subClassLevel > 0 ? "" : 
//			r += "]\n";
//			subClassLevel = subClassLevel + 1;
//			
//			return r;
//		}
//	}
//}

