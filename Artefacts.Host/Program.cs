using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.Logging;

namespace Artefacts
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (!args.Contains("-nostartup"))//, StringComparer.OrdinalIgnoreCase))
			    Console.ReadLine();
			DebugLog<ConsoleLogger>.TrimSourceRootPath = "/home/jk/Code/Projects/NET/Artefacts/";
  			new ArtefactsHost().Init().Start("http://localhost:8888/Artefacts/");
			Console.ReadLine();
		}
	}
}
