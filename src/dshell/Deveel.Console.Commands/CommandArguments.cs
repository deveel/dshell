using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	public sealed class CommandArguments : IEnumerator<string> {
		private int index;
		private readonly string[] args;
		private int length;

		internal CommandArguments(string[] args) {
			this.args = args;
			length = args.Length;
			index = -1;
		}

		public int Count {
			get { return length; }
		}

		public string Current {
			get { return args[index]; }
		}

		public int CurrentIndex {
			get { return index; }
		}

		object IEnumerator.Current {
			get { return Current; }
		}

		void IDisposable.Dispose() {
		}

		public bool MoveNext() {
			return ++index < length;
		}

		public bool MoveBack() {
			return --index > 0;
		}

		public bool MoveTo(int offset) {
			int moveIndex = index + offset;
			if (moveIndex < 0 || moveIndex >= length)
				return false;

			index = moveIndex;
			return true;
		}

		public void Reset() {
			index = -1;
			length = args.Length;
		}

		public string Peek(int offset) {
			int peekIndex = index + offset;
			if (peekIndex >= length || peekIndex < 0)
				return null;

			return args[index + offset];
		}

		public override string ToString() {
			return String.Join(" ", args);
		}
	}
}