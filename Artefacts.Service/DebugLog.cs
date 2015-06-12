using System;
using ServiceStack.Logging;
using System.Reflection;
using System.Diagnostics;
using ServiceStack;
using System.Runtime.CompilerServices;

namespace Artefacts
{
	/// <summary>
	/// Debug log.
	/// </summary>
	/// <typeparam name="T">Type of log (implements <see cref="ServiceStack.Logging.ILog"/> </typeparam>
	public class DebugLog<T> : IDebugLog where T : class, ILog
	{
		#region Static Members
		/// <summary>
		/// The trim source root path.
		/// </summary>
		public static string TrimSourceRootPath = "";

		/// <summary>
		/// The name of the trim source class.
		/// </summary>
		public static string TrimSourceClassName = "";
		#endregion

		#region Fields & Properties
		/// <summary>
		/// The _log.
		/// </summary>
		private T _log;

		/// <summary>
		/// Gets a value indicating whether this instance is debug enabled.
		/// </summary>
		/// <value><c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.</value>
		/// <remarks>ILog implementation</remarks>
		public bool IsDebugEnabled { get { return _log.IsDebugEnabled; } }

		/// <summary>
		/// Gets or sets the prefix.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Artefacts.DebugLog`1"/> include pid.
		/// </summary>
		public bool IncludePid { get; set; }

		/// <summary>
		/// Gets or sets the source class.
		/// </summary>
		/// <value>The source class.</value>
		public Type SourceClass { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Artefacts.DebugLog`1"/> use short class name.
		/// </summary>
		public bool UseShortClassName { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.DebugLog`1"/> class.
		/// </summary>
		/// <param name="type">Type.</param>
		public DebugLog(Type type)
		{
			ConstructorInfo ci =
				typeof(T).GetConstructor(new Type[] { typeof(Type) })
			??	typeof(T).GetConstructor(new Type[] { });
//			if (ci == null)
//				throw new MissingMethodException(string.Format("Type {0} does not have a suitable constructor", ))
			_log = (T)(ci.GetParameters().Length > 0 ?
				ci.Invoke(new object[] { type }) 
			:	ci.Invoke(new object[] { }));

			IncludePid = true;
			UseShortClassName = true;
			SourceClass = type;
		}

		#region String helper functions
		/// <summary>
		/// Gets the formatted optional string.
		/// </summary>
		/// <returns>The formatted optional string.</returns>
		/// <param name="option">Option.</param>
		private string GetOptionalString(string option, string suffix = ": ")
		{
			return string.IsNullOrWhiteSpace(option) ? string.Concat(option, suffix) : string.Empty;
		}

		/// <summary>
		/// Gets the formatted optional string.
		/// </summary>
		/// <returns>The formatted optional string.</returns>
		/// <param name="condition"></param>
		/// <param name="option">Option.</param>
		private string GetOptionalString(bool condition, string option, string suffix = ": ")
		{
			return condition ? string.Concat(option, suffix) : string.Empty;
		}

		/// <summary>
		/// Gets the full prefix.
		/// </summary>
		/// <returns>The full prefix.</returns>
		private string GetFullPrefix(string caller)
		{
			Process currentProcess = Process.GetCurrentProcess();
			return string.Concat(
				GetOptionalString(IncludePid, currentProcess.Id.ToString()),
				GetOptionalString(IncludePid, currentProcess.ProcessName),
				GetOptionalString(
					SourceClass != null,
			        UseShortClassName ? SourceClass.Name : SourceClass.FullName, ".").Trim(TrimSourceClassName),
					caller,	// ": ",
				GetOptionalString(Prefix));
		}

		/// <summary>
		/// Gets the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="message">Message.</param>
		private string GetString(object message)
		{
			return message == null ? "(null)" : message is IDebugFormattable ? ((IDebugFormattable)message).DebugString() : message.ToString();
		}

		/// <summary>
		/// Gets the source file string.
		/// </summary>
		/// <returns>The source file string.</returns>
		/// <param name="">.</param>
		private string GetSourceFileString(string sourceFile, int sourceLine)
		{
			return string.Concat(" @ ", sourceFile.TrimStart(TrimSourceRootPath), ":", sourceLine);
		}
		#endregion

		#region IDebugLog implementation
		#region Debug
		/// <summary>
		/// Debug the specified message, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <remarks>IDebugLog implementation</remarks>
		public void Debug(object message = null, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Debug(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Debug the specified variableName, variable, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="variableName">Variable name.</param>
		/// <param name="variable">Variable.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void DebugVariable(string variableName, object variable, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Debug(string.Concat(GetFullPrefix(caller), variableName, ": ", GetString(variable), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Debug the specified message, exception, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <remarks>IDebugLog implementation</remarks>
		public void Debug(object message, Exception exception, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Debug(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)), exception);
		}

		/// <summary>
		/// Debugs the format.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <param name="args">Arguments.</param>
		/// <remarks>IDebugLog implementation</remarks>
		public void DebugFormat(string format, object[] args, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Debug(string.Concat(GetFullPrefix(caller), string.Format(format, args), GetSourceFileString(sourceFile, sourceLine)));
		}
		#endregion

		#region Error
		/// <summary>
		/// Error the specified message, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Error(object message = null, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Error(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Error the specified variableName, variable, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="variableName">Variable name.</param>
		/// <param name="variable">Variable.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void ErrorVariable(string variableName, object variable, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Error(string.Concat(GetFullPrefix(caller), variableName, ": ", GetString(variable), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Error the specified message, exception, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Error(object message, Exception exception, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Error(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)), exception);
		}

		/// <summary>
		/// Errors the format.
		/// </summary>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		public void ErrorFormat(string format, object[] args, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Error(string.Concat(GetFullPrefix(caller), string.Format(format, args), GetSourceFileString(sourceFile, sourceLine)));
		}
		
		/// <summary>
		/// Throws the error.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void ThrowError(object message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0)
		{
			_log.Error(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)), exception);
			throw exception;
		}
		
		/// <summary>
		/// Throws the error.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void ThrowError(Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0)
		{
			_log.Error(string.Concat(GetFullPrefix(caller), GetSourceFileString(sourceFile, sourceLine)), exception);
			throw exception;
		}
		#endregion

		#region Fatal
		/// <summary>
		/// Fatal the specified message, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Fatal(object message = null, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Fatal(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Fatal the specified variableName, variable, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="variableName">Variable name.</param>
		/// <param name="variable">Variable.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void FatalVariable(string variableName, object variable, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Fatal(string.Concat(GetFullPrefix(caller), variableName, ": ", GetString(variable), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Fatal the specified message, exception, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Source name.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Fatal(object message, Exception exception, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Fatal(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)), exception);
		}

		/// <summary>
		/// Fatals the format.
		/// </summary>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		public void FatalFormat(string format, object[] args, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Fatal(string.Concat(GetFullPrefix(caller), string.Format(format, args), GetSourceFileString(sourceFile, sourceLine)));
		}
		#endregion

		#region Info
		/// <summary>
		/// Info the specified message, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Info(object message = null, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Info(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Info the specified variableName, variable, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="variableName">Variable name.</param>
		/// <param name="variable">Variable.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void InfoVariable(string variableName, object variable, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Info(string.Concat(GetFullPrefix(caller), variableName, ": ", GetString(variable), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Info the specified message, exception, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Info(object message, Exception exception, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Info(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)), exception);
		}

		/// <summary>
		/// Infos the format.
		/// </summary>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		public void InfoFormat(string format, object[] args, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Info(string.Concat(GetFullPrefix(caller), string.Format(format, args), GetSourceFileString(sourceFile, sourceLine)));
		}
		#endregion

		#region Warn
		/// <summary>
		/// Warn the specified message, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Warn(object message = null, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Warn(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Warn the specified message, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void WarnVariable(string variableName, object variable, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Warn(string.Concat(GetFullPrefix(caller), variableName, ": ", GetString(variable), GetSourceFileString(sourceFile, sourceLine)));
		}

		/// <summary>
		/// Warn the specified message, exception, caller, sourceFile and sourceLine.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		public void Warn(object message, Exception exception, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Warn(string.Concat(GetFullPrefix(caller), GetString(message), GetSourceFileString(sourceFile, sourceLine)), exception);
		}

		/// <summary>
		/// Warns the format.
		/// </summary>
		/// <param name="caller">Caller.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="sourceLine">Source line.</param>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		public void WarnFormat(string format, object[] args, string caller = "", string sourceFile = "", int sourceLine = 0)
		{
			_log.Warn(string.Concat(GetFullPrefix(caller), string.Format(format, args), GetSourceFileString(sourceFile, sourceLine)));
		}
		#endregion
		#endregion
		#endregion
	}
}

