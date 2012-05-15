using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Deveel.Util;

namespace Deveel.Console {
	public delegate void StreamAction(Stream stream);

	public class ConfigurationFile {
		public ConfigurationFile(string configFile) {
			_configFile = configFile;
			LoadFromFile();
		}

		private readonly string _configFile;
		private byte[] inputDigest;
		private Properties properties;
		private Dictionary<string, string> props;

		public string FileName {
			get { return _configFile; }
		}

		internal IDictionary<string, string> Properties {
			get { 
				if (props == null) {
					props = new Dictionary<string, string>();
					foreach(KeyValuePair<object, object> entry in properties)
						props[entry.Key.ToString()] = entry.Value == null ? null : entry.Value.ToString();
				}
				return props;
			}
		}

		private Stream GetInput() {
			FileStream input = null;

			try {
				MD5 md5 = MD5.Create();
				input = new InputFileStream(this, md5);
				if (!input.CanRead)
					return null;

				return input;
			} catch (Exception) {
				if (input != null)
					input.Close();

				return null;
			}
		}

		private class InputFileStream : FileStream {
			public InputFileStream(ConfigurationFile file, HashAlgorithm algorithm)
				: base(file._configFile, FileMode.Open, FileAccess.Read, FileShare.Read) {
				this.file = file;
				this.algorithm = algorithm;
				readStream = new MemoryStream();
			}

			private bool closed;
			private readonly MemoryStream readStream;
			private readonly HashAlgorithm algorithm;
			private readonly ConfigurationFile file;

			public override int Read(byte[] array, int offset, int count) {
				int len = base.Read(array, offset, count);
				readStream.Write(array, offset, len);
				return len;
			}

			public override void Close() {
				if (!closed) {
					base.Close();
					file.inputDigest = algorithm.ComputeHash(readStream);
				}
				closed = true;
			}
		}

		public void Read(StreamAction action) {
			try {
				Stream input = GetInput();
				try {
					action(input);
				} finally {
					if (input != null)
						input.Close();
				}
			} catch (Exception) {
			}
		}

		public void Write(StreamAction action) {
			string tmpFile = null;
			try {
				tmpFile = CreateTempFile("config-", ".tmp", Path.GetDirectoryName(_configFile));
				MD5 outputDigest = MD5.Create();
				OutputFileStream output = new OutputFileStream(tmpFile, outputDigest);
				try {
					action(output);
				} finally {
					output.Close();
				}
				if (inputDigest == null || !File.Exists(_configFile) ||
					!ArraysAreEqual(inputDigest, output.ComputedHash)) {
					if (File.Exists(_configFile))
						File.Delete(_configFile);
					File.Move(tmpFile, _configFile);
				}
			} catch (Exception e) {
				System.Console.Error.WriteLine("do not write config. Error occured: " + e);
			} finally {
				if (tmpFile != null)
					File.Delete(tmpFile);
			}
		}

		private static bool ArraysAreEqual(Array a, Array b) {
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++) {
				object aValue = a.GetValue(i);
				object bValue = b.GetValue(i);
				if (!Equals(aValue, bValue))
					return false;
			}
			return true;
		}

		private class OutputFileStream : FileStream {
			public OutputFileStream(string file, HashAlgorithm algorithm)
				: base(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None) {
				this.algorithm = algorithm;
				writeStream = new MemoryStream();
			}

			private byte[] computedHash;
			private readonly HashAlgorithm algorithm;
			private bool closed;
			private readonly MemoryStream writeStream;

			public byte[] ComputedHash {
				get { return computedHash; }
			}

			public override void Write(byte[] array, int offset, int count) {
				writeStream.Write(array, offset, count);
				base.Write(array, offset, count);
			}

			public override void Close() {
				if (!closed) {
					base.Close();
					writeStream.Flush();
					writeStream.Close();
					computedHash = algorithm.ComputeHash(writeStream.ToArray());
				}
				closed = true;
			}
		}

		private static string CreateTempFile(string prefix, string ext, string path) {
			Random rnd = new Random();
			string file;
			while (File.Exists((file = Path.Combine(path, GenerateTempFileName(rnd, prefix, ext)))))
				continue;
			return file;
		}

		private static string GenerateTempFileName(Random rnd, string prefix, string ext) {
			const string hex = "ABCDEFGH0123456789";
			StringBuilder sb = new StringBuilder(prefix.Length + 10 + ext.Length);
			sb.Append(prefix);
			for (int i = 0; i < 10; i++)
				sb.Append(hex[rnd.Next(hex.Length)]);
			sb.Append(ext);
			return sb.ToString();
		}

		private void LoadFromFile() {
			properties = new Properties();

			Stream input = GetInput();
			if (input != null) {
				try {
					properties.Load(input);
				} catch (Exception e) {
					System.Console.Error.WriteLine(e.Message); // can't help.
				} finally {
					input.Close();
				}
			}
		}

		public void ClearValues() {
			properties.Clear();
		}

		public void SetValue(string key, string value) {
			if (value == null)
				RemoveValue(key);
			else
				properties.SetProperty(key, value);
		}

		public void RemoveValue(string key) {
			properties.Remove(key);
		}

		public string GetValue(string key) {
			return properties.GetProperty(key);
		}

		public void Merge(IDictionary props) {
			// all properties, that are not present compared to last read
			// should be removed after merge.
			ArrayList locallyRemovedProperties = new ArrayList();
			locallyRemovedProperties.AddRange(properties.Keys);
			foreach (object key in props.Keys)
				locallyRemovedProperties.Remove(key);

			foreach (object key in locallyRemovedProperties)
				properties.Remove(key);

			foreach (DictionaryEntry entry in props)
				properties[entry.Key] = entry.Value;
		}

		public void Save(string comment) {
			PropertiesWriter writer = new PropertiesWriter(properties, comment);
			Write(new StreamAction(writer.Store));
		}

		public void Save() {
			Save(null);
		}

		private class PropertiesWriter {
			public PropertiesWriter(Properties outputProperties, string comment) {
				this.outputProperties = outputProperties;
				this.comment = comment;
			}

			private readonly string comment;
			private readonly Properties outputProperties;

			public void Store(Stream stream) {
				outputProperties.Store(stream, comment);
				stream.Close();
			}
		}
	}
}