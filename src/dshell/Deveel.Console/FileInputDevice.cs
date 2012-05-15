using System;
using System.IO;

namespace Deveel.Console {
	public sealed class FileInputDevice : InputDevice {
		private readonly string fileName;
		private readonly StreamReader reader;
		
		public FileInputDevice(string fileName) {
			this.fileName = fileName;
			reader = new StreamReader(fileName);
		}
		
		public string FileName {
			get { return fileName; }
		}
		
		public override int Read() {
			return reader.Read();
		}
		
		public override int Read(char[] buffer, int index, int count) {
			return reader.Read(buffer, index, count);
		}
		
		public override string ReadLine() {
			return reader.ReadLine();
		}
		
		public override void Close() {
			reader.Close();
		}
	}
}