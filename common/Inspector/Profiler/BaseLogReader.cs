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
using System.IO;
using System.Threading;

namespace XamarinProfiler.Core.Reader
{
	public class BaseLogReader
	{
		protected bool Exited;
		readonly string fileName;

		internal CachedBinaryReader Reader;

		public LogHeader Header;

		public string FileName {
			get {
				return fileName;
			}
		}

		public BaseLogReader (string fileName)
		{
			this.fileName = fileName;
		}

		public bool IsEof {
			get { return Reader.IsEof; }
		}

		public bool OpenReader ()
		{
			while (new FileInfo (fileName).Length == 0)
				Thread.Sleep (20);
			Reader = new CachedBinaryReader (fileName);
			int i = 0;
			while (i ++ < 5) {
				try {
					Header = LogHeader.Read (Reader);
					if (Header != null)
						return true;
				} catch (Exception ex) {
					Console.WriteLine ("BaseLogReader exception {0}", ex.Message);
				}
				Thread.Sleep (50);
			}
			return false;
		}

		public long Length {
			get {
				return Reader.Length;
			}
		}

		public BufferDescriptor TryReadBuffer (BufferDescriptor bufferDescriptor, EventListener listener)
		{
			if (bufferDescriptor != null)
				Reader.Position = bufferDescriptor.FilePosition;
			return BufferDescriptor.Read (Header, Reader, listener);
		}

		public void DeleteLogfile ()
		{
			try {
				Console.WriteLine ("Delete: " + fileName);
				File.Delete (fileName);
			} catch (Exception ex) {
				Console.WriteLine ("BaseLogReader exception {0}", ex.Message);
			}
		}
	}
}