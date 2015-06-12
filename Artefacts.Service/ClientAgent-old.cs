using System;
using System.Diagnostics;
using ServiceStack.Logging;
using System.IO;
using System.Runtime.Serialization;

namespace Artefacts
{
	/// <summary>
	/// Client agent abstract base class
	/// </summary>
	[DataContract]
	public abstract class ClientAgent
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(Artefact)) { SourceClass = typeof(ClientAgent) };

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
			getDiskSerialProcess.WaitForExit(1111);
			hostId = getDiskSerialProcess.StandardOutput.ReadLine();
			if (string.IsNullOrEmpty(hostId))
				throw new InvalidDataException("Unexpected output data from hostid command");
			return hostId;
		}
		#endregion

		#region Fields & properties
		/// <summary>
		/// The created.
		/// </summary>
		public DateTime Created { get; private set; }

		/// <summary>
		/// The started.
		/// </summary>
		public DateTime Started { get; private set; }

		/// <summary>
		/// The finished.
		/// </summary>
		public DateTime Finished { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		/// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
		public bool IsRunning {
			get { return Started != default(DateTime) && Finished == default(DateTime); }
		}

		/// <summary>
		/// Gets the duration.
		/// </summary>
		public TimeSpan Duration {
			get
			{
				return Started == default(DateTime) ? TimeSpan.Zero
				: (Finished != default(DateTime) ? Finished : DateTime.Now) - Started;
			}
		}

		/// <summary>
		/// Gets the client.
		/// </summary>
		public Host ClientHost { get; private set; }

		/// <summary>
		/// Gets the client.
		/// </summary>
		/// <value>The client.</value>
		public IArtefactsRepository Client { get; private set; }
		#endregion

		#region Methods
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.ClientAgent"/> class.
		/// </summary>
		public ClientAgent(IArtefactsRepository client)
		{
			if (client == null)
				throw new ArgumentNullException("repository");
			Created = DateTime.Now;
			Started = Finished = default(DateTime);
//			ClientHost = Client.Get<Host>(string.Empty);
			Client = client;
		}

		/// <summary>
		/// Marks the start.
		/// </summary>
		protected void MarkStart()
		{
			Started = DateTime.Now;
		}

		/// <summary>
		/// Marks the finish.
		/// </summary>
		protected void MarkFinish()
		{
			Finished = DateTime.Now;
		}

		/// <summary>
		/// Run this instance.
		/// </summary>
		public abstract void Run(object param);
		#endregion
	}
}

