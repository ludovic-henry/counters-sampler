//
// BufferHeader.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
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

namespace XamarinProfiler.Core.Reader
{
	/// <summary>
	///  buffer header format:
	///     [bufid: 4 bytes] constant value: BUF_ID
	///     [len: 4 bytes] size of the data following the buffer header
	///     [time_base: 8 bytes] time base in nanoseconds since an unspecified epoch
	///     [ptr_base: 8 bytes] base value for pointers
	///     [obj_base: 8 bytes] base value for object addresses
	///     [thread id: 8 bytes] system-specific thread ID (pthread_t for example)
	///     [method_base: 8 bytes] base value for MonoMethod pointers
	/// </summary>
	public class BufferHeader
	{
		// BUF_ID (mono/mono/profiler/proflog.h)
		const int BufId = 0x4D504C01;
		const int BufferSize = 48;

		/// <summary>
		/// size of the data following the buffer header
		/// </summary>
		public readonly int Length;

		/// <summary>
		/// time base in nanoseconds since an unspecified epoch
		/// </summary>
		public readonly ulong TimeBase;

		/// <summary>
		/// base value for pointers
		/// </summary>
		public readonly long PtrBase; 

		/// <summary>
		/// base value for object addresses
		/// </summary>
		public readonly long ObjBase;

		/// <summary>
		/// system-specific thread ID (pthread_t for example)
		/// </summary>
		public readonly long ThreadId;

		/// <summary>
		/// base value for MonoMethod pointers
		/// </summary>
		public readonly long MethodBase;
		
		BufferHeader (CachedBinaryReader reader)
		{
			var id = reader.ReadInt32 ();
			if (id != BufId)
				throw new IOException (string.Format ("Incorrect buffer id: 0x{0:X}", id));

			Length = reader.ReadInt32 ();
			TimeBase = reader.ReadUInt64 ();
			PtrBase = reader.ReadInt64 ();
			ObjBase = reader.ReadInt64 ();
			ThreadId = reader.ReadInt64 ();
			MethodBase = reader.ReadInt64 ();
		}

		internal unsafe static BufferHeader Read (CachedBinaryReader reader)
		{
			if (!reader.LoadData (BufferSize))
				return null;
			fixed (byte* buffer = reader.buffer) {
				reader.SetBufferPointer (buffer);
				return new BufferHeader (reader);
			}
		}
	}
	
}
