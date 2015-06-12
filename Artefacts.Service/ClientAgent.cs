using System;
using ServiceStack.Logging;

namespace Artefacts
{
	/// <summary>
	/// Client agent abstract base class
	/// </summary>
	public abstract class ClientAgent
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(ClientAgent));
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
		public IDataStore Client { get; private set; }
		#endregion

		#region Methods
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.ClientAgent"/> class.
		/// </summary>
		protected ClientAgent(IDataStore client)
		{
			if (client == null)
				throw new ArgumentNullException("client");
			Created = DateTime.Now;
			Started = Finished = default(DateTime);
			Client = client;
		}

		/// <summary>
		/// Marks the start.
		/// </summary>
		protected void MarkStart()
		{
			Started = DateTime.Now;
			Log.DebugVariable("Started", Started);
		}

		/// <summary>
		/// Marks the finish.
		/// </summary>
		protected void MarkFinish()
		{
			Finished = DateTime.Now;
			Log.DebugVariable("Finished", Finished);
		}

		/// <summary>
		/// Mark start and finish times either side of calling <see cref="ClientAgent.Run"/>
		/// </summary>
		/// <param name="param">Arbitrary parameter.</param>
		public void Execute(object param = null)
		{
			MarkStart();
			Run(param);
			MarkFinish();
			Log.DebugVariable("Duration", Duration);
		}

		/// <summary>
		/// Run this instance.
		/// </summary>
		public abstract void Run(object param);
		#endregion
	}
}

