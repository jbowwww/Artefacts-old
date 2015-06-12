using NUnit.Framework;
using System;
using System.Diagnostics;
using ServiceStack;
using System.Linq;
using Artefacts.FileSystem;

namespace Artefacts
{

//	[TestFixture()]
	public class NUnitTestClass
	{

		private ArtefactsClient client;

		public NUnitTestClass()
		{
			client = new ArtefactsClient("http://localhost:8888/Artefacts/");
//			client.
		}

//		[Test()]
		public void Test_GetUsingQueryRequest()
		{
			QueryableRequest request = new QueryableRequest(System.Linq.Expressions.Expression.Parameter(typeof(IQueryable<Artefact>), "root"));
			QueryableResponse response = client.Get<QueryableResponse>(request);
			ConsoleWriteRequest(request, response);
		}

//		[Test()]
		public void Test_SendUsingQueryRequest()
		{
			QueryableRequest request = new QueryableRequest(System.Linq.Expressions.Expression.Parameter(typeof(IQueryable<Artefact>), "root"));
			QueryableResponse response = client.Get<QueryableResponse>(request);
			ConsoleWriteRequest(request, response);
		}

//		[Test()]
//		public void Test_AddArtefactAspect()
//		{
//			FileDescription fd = new FileDescription() { Path = "/home/jk/test.txt" };
//			ArtefactAddAspectRequest request = new ArtefactAddAspectRequest(fd);
//			ArtefactAddAspectResponse response = client.Post(request);
//			ConsoleWriteRequest(request, response);
//		}

//		[Test()]
//		public void Test_AddArtefact()
//		{
//			FileDescription fd = new FileDescription() { Path = "/home/jk/test.txt" };
//			Artefact artefact = new Artefact();
//			artefact.Data.Add(fd);
//			ArtefactAddRequest request = new ArtefactAddRequest(artefact);
//			ArtefactAddResponse response = client.Post<ArtefactAddResponse>(request);
////			ConsoleWriteRequest(request, response);
//		}

//		[Test()]
//		public void Test_AddFS()
//		{
//		Artefacts.FileSystem.File f = new Artefacts.FileSystem.File("/home/jk/test.txt");
//			Artefact artefact = new Artefact();
//			artefact.Add(f);
//			ArtefactAddRequest request = new ArtefactAddRequest(artefact);
//			ArtefactAddResponse response = client.Post<ArtefactAddResponse>(request);
//			ConsoleWriteRequest(request, response);
//		}

		[Test]
		public void Test_FSClient()
		{
			FileSystemAgent agent = new FileSystemAgent(new ArtefactsRepositoryProxy("http://localhost:8888/Artefacts/"));
			agent.Run(null);
		}

		private void ConsoleWriteRequest(object request, object response)
		{
			Console.WriteLine(DateTime.Now.ToShortDateString() + " " + request.GetType().FullName + ": " + new StackFrame().ToString());
			Console.WriteLine(DateTime.Now.ToShortDateString() + " " + request.GetType().FullName + ": requesting with " + request.ToString());
			Console.WriteLine(DateTime.Now.ToShortDateString() + " " + response.GetType().FullName + ": " + response.ToString());
		}
	}
}

