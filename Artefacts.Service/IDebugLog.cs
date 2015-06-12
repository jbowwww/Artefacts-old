using System;
using ServiceStack.Logging;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Artefacts
{
	public interface IDebugLog// : ILog
	{
		void Debug(object message = null, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void DebugVariable(string variableName, object variable, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void Debug(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void DebugFormat(string format, object[] args, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);

		void Error(object message = null, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void ErrorVariable(string variableName, object variable, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void Error(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void ErrorFormat(string format, object[] args, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void ThrowError(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void ThrowError(Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		
		void Fatal(object message = null, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void FatalVariable(string variableName, object variable, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void Fatal(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void FatalFormat(string format, object[] args, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);

		void Info(object message = null, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void InfoVariable(string variableName, object variable, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void Info(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void InfoFormat(string format, object[] args, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);

		void Warn(object message = null, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void WarnVariable(string variableName, object variable, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void Warn(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
		void WarnFormat(string format, object[] args, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0);
	}
}
