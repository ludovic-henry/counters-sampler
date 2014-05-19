//
// EventType.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//		 Stephen Shaw <shaw@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (http://xamarin.com)
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

namespace XamarinProfiler.Core.Reader
{
	/***
	 * proflog.h
	 *
	 * #define BUF_ID 0x4D504C01
	 * #define LOG_HEADER_ID 0x4D505A01
	 * #define LOG_VERSION_MAJOR 0
	 * #define LOG_VERSION_MINOR 4
	 * #define LOG_DATA_VERSION 4
	 *  *
	 *  * Changes in data versions:
	 *  * version 2: added offsets in heap walk
	 *  * version 3: added GC roots
	 *  * version 4: added sample/statistical profiling
	 *  *
	 ***/

	/// <summary>
	/// Profiler Log Event Types (from proflog.h)
	/// </summary>
	static class EventType
	{
		public const byte Alloc      = 0;
		public const byte Gc         = 1;
		public const byte Metadata   = 2;
		public const byte Method     = 3;
		public const byte Exception  = 4;
		public const byte Monitor    = 5;
		public const byte Heap       = 6;
		public const byte Sample     = 7;
		public const byte Counters   = 8;
	}

	/// <summary>
	/// Extended type for TYPE_HEAP (from proflog.h)
	/// </summary>
	static class TypeHeap
	{
		public const byte Start  = 0 << 4;
		public const byte End    = 1 << 4;
		public const byte Object = 2 << 4;
		public const byte Root   = 3 << 4;
	}

	/// <summary>
	/// extended type for TYPE_METADATA (from proflog.h)
	/// </summary>
	static class TypeMetadata
	{
		public const byte Class    = 1;
		public const byte Image    = 2;
		public const byte Assembly = 3;
		public const byte Domain   = 4;
		public const byte Thread   = 5;
	}

	/// <summary>
	/// extended type for TYPE_GC (from proflog.h)
	/// </summary>
	static class TypeGc
	{
		public const byte Event           = 1 << 4;
		public const byte Resize          = 2 << 4;
		public const byte Move            = 3 << 4;
		public const byte HandleCreated   = 4 << 4;
		public const byte HandleDestroyed = 5 << 4;
	}

	/// <summary>
	/// extended type for TYPE_METHOD  (from proflog.h)
	/// </summary>
	static class TypeMethod
	{
		public const byte Leave     = 1 << 4;
		public const byte Enter     = 2 << 4;
		public const byte ExcLeave  = 3 << 4;
		public const byte Jit       = 4 << 4;
	}

	/// <summary>
	/// extended type for TYPE_EXCEPTION  (from proflog.h)
	/// </summary>
	static class TypeException
	{
		public const byte Throw = 0 << 4;
		public const byte Clause = 1 << 4;
		public const byte BacktraceBit = 1 << 7;
	}

	/// <summary>
	/// extended type for TYPE_ALLOC (from proflog.h)
	/// </summary>
	static class TypeAlloc
	{
		public const byte BacktraceBit  = 1 << 4;
	}

	/// <summary>
	/// extended type for TYPE_MONITOR (from proflog.h)
	/// </summary>
	static class TypeMonitor
	{
		public const int ProfilerMonitorContention = 1;
		public const int ProfilerMonitorDone       = 2;
		public const int ProfilerMonitorFail       = 3;
		
		public const byte BacktraceBit = 1 << 7;
	}

	/// <summary>
	/// extended type for TYPE_SAMPLE (from proflog.h)
	/// </summary>
	static class TypeSample
	{
		public const byte Hit          = 0 << 4;
		public const byte USym         = 1 << 4;
		public const byte UBin         = 2 << 4;
		public const byte CountersDesc = 3 << 4;
		public const byte Counters     = 4 << 4;
	}

	/// <summary>
	/// counters samples types (from mono-counters.h)
	/// </summary>
	static class TypeCountersSample
	{
		public const ulong Int = 0;
		public const ulong UInt = 1;
		public const ulong Word = 2;
		public const ulong Long = 3;
		public const ulong ULong = 4;
		public const ulong Double = 5;
		public const ulong String = 6;
		public const ulong TimeInterval = 7;
	}
}