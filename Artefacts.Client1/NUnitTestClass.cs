using System.Reflection;
using NUnit.Framework;
using System;
using System.Diagnostics;
using ServiceStack;
using System.Linq;
using Artefacts.FileSystem;
using System.Collections.Generic;
using NUnit.Core;
using System.Web.UI.WebControls;
using NUnit.Core.Builders;
using ServiceStack.Logging;
using System.Text;

namespace Artefacts
{
	/// <summary>
	/// N unit test class.
	/// </summary>
	[TestFixture(Category = "Client, Service, Basic", Description = "Test basic functionality of server and client proxy")]
	public class NUnitTestClass
	{
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(NUnitTestClass));

		/// <summary>
		/// The proxy.
		/// </summary>
		public DataStoreProxy Proxy;

		/// <summary>
		/// The FS agent.
		/// </summary>
		public FileSystemAgent FSAgent;

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.NUnitTestClass"/> class.
		/// </summary>
		public NUnitTestClass()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.NUnitTestClass"/> class.
		/// </summary>
		[TestFixtureSetUp]
		public void NUnitTestClassSetUp()
		{
			Proxy = new DataStoreProxy("http://localhost:8888/Artefacts/");
			if (Proxy == null)
				throw new ApplicationException("NUnitTestClass.Proxy == null");
			FSAgent = new FileSystemAgent(
				Proxy,
				new UriBuilder(
					Uri.UriSchemeFile,
					"localhost", 0,
					System.IO.Path.Combine(
						Environment.CurrentDirectory,
						"./testdata/"),
					"")
				.Uri);
			if (FSAgent == null)
				throw new ApplicationException("NUnitTestClass.FSAgent == null");
		}

		/// <summary>
		/// Tests the artefacts.
		/// </summary>
//		[Test(Description = "Proxy.Artefacts Count property and enumeration")]
		public void TestArtefacts()
		{
			Log.InfoVariable("Proxy.Artefacts", Proxy.Artefacts);
			Log.InfoVariable("Proxy.Artefacts.Count()", Proxy.Artefacts.Count());
			foreach (Artefact artefact in Proxy.Artefacts)
				Log.InfoVariable("Proxy.Artefacts[]", artefact);

			Queryable<Artefact> query = (Queryable<Artefact>)(from artefact in Proxy.Artefacts select artefact);
			Log.InfoVariable("query", query);
			Log.InfoVariable("query.Count()", query.Count());
			Log.InfoVariable("query.Count", query.Count);
			foreach (Artefact artefact in Proxy.Artefacts)
				Log.InfoVariable("query.Item[]", artefact);
		}
		
		/// <summary>
		/// Tests the FS agent.
		/// </summary>
		[Test(Description = "FileSystemAgent")]
		public void TestFSAgent()
		{
			FSAgent.Run(null);
		}
	}
}

