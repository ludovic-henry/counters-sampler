// 
// Header.cs
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
	/// <summary>
	/// header format:
	/// [id: 4 bytes] constant value: LOG_HEADER_ID
	/// [major: 1 byte] [minor: 1 byte] major and minor version of the log profiler
	/// [format: 1 byte] version of the data format for the rest of the file
	/// [ptrsize: 1 byte] size in bytes of a pointer in the profiled program
	/// [startup time: 8 bytes] time in milliseconds since the unix epoch when the program started
	/// [timer overhead: 4 bytes] approximate overhead in nanoseconds of the timer
	/// [flags: 4 bytes] file format flags, should be 0 for now
	/// [pid: 4 bytes] pid of the profiled process
	/// [port: 2 bytes] tcp port for server if != 0
	/// [sysid: 2 bytes] operating system and architecture identifier
	/// </summary>
	public class LogHeader
	{
		// LOG_HEADER_ID (mono/mono/profiler/proflog.h)
		const int LogHeaderId = 0x4D505A01;
		const int HeaderSize = 32;

		/// <summary>
		/// Version of the log profiler
		/// </summary>
		public readonly Version Version;

		/// <summary>
		/// Version of the data format for the rest of the file
		/// </summary>
		public readonly byte Format;

		/// <summary>
		/// size in bytes of a pointer in the profiled program
		/// </summary>
		public readonly byte PtrSize;

		/// <summary>
		/// time in milliseconds since the unix epoch when the program started
		/// </summary>
		public readonly long StartupTime;

		/// <summary>
		/// approximate overhead in nanoseconds of the timer
		/// </summary>
		public readonly int TimerOverhead;

		/// <summary>
		/// file format flags, should be 0 for now
		/// </summary>
		public readonly int Flags;

		/// <summary>
		/// pid of the profiled process
		/// </summary>
		public readonly int Pid;

		/// <summary>
		/// tcp port for server if != 0
		/// </summary>
		public readonly int Port;

		/// <summary>
		/// operating system and architecture identifier
		/// </summary>
		public readonly int SysId;
		
		LogHeader (CachedBinaryReader reader)
		{
			var id = reader.ReadInt32 ();
			if (id != LogHeaderId)
				throw new InvalidOperationException ("Id doesn't match.");

			var versionMajor = reader.ReadByte ();
			var versionMinor = reader.ReadByte ();
			Version = new Version (versionMajor, versionMinor);

			Format = reader.ReadByte ();
			PtrSize = reader.ReadByte ();
			StartupTime = reader.ReadInt64 ();
			TimerOverhead = reader.ReadInt32 ();
			Flags = reader.ReadInt32 ();
			Pid = reader.ReadInt32 ();
			Port = reader.ReadUInt16 ();
			SysId = reader.ReadUInt16 ();
		}

		internal unsafe static LogHeader Read (CachedBinaryReader reader)
		{
			if (!reader.LoadData (HeaderSize))
				return null;
			fixed (byte* buffer = reader.buffer) {
				reader.SetBufferPointer (buffer);
				return new LogHeader (reader);
			}
		}
	}
}