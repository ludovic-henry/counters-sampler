// 
// Buffer.cs
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

namespace XamarinProfiler.Core.Reader
{
	public class BufferDescriptor
	{
		public readonly long FilePosition;
		public readonly BufferHeader Header;

		bool haveTime;
		ulong minTime;
		ulong maxTime;

		public TimeSpan TimeSpan {
			get {
				return new TimeSpan ((long)(maxTime - minTime));
			}
		}

		BufferDescriptor (long filePosition, BufferHeader header)
		{
			this.FilePosition = filePosition;
			this.Header = header;
		}

		internal unsafe static BufferDescriptor Read (LogHeader logHeader, CachedBinaryReader reader, EventListener listener)
		{
			var pos = reader.Position;
			var header = BufferHeader.Read (reader);
			if (header == null)
				return null;

			if (!reader.LoadData (header.Length)) {
				reader.Position = pos;
				return null;
			}

			fixed (byte* buffer = reader.buffer) {
				// FIXME: unset the buffer pointer after this block!
				reader.SetBufferPointer (buffer);
				var result = new BufferDescriptor (pos, header);
				try {
					listener.ReadBufferEvents (logHeader, result, reader);
				} catch (Exception e) {
					Console.WriteLine ("error while reading event !");
					Console.WriteLine (e);
				}
				return result;
			}
		}

		public void RecordTime (ulong time)
		{
			if (!haveTime) {
				minTime = maxTime = time;
				haveTime = true;
				return;
			}

			if (time < minTime)
				minTime = time;
			else if (time > maxTime)
				maxTime = time;
		}

		public override string ToString ()
		{
			return string.Format ("[BufferDescriptor: Position={0}, Time={1}-{2}]", FilePosition, minTime, maxTime);
		}
	}
}
