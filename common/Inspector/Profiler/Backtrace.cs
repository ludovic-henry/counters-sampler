//
// Backtrace.cs
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

namespace XamarinProfiler.Core.Reader
{
	/// <summary>
	///  backtrace format:
	///  [flags: uleb128] must be 0
	///  [num: uleb128] number of frames following
	///  [frame: sleb128]* num MonoMethod pointers as differences from ptr_base
	/// </summary>
	public class Backtrace
	{
		/// <summary>
		/// MonoMethod pointers as differences from ptr_base
		/// </summary>
		public long[] Frame;

		Backtrace () {}
		
		internal static Backtrace Read(CachedBinaryReader reader)
		{
			var flags = reader.ReadULeb128 ();
			if (flags != 0)
				throw new Exception ("Error while reading Backtrace!");
			ulong num = reader.ReadULeb128 ();
			var frame = new long[num];
			for (ulong i = 0; i < num; i++) {
				frame [i] = reader.ReadSLeb128 ();
			}
			return new Backtrace { Frame = frame };
		}
	}
}
