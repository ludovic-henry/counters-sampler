//
// LogBuffer.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
//
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace XamarinProfiler.Core.Reader
{
	public class LogReader : BaseLogReader, IDisposable
	{
		readonly Process process;
		readonly CancellationTokenSource src = new CancellationTokenSource ();
		readonly CancellationToken token;

		public LogReader (Process process, string fileName) : base (fileName)
		{
			this.process = process;
			this.token = src.Token;
			OpenReader ();
			if (process != null) {
				process.Exited += delegate (object sender, EventArgs e) {
					Exited = true;
					OnProcessExited (EventArgs.Empty);
				};
			}
		}

		public event EventHandler ProcessExited;

		protected virtual void OnProcessExited (EventArgs e)
		{
			var handler = ProcessExited;
			if (handler != null)
				handler (this, e);
		}

		public bool HasProcess {
			get {
				return process != null;
			}
		}

		public bool IsStopping {
			get {
				return token.IsCancellationRequested;
			}
		}

		public void Stop ()
		{
			if (!Exited && process != null)
				process.Kill ();
			src.Cancel ();
		}

		public IEnumerable<BufferDescriptor> ReadBuffer (EventListener listener)
		{
			do {
				var buffer = TryReadBuffer (null, listener);
				if (buffer != null) {
					yield return buffer;
				} else {
					if (Exited && Reader.IsEof)
						break;
					Thread.Sleep (5);
				}
			} while (!token.IsCancellationRequested);
			Reader.Close ();
		}

		public void Dispose ()
		{
			if (process == null) // don't delete log file if no process is attached.
				return;

			if (!process.HasExited)
				process.Kill ();
			DeleteLogfile ();
		}
	}
}