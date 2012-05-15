using System;
using System.Collections;
using System.Text;

namespace Deveel.Console.Commands {
	public abstract class CommandSeparator : ICommandSeparator {
		private ParseState currentState;
		private readonly Stack stateStack;

		protected CommandSeparator() {
			currentState = new ParseState();
			stateStack = new Stack();
		}

		public string Current {
			get {
				if (currentState.Type != (int)TokenType.PotentialEndFound)
					throw new InvalidOperationException("Current called without MoveNext()");
				return currentState.CommandBuffer.ToString();
			}
		}

		object IEnumerator.Current {
			get { return Current; }
		}

		void IEnumerator.Reset() {
		}

		void IDisposable.Dispose() {
		}

		private void ParsePartialInput() {
			int pos = 0;
			char current;
			int oldstate = -1;

			// local variables: faster access.
			int state = currentState.Type;
			bool lastEoline = currentState.NewlineSeen;

			StringBuilder input = currentState.InputBuffer;
			StringBuilder parsed = currentState.CommandBuffer;

			if (state == (int) TokenType.NewStatement) {
				parsed.Length = 0;
				// skip leading whitespaces of next statement...
				while (pos < input.Length &&
					Char.IsWhiteSpace(input[pos])) {
					//CHECK: what about \r?
					currentState.NewlineSeen = (input[pos] == '\n');
					++pos;
				}
				input.Remove(0, pos);
				pos = 0;
			}

			if (input.Length == 0)
				state = (int) TokenType.PotentialEndFound;

			while (state != (int) TokenType.PotentialEndFound && 
				pos < input.Length) {
				bool reIterate;
				current = input[pos];
				if (current == '\r')
					current = '\n'; // canonicalize.

				if (current == '\n')
					currentState.NewlineSeen = true;

				do {
					reIterate = false;
					ParseToken token = new ParseToken(state, current, lastEoline);
					state = Parse(token);

					if (token.NewLineSeenWasSet)
						lastEoline = token.NewLineSeen;

					if (token.AppendedCharacter != '\0')
						parsed.Append(token.AppendedCharacter);

					if (token.ContinueParsingWasSet)
						reIterate = token.ContinueParsing;
				} while (reIterate);

				ParseToken postToken = new ParseToken(oldstate, current, lastEoline);
				PostParse(postToken);

				if (postToken.AppendedCharacter != '\0')
					parsed.Append(postToken.AppendedCharacter);

				oldstate = state;
				pos++;
				// we maintain the state of 'just seen newline' as long
				// as we only skip whitespaces..
				lastEoline &= Char.IsWhiteSpace(current);
			}

			// we reached: POTENTIAL_END_FOUND. Store the rest, that
			// has not been parsed in the input-buffer.
			input.Remove(0, pos);
			currentState.Type = state;
		}

		protected abstract int Parse(ParseToken token);

		protected virtual void PostParse(ParseToken token) {
		}

		public void Push() {
			stateStack.Push(currentState);
			currentState = new ParseState();
		}

		public void Pop() {
			currentState = (ParseState)stateStack.Pop();
		}

		public void Append(string s) {
			currentState.InputBuffer.Append(s);
		}

		public void Discard() {
			currentState.InputBuffer.Length = 0;
			currentState.CommandBuffer.Length = 0;
			currentState.Type = (int) TokenType.NewStatement;
		}

		public void Cont() {
			currentState.Type = (int) TokenType.Start;
		}

		public void Consumed() {
			currentState.Type = (int) TokenType.NewStatement;
		}

		public bool MoveNext() {
			if (currentState.Type == (int) TokenType.PotentialEndFound)
				throw new InvalidOperationException("call Cont() or Consumed() before MoveNext()");
			if (currentState.InputBuffer.Length == 0)
				return false;

			ParsePartialInput();
			return (currentState.Type == (int) TokenType.PotentialEndFound);
		}

		#region ParseState

		class ParseState {
			private int _state;
			private StringBuilder _inputBuffer;
			private StringBuilder _commandBuffer;
			/*
			 * instead of adding new states, we store the
			 * fact, that the last 'potential_end_found' was
			 * a newline here.
			 */
			private bool _eolineSeen;

			internal ParseState() {
				_eolineSeen = true; // we start with a new line.
				_state = (int) TokenType.NewStatement;
				_inputBuffer = new StringBuilder();
				_commandBuffer = new StringBuilder();
			}


			public int Type {
				get { return _state; }
				set { _state = value; }
			}

			public bool NewlineSeen {
				get { return _eolineSeen; }
				set { _eolineSeen = value; }
			}

			public StringBuilder InputBuffer {
				get { return _inputBuffer; }
			}

			public StringBuilder CommandBuffer {
				get { return _commandBuffer; }
			}
		}
		#endregion

		#region TokenType

		protected enum TokenType {
			NewStatement = 0,
			Start = 1,
			PotentialEndFound = -1
		}

		#endregion

		#region ParseToken

		protected sealed class ParseToken {
			internal ParseToken(int oldState, char current, bool newLineSeen) {
				this.oldState = oldState;
				this.newLineSeen = newLineSeen;
				this.current = current;
			}

			private readonly int oldState;
			private int newState;
			private readonly char current;
			private char toAdd;
			private bool newLineSeen;
			private bool newLineSeenSet;
			private bool continueParsing;
			private bool _continueParsingWasSet;

			public char CurrentCharacter {
				get { return current; }
			}

			public int OldState {
				get { return oldState; }
			}

			public int NewState {
				get { return newState; }
				set { newState = value; }
			}

			public bool NewLineSeen {
				get { return newLineSeen; }
				set {
					newLineSeen = value;
					newLineSeenSet = true;
				}
			}

			internal bool NewLineSeenWasSet {
				get { return newLineSeenSet; }
			}

			public bool ContinueParsing {
				get { return continueParsing; }
				set {
					continueParsing = value;
					_continueParsingWasSet = true;
				}
			}

			public char AppendedCharacter {
				get { return toAdd; }
				set { toAdd = value; }
			}

			internal bool ContinueParsingWasSet {
				get { return _continueParsingWasSet; }
			}
		}

		#endregion
	}
}