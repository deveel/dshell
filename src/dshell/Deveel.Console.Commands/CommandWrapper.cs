using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

using Deveel.Configuration;

namespace Deveel.Console.Commands {
	internal class CommandWrapper : Command {
		public CommandWrapper(Type type, object application) {
			obj = Instantiate(type, application);
			if (obj == null)
				throw new InvalidOperationException();

			executeMethod = GetExecuteMethod(obj);
			handleCommandLineMethod = GetHandleCommandLineMethod(obj);
			registerOptionsMethod = GetRegisterOptionsMethod(obj);
		}

		private readonly object obj;
		private readonly MethodInfo executeMethod;
		private readonly MethodInfo handleCommandLineMethod;
		private readonly MethodInfo registerOptionsMethod;

		private CommandAttributes attributes;

		private CommandAttributes Attributes {
			get {
				if (attributes == null)
					attributes = RetrieveCommandAttributes();
				return attributes;
			}
		}

		public override string ShortDescription {
			get { return Attributes.ShortDescription; }
		}

		public override bool CommandCompletion {
			get { return Attributes.CommandCompletion; }
		}

		public override bool RequiresContext {
			get { return Attributes.RequiresContext; }
		}

		public override string GroupName {
			get { return Attributes.CommandGroup; }
		}

		public override string[] Aliases {
			get { return (string[])Attributes.Aliases.ToArray(typeof(string)); }
		}

		public override string Name {
			get { return Attributes.CommandName; }
		}

		public override string[] Synopsis {
			get {
				ArrayList versions = Attributes.Synopsis;
				if (versions == null || versions.Count == 0)
					return new string[] { Name };

				return (string[])versions.ToArray(typeof(string));
			}
		}

		public override string LongDescription {
			get { return Attributes.Description; }
		}

		private CommandAttributes RetrieveCommandAttributes() {
			CommandAttributes commandAttributes = new CommandAttributes();

			object[] attrs = obj.GetType().GetCustomAttributes(typeof(ICommandAttribute), false);
			if (attrs.Length > 0) {
				for (int i = 0; i < attrs.Length; i++) {
					object attr = attrs[i];
					if (attr is CommandAttribute) {
						CommandAttribute commandAttr = (CommandAttribute)attr;
						commandAttributes.CommandName = commandAttr.CommandName;
						commandAttributes.RequiresContext = commandAttr.RequiresContext;
						commandAttributes.ShortDescription = commandAttr.ShortDescription;
					} else if (attr is CommandAliasAttribute) {
						CommandAliasAttribute aliasAttribute = (CommandAliasAttribute)attr;
						commandAttributes.Aliases.Add(aliasAttribute.Alias);
					} else if (attr is CommandSynopsisAttribute) {
						CommandSynopsisAttribute synopsisAttr = (CommandSynopsisAttribute)attr;
						if (synopsisAttr.Text != null && synopsisAttr.Text.Length > 0)
							commandAttributes.Synopsis.Add(synopsisAttr.Text);
					} else if (attr is CommandGroupAttribute) {
						CommandGroupAttribute groupAttribute = (CommandGroupAttribute)attr;
						commandAttributes.CommandGroup = groupAttribute.GroupName;
					} else if (attr is CommandCompletionAttribute) {
						CommandCompletionAttribute completionAttribute = (CommandCompletionAttribute)attr;
						attributes.CommandCompletion = completionAttribute.CompleteCommand;
					} else if (attr is CommandDesctiprionAttribute) {
						CommandDesctiprionAttribute desctiprionAttribute = (CommandDesctiprionAttribute)attr;
						attributes.Description = ReadDescription(desctiprionAttribute.Value, desctiprionAttribute.Source);
					}
				}
			}

			return commandAttributes;
		}

		private string ReadDescription(string value, DescriptionSource source) {
			if (source == DescriptionSource.Direct)
				return value;

			Stream inputStream = null;

			try {
				if (source == DescriptionSource.Resource) {
					Assembly assembly = Assembly.GetExecutingAssembly();
					inputStream = assembly.GetManifestResourceStream(value);
				} else if (source == DescriptionSource.LocalFile) {
					inputStream = new FileStream(value, FileMode.Open, FileAccess.Read, FileShare.Read);
				} else {
					WebRequest request = WebRequest.Create(value);
					WebResponse response = request.GetResponse();
					if (response == null)
						return null;

					inputStream = response.GetResponseStream();
				}

				if (inputStream == null)
					return null;

				StreamReader reader = new StreamReader(inputStream);
				StringBuilder sb = new StringBuilder();
				string line;
				while ((line = reader.ReadLine()) != null)
					sb.AppendLine(line);

				return sb.ToString();
			} catch (Exception) {
				Application.Error.WriteLine("Error while retrieving the description for the command.");
				return null;
			} finally {
				if (inputStream != null)
					inputStream.Close();
			}
		}

		private static MethodInfo GetExecuteMethod(object obj) {
			if (obj is IExecutable)
				return null;

			Type type = obj.GetType();
			MethodInfo method = type.GetMethod("Execute", new Type[] {typeof (object), typeof (string[])});
			if (method == null)
				return null;

			if (method.ReturnType != typeof(CommandResultCode) &&
				method.ReturnType != typeof(int))
				return null;

			return method;
		}

		private static object Instantiate(Type type, object application) {
			ConstructorInfo ctor = null;
			int parCount = 0;
			ConstructorInfo[] ctors = type.GetConstructors();
			for (int i = 0; i < ctors.Length; i++) {
				ctor = ctors[i];
				ParameterInfo[] pars = ctor.GetParameters();
				if ((parCount = pars.Length) <= 1) {
					break;
				}
			}

			if (ctor == null)
				throw new NotSupportedException();

			if (parCount == 1)
				return ctor.Invoke(new object[] {application});
			return ctor.Invoke(null);
		}

		private static MethodInfo GetHandleCommandLineMethod(object obj) {
			if (obj is IOptionsHandler)
				return null;

			Type type = obj.GetType();
			MethodInfo method = type.GetMethod("HandleCommandLine");
			if (method == null)
				return null;

			ParameterInfo[] pars = method.GetParameters();
			if (pars.Length != 1)
				return null;

			if (pars[0].ParameterType != typeof(CommandLine))
				return null;

			return method;
		}

		private static MethodInfo GetRegisterOptionsMethod(object obj) {
			if (obj is IOptionsHandler)
				return null;

			Type type = obj.GetType();
			MethodInfo method = type.GetMethod("RegisterOptions");
			if (method == null)
				return null;

			ParameterInfo[] pars = method.GetParameters();
			if (pars.Length != 1)
				return null;

			if (pars[0].ParameterType != typeof(Options))
				return null;

			return method;
		}

		public override void RegisterOptions(Options options) {
			if (obj is IOptionsHandler) {
				((IOptionsHandler)obj).RegisterOptions(options);
			} else if (registerOptionsMethod != null) {
				registerOptionsMethod.Invoke(obj, new object[] {options});
			} else {
				throw new NotSupportedException();
			}
		}

		public override bool HandleCommandLine(CommandLine commandLine) {
			if (obj is IOptionsHandler)
				return ((IOptionsHandler)obj).HandleCommandLine(commandLine);
			if (handleCommandLineMethod != null)
				return (bool)handleCommandLineMethod.Invoke(obj, new object[] {commandLine});

			return false;
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (obj is IExecutable)
				return ((IExecutable) obj).Execute(context, args);
			if (executeMethod != null)
				return (CommandResultCode) executeMethod.Invoke(obj, new object[] {context, args});
			return CommandResultCode.ExecutionFailed;
		}

		private class CommandAttributes {
			public readonly ArrayList Aliases = new ArrayList();
			public string CommandName;
			public bool RequiresContext;
			public string ShortDescription;
			public string Description = String.Empty;
			public readonly ArrayList Synopsis = new ArrayList();
			public string CommandGroup;
			public bool CommandCompletion = true;
		}
	}
}