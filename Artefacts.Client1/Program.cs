using System;
using ServiceStack;
using ServiceStack.Text.Json;
using ServiceStack.Web;
using NUnit.Core;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using ServiceStack.Logging;
using NUnit.Framework;

namespace Artefacts
{
	/// <summary>
	/// Main class.
	/// </summary>
	/// <remarks>
	/// 
	/// Current TODO Plan
	/// 
	/// 	- Implement a basic but reasonably representative Artefact DTOs
	/// 		- Confirm ability to send by request and receive by response
	/// 		- Try/test async variation
	/// 	- Ditto above for Queryable DTOs
	/// 		- Approach for async queryable[/DTOs]?
	/// 			- Could load result count (sync or async?) intially
	/// 			  and then launch async/task threads to retrieve:
	/// 				- Result artefact IDs and/or URLs
	/// 					- Basically the 'compact' data describing results of query
	/// 				- Separate threads to retrieve artefact data?
	/// 					- individually or in pages?
	/// 					- Perhaps not, only fetch on demand? (lazy?)
	/// 				
	/// </remarks>
	class MainClass
	{
		#region Static members
		/// <summary>
		/// The log.
		/// </summary>
		public static readonly IDebugLog Log = new DebugLog<ConsoleLogger>(typeof(MainClass));
		#endregion

		class TestListener : EventListener, ITestFilter
		{
			#region ITestFilter implementation
			public bool Pass(ITest test)
			{
				return true;
			}

			public bool Match(ITest test)
			{
				return true;
			}

			public bool IsEmpty {
				get {
					return false;
				}
			}
			#endregion

			#region EventListener implementation
			public void RunStarted(string name, int testCount)
			{
				Log.Info("####### Run Start: " + name + " (" + testCount + ") #######");
			}
			public void RunFinished(TestResult result)
			{
				Log.Info("####### Run Finish: " + result.Name + " = " + result + "#######");
			}
			public void RunFinished(Exception exception)
			{
				Log.Error("!!##### Run Finish: " + exception + "#######");
			}
			public void TestStarted(TestName testName)
			{
				Log.Info("------- Test Start: " + testName + " -------");
			}
			public void TestFinished(TestResult result)
			{
				Log.Info("------- Test Finish: " + result.Name + " = " + result + " -------");
			}
			public void SuiteStarted(TestName testName)
			{
				Log.Info("******* Suite Start: " + testName + " *******");
			}
			public void SuiteFinished(TestResult result)
			{
				Log.Info("******* Suite Finish: " + result.Name + " = " + result + " *******");
			}
			public void UnhandledException(Exception exception)
			{
				Log.Error("-xx-xx- Unhandled: " + exception + "#######");
			}
			public void TestOutput(TestOutput testOutput)
			{
				Log.Info(testOutput);
			}
			#endregion
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			Console.ReadLine();
			DebugLog<ConsoleLogger>.TrimSourceRootPath = "/home/jk/Code/Projects/NET/Artefacts/Artefacts.";
//			DebugLog<ConsoleLogger>.TrimSourceClassName = "Artefacts";

//			TestListener testListener = new TestListener();
//			TestFixture testFixture = new TestFixture(typeof(NUnitTestClass));
//			TestResult testResult;

			string testName = string.Empty;
			try
			{
				NUnitTestClass nuTC = new NUnitTestClass();
				nuTC.NUnitTestClassSetUp();
				foreach (MethodInfo mi in typeof(NUnitTestClass).GetMethods().Where((mi) => mi.AllAttributes<TestAttribute>().Length > 0))
				{
					testName = mi.Name;
					Log.Info("----- Start Test: " + mi.Name + (string.IsNullOrEmpty(mi.GetDescription()) ? "" : ": " + mi.GetDescription()));
					mi.Invoke(nuTC, new object[] {} );
					Log.Info("----- Finish Test: " + mi.Name + ": Success");
//					testResult = testFixture.Run(testListener, testListener);
				}
			}
			catch (WebServiceException e) { Log.Error("-!!-- Finish Test: " + testName + ": Failure:\n" + e.Format()); }
			catch (TargetInvocationException e) { Log.Error("-!!-- Finish Test: " + testName + ": Failure:\n" + e.Format()); }
			catch (Exception e) { Log.Error("-!!-- Finish Test: " + testName + ": Failure:\n" + e.Format()); }
		}


	}
}
