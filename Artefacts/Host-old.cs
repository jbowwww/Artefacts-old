using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using ServiceStack.Logging;

namespace Artefacts
{
	public class Host
	{
		public static IDebugLog Log { get {  return Artefact.Log; } }

		#region Static members
		private static Host _current = null;
		public static Host Current {
			get
			{
				if (_current == null)
					_current = new Host(GetHostId());
				return _current;
			}
		}
		
		public static string GetHostId()
		{
			string hostId;
			Process getDiskSerialProcess = Process.Start(
				new ProcessStartInfo("hostid")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false
				});
			getDiskSerialProcess.WaitForExit(1111);
			hostId = getDiskSerialProcess.StandardOutput.ReadLine();
			if (string.IsNullOrEmpty(hostId))
				throw new InvalidDataException("Unexpected output data from hostid command");
			return hostId;
		}
		#endregion

		public virtual string HostId { get; private set; }

		public string Machine { get; private set; }

		public string Domain { get; private set; }

		public virtual int ConnectionId { get; set; }

		public virtual bool Connected { get { return ConnectionId != default(int); } }
		
		public virtual DateTime ConnectTime { get; set; }
		
		public virtual TimeSpan ConnectionAge { get { return ConnectTime == DateTime.MinValue ? TimeSpan.Zero : DateTime.Now - ConnectTime; } }
		
		public Host(string host)
		{
			if (string.IsNullOrWhiteSpace(host))
				throw new ArgumentOutOfRangeException("host");
			ConnectionId = -1;
			ConnectTime = DateTime.MinValue;
			HostId = host;
			string[] hostComponents = host.Split(new char[] { '.' }, 2);
			Machine = hostComponents[0];
			if (hostComponents.Length > 1)
				Domain = hostComponents[0];
			Log.DebugVariable(this);
		}
		
		public override bool Equals(object obj)
		{
			if (System.Object.ReferenceEquals(this, obj))
				return true;
			if (obj == null || obj.GetType() != this.GetType())
				return false;
			return HostId == ((Host)obj).HostId;
		}

		public override int GetHashCode()
		{
			return Convert.ToInt32(HostId);
		}

		public override string ToString()
		{
			return string.Format("[Host: HostId={0}, Machine={1}, Domain={2}]", HostId, Machine, Domain);
		}
	}
}


//		public override Artefact Update()
//		{
//			if (UpdateAge > Artefact.UpdateAgeLimit)
//			{
//				if (GetHostId().CompareTo(HostId) != 0)
//					throw new ApplicationException(string.Format("HostId has somehow changed!! From {0} to {1}", HostId, GetHostId()));
//				return base.Update();
//			}
//			return this;
//		}		
//
//		public override void CopyMembersFrom(Artefact source)
//		{
//			base.CopyMembersFrom(source);
//			HostId = ((Host)source).HostId;
//		}		
