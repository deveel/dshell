using System;
using System.Collections.Generic;

using Deveel.Configuration;

namespace Deveel.Console.Commands {
	/// <summary>
	/// Represents a user level command.
	/// </summary>
	/// <remarks>
	/// This interface needs to be implemented by commands or plugins that 
	/// should be supported by Minosse.
	/// </remarks>
	public abstract class Command : IExecutable, IOptionsHandler {
		#region Fields

		private IApplicationContext application;

		#endregion

		#region Properties

		public IApplicationContext Application {
			get { return application; }
		}

		protected OutputDevice Out {
			get { return Application.Out; }
		}

		protected OutputDevice Error {
			get { return Application.Error; }
		}

		public virtual string GroupName {
			get { return null; }
		}

		public virtual string [] Aliases {
			get { return new string[0]; }
		}

		public bool HasAliases {
			get {
				string[] aliases = Aliases;
				return aliases != null && aliases.Length > 0;
			}
		}

		public abstract string Name { get; }


		internal void Init() {
			OnInit();
		}

		/// <summary>
		/// Gets wheter or not the commands supported by this <see cref="Command"/> 
		/// should not be part of the toplevel command completion.
		/// </summary>
		/// <remarks>
		/// If the user presses <i>TAB</i> on an empty string to get the full 
		/// list of possible commands, this command should not show up.
		/// In Minosse, this returns <b>false</b> for the SQL-commands 
		/// (<i>select</i>, <i>update</i>, <i>drop</i> ..), since this would
		/// clobber the toplevel list of available commands.
		/// If unsure, returns <b>true</b>.
		/// </remarks>
		public virtual bool CommandCompletion {
			get { return true; }
		}

		/// <summary>
		/// Returns a short string describing the purpose of the commands
		/// handled by this Command-implementation.
		/// </summary>
		/// <remarks>
		/// This is the string listed in the bare 'help' overview. Should 
		/// contain no newline, no leading spaces.
		/// </remarks>
		public virtual string ShortDescription {
			get { return null; }
		}

		#endregion

		
		#region Public Methods

		/// <summary>
		/// Executes the given command.
		/// </summary>
		/// <param name="context">The session this command is executed from.</param>
		/// <param name="args">The rest parameters following the command.</param>
		/// <remarks>
		/// The command is given completely without the final delimiter 
		/// (which would be newline or semicolon). Before this method is 
		/// called, the <see cref="CommandDispatcher"/> checks with the 
		/// <see cref="IsComplete(String)"/> method, if this command is 
		/// complete.
		/// </remarks>
		/// <returns>
		/// Returns a <see cref="CommandResultCode">code</see> indicating the
		/// result of the command execution.
		/// </returns>
		public abstract CommandResultCode Execute(IExecutionContext context, CommandArguments args);

		/// <summary>
		/// Returns a list of strings that are possible at this stage.
		/// </summary>
		/// <param name="dispatcher">The <see cref="CommandDispatcher"/> that
		/// can be used to access other values through it.</param>
		/// <param name="partialCommand">The command typed so far.</param>
		/// <param name="lastWord">The last word returned by readline.</param>
		/// <remarks>
		/// Used for the readline-completion in interactive mode. Based on 
		/// the partial command and the <paramref name="lastWord"/> you have 
		/// to determine the words that are available at this stage. Returns 
		/// <b>null</b>, if you don't know a possible completion.
		/// </remarks>
		/// <returns>
		/// </returns>
		public virtual IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
			return null;
		}

		/// <summary>
		/// Checks wether the command is complete or not.
		/// </summary>
		/// <param name="command">The partial command read so far given to 
		/// decide by the command whether it is complete or not.</param>
		/// <remarks>
		/// This method is called, whenever the input encounters a newline or
		/// a semicolon to decide if this separator is to separate different
		/// commands or if it is part of the command itself.
		/// <para>
		/// The delimiter (newline or semicolon) is contained (at the end)
		/// in the string passed to this method. This method returns <b>false</b>, 
		/// if the delimiter is part of the command and will not be regarded 
		/// as delimiter between commands -- the reading part of the command 
		/// dispatcher will go on reading characters and not execute the 
		/// command.
		/// <para>
		/// This method will return true for most simple commands like <i>help</i>.
		/// For commands that have a more complicated syntax, this might not 
		/// be true.
		/// </para>
		/// <list>
		///		<item>
		///		<i>select * from foobar</i> is not complete after a return, 
		///		since we can expect a where clause. If it has a semicolon at 
		///		the end, we know, that is is complete. So newline is <b>not</b> 
		///		a delimiter while <i>;</i> is (return command.EndsWith(";")).
		///		</item>
		///		<item>
		///		definitions of stored procedures are even more complicated: 
		///		it depends on the syntax whether a semicolon is part of the
		///		command or can be regarded as delimiter. Here, neither <i>;</i> 
		///		nor newline can be regarded as delimiter per-se. Only the 
		///		<see cref="Command"/> implementation can decide upon this.
		///		In Minosse, a single <i>/</i> on one line is used to denote 
		///		this command-complete.
		///		</item>
		///	</list>
		///	Note, this method should only apply a very lazy syntax check so 
		///	it does not get confused and uses too much cycles unecessarily.
		/// </remarks>
		/// <returns></returns>
		public virtual bool IsComplete(string command) {
			return true;
		}

		/// <summary>
		/// Check wheter or not this command requires a valid context.
		/// </summary>
		public virtual bool RequiresContext {
			get { return false; }
		}

		/// <summary>
		/// Called when the command is initialized and registered
		/// into the application context.
		/// </summary>
		protected virtual void OnInit() {
		}

		/// <summary>
		/// Alerts the method that the application is closing.
		/// </summary>
		/// <remarks>
		/// This is called on exit of the <see cref="CommandDispatcher"/> 
		/// and allows you to do some cleanup (close connections, flush files..).
		/// </remarks>
		public virtual void OnShutdown() {
		}

		/// <summary>
		/// Returns the synopsis string for the command.
		/// </summary>
		/// <remarks>
		/// The synopsis string returned should follow the following conventions:
		/// <list>
		///		<item>expected parameters are described with angle brackets 
		///		like in <c>export-xml &lt;table&gt; &lt;filename&gt;</c></item>
		///		<item>optional parameters are described with square brackets 
		///		like in <c>help [command]</c></item>
		///	</list>
		///	<para>
		///	The string returned should contain no newline, no leading spaces. 
		///	This synopsis is printed in the detailed help of a command or if 
		///	the <see cref="Execute"/> method returned a <see cref="CommandResultCode.SyntaxError"/>.
		/// </para>
		/// </remarks>
		/// <returns></returns>
		public virtual string[] Synopsis {
			get { return new string[] {Name}; }
		}

		/// <summary>
		/// Gets a full description string of the command.
		/// </summary>
		/// <remarks>
		/// The returned description should start with a TAB-character 
		/// in each new line (the first line is a new line). The last 
		/// line should not end with newline.
		/// </remarks>
		public virtual string LongDescription {
			get { return null; }
		}

		public bool HasGroup {
			get {
				string groupName = GroupName;
				return (groupName != null && groupName.Length > 0);
			}
		}

		/// <summary>
		/// Registers the command specific command-line options.
		/// </summary>
		/// <param name="options">The command-line options to register.</param>
		/// <remarks>
		/// This method is called just before the command-line parser
		/// is executed to set the command-specific configurations.
		/// </remarks>
		public virtual void RegisterOptions(Options options) {
		}

		/// <summary>
		/// Handles the parsed command-line configurations of the command.
		/// </summary>
		/// <param name="commandLine">The object containing the configurations
		/// parsed from the command-line.</param>
		/// <remarks>
		/// This method is called after the parse of the command-line
		/// configurations is done to handle the values of the options
		/// set on the <see cref="RegisterOptions"/> method.
		/// </remarks>
		/// <returns>
		/// Returns <b>true</b> if the command handled the command line
		/// arguments and was executed, otherwise it returns <b>false</b>.
		/// </returns>
		public virtual bool HandleCommandLine(CommandLine commandLine) {
			return false;
		}

		#endregion

		internal virtual void SetApplicationContext(IApplicationContext app) {
			application = app;
		}
	}
}