using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using ServiceStack.Logging;
using MongoDB.Bson.Serialization.Attributes;

namespace Artefacts
{
	/// <summary>
	/// Host.
               	/// </summary>
	[DataContract(IsReference = true, Namespace = "")]
	public class Host
	{
		#region Static members
		/// <summary>
		/// Gets the log.
		/// </summary>
		public static IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Artefact)) { SourceClass = typeof(Host) };

		/// <summary>
		/// Gets the current.
		/// </summary>
		public static Host Current {
			get
			{
				if (_current == null)
					_current = new Host(GetHostId());
				return _current;
			}
		}
		private static Host _current = null;

		/// <summary>
		/// Gets the host identifier.
		/// </summary>
		/// <returns>The host identifier.</returns>
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
			if (!getDiskSerialProcess.WaitForExit(1111))
				throw new TimeoutException("Timeout waiting for hostid command to return");
			hostId = getDiskSerialProcess.StandardOutput.ReadLine();
			if (string.IsNullOrEmpty(hostId))
				throw new InvalidDataException("Unexpected output data from hostid command");
			return hostId;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the host identifier.
		/// </summary>
		[BsonRequired]
		[DataMember]
		public string HostId { get; set; }

		/// <summary>
		/// Gets or sets the machine.
		/// </summary>
		/// <value>The machine.</value>
		[BsonRequired]
		[DataMember]
		public string Machine { get; set; }

		/// <summary>
		/// Gets or sets the domain.
		/// </summary>
		/// <value>The domain.</value>
		[BsonIgnoreIfDefault]
		[DataMember]
		public string Domain { get; set; }

		/// <summary>
		/// Gets or sets the connection identifier.
		/// </summary>
		/// <value>The connection identifier.</value>
		[BsonIgnore]
		public int ConnectionId { get; set; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="Artefacts.Host"/> is connected.
		/// </summary>
		/// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
		[BsonIgnore]
		public bool Connected { get { return ConnectionId != default(int); } }

		/// <summary>
		/// Gets or sets the connect time.
		/// </summary>
		/// <value>The connect time.</value>
		public DateTime ConnectTime { get; set; }

		/// <summary>
		/// Gets the connection age.
		/// </summary>
		[BsonIgnore]
		public TimeSpan ConnectionAge {
			get
			{
				return ConnectTime == DateTime.MinValue ?
					TimeSpan.Zero : DateTime.Now - ConnectTime;
			}
		}
		#endregion

		#region Methods
		#region Construction
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Host"/> class.
		/// </summary>
		public Host()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Host"/> class.
		/// </summary>
		/// <param name="host">Host.</param>
		public Host(string hostId)
		{
			if (string.IsNullOrWhiteSpace(hostId))
				throw new ArgumentOutOfRangeException("host");
			ConnectionId = -1;
			ConnectTime = DateTime.MinValue;
			HostId = hostId;
			Domain = "domain";
		}
		#endregion

		#region System.Object overrides
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Artefacts.Host"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Artefacts.Host"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="Artefacts.Host"/>;
		/// otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if (System.Object.ReferenceEquals(this, obj))
				return true;
			if (obj == null || obj.GetType() != this.GetType())
				return false;
			return HostId == ((Host)obj).HostId;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="Artefacts.Host"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode()
		{
			return HostId.GetHashCode();
		}
		#endregion
		#endregion
	}
}