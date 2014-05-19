//
// LogFileReader.cs
//
// Authors:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
// Contains code gleaned from:
// ----
// UTF8Encoding.cs - Implementation of the "System.Text.UTF8Encoding" class.
// Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// ----
//
// Copyright (C) 2011 Xamarin Inc. (http://www.xamarin.com)
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
	unsafe class CachedBinaryReader : IDisposable
	{
		readonly Stream stream;

		internal byte[] buffer = new byte [ushort.MaxValue];
		byte* curPtr;
		byte* endPtr;

		int buffered_size;

		public CachedBinaryReader (string filename)
		{
			stream = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public CachedBinaryReader (Stream s)
		{
			stream = s;
		}

		public void SetBufferPointer (byte* bufferPointer)
		{
			curPtr = bufferPointer;
			endPtr = bufferPointer + buffered_size;
		}

		public bool LoadData (int size)
		{
			if (!IsBufferEmpty)
				throw new InvalidProgramException ("LoadData called, but buffer wasn't empty - potential data loss!");
			long str_pos = stream.Position;
			if (str_pos + size > stream.Length)
				return false;

			if (buffer.Length < size)
				buffer = new byte [size];

			if (stream.Read (buffer, 0, size) != size) {
				stream.Position = str_pos;
				return false;
			}
			buffered_size = size;

			return true;
		}

		public bool IsBufferEmpty {
			get {
				return curPtr >= endPtr;
			}
		}

		public bool IsEof {
			get { return stream.Position == stream.Length; }
		}

		public long Position {
			get { return stream.Position; }
			set { stream.Position = value; buffered_size = 0; curPtr = endPtr = (byte*)0; }
		}

		public long Length {
			get { return stream.Length; }
		}

		public byte ReadByte ()
		{
			return *curPtr++;
		}

		public ushort ReadUInt16 ()
		{
			ushort res;
			res = (ushort) (*curPtr++ | (*curPtr++ << 8));
			return res;
		}

		public int ReadInt32 ()
		{
			int res;
			res = (*curPtr++ | (*curPtr++ << 8) | (*curPtr++ << 16) | (*curPtr++ << 24));
			return res;
		}

		public long ReadInt64 ()
		{
			uint ret_low = (((uint)*curPtr++) | (((uint)*curPtr++) << 8) | (((uint)*curPtr++) << 16) | (((uint)*curPtr++) << 24));
			uint ret_high = (((uint)*curPtr++) | (((uint)*curPtr++) << 8) | (((uint)*curPtr++) << 16) | (((uint)*curPtr++) << 24));
			return (long) ((((ulong) ret_high) << 32) | ret_low);
		}

		public ulong ReadUInt64 ()
		{
			uint ret_low = (((uint)*curPtr++) | (((uint)*curPtr++) << 8) | (((uint)*curPtr++) << 16) | (((uint)*curPtr++) << 24));
			uint ret_high = (((uint)*curPtr++) | (((uint)*curPtr++) << 8) | (((uint)*curPtr++) << 16) | (((uint)*curPtr++) << 24));
			return (((ulong) ret_high) << 32) | ret_low;
		}

		public ulong ReadULeb128 ()
		{
			ulong result = 0;
			int shift = 0;
			while (true) {
				byte b = *curPtr++;
				result |= ((ulong)(b & 0x7f)) << shift;
				if ((b & 0x80) != 0x80)
					break;
				shift += 7;
			}
			return result;
		}

		public long ReadSLeb128 ()
		{
			long result = 0;
			int shift = 0;
			while (true) {
				byte b = *curPtr++;
				result |= ((long)(b & 0x7f)) << shift;
				shift += 7;
				if ((b & 0x80) != 0x80) {
					if (shift < sizeof(long) * 8 && (b & 0x40) == 0x40)
						result |= -(1L << shift);
					break;
				}
			}
			return result;
		}

		public double ReadDouble ()
		{
			byte[] buffer = new byte[8];
			if (BitConverter.IsLittleEndian) {
				for (int i = 0; i < 8; i++)
					buffer [i] = ReadByte ();
			} else {
				for (int i = 7; i >= 0; i--)
					buffer [i] = ReadByte ();
			}
			return BitConverter.ToDouble (buffer, 0);
		}

		public unsafe string ReadNullTerminatedString ()
		{
			// unsafe version uses UTF8Encoding code

			byte ch;
			byte* startPtr = curPtr;
			while ((ch = *curPtr) != 0) {
				if (ch < 0x80) {
					curPtr++;
				} else if ((ch & 0xE0) == 0xC0) {
					// Double-byte UTF-8 character.
					curPtr += 2;
				} else if ((ch & (uint)0xF0) == (uint)0xE0) {
					// Three-byte UTF-8 character.
					curPtr += 3;
				} else if ((ch & (uint)0xF8) == (uint)0xF0) {
					// Four-byte UTF-8 character.
					curPtr += 4;
				} else if ((ch & (uint)0xFC) == (uint)0xF8) {
					// Five-byte UTF-8 character.
					curPtr += 5;
				} else if ((ch & (uint)0xFE) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					curPtr += 6;
				} else {
					// Invalid UTF-8 start character.
					throw new InvalidProgramException ("Invalid UTF8 sequence found!");
				}
			}
			var length = (int)(curPtr - startPtr);
			if (length == 0) {
				curPtr ++;
				return string.Empty;
			}

			var chars = new char[length];
			fixed (char* charPtr = chars)
				InternalGetChars (startPtr, curPtr, charPtr, length);
			curPtr ++;
			return new string (chars);
		}

		unsafe static int InternalGetChars (byte* start, byte* end, char* chars, int charCount)
		{
			uint leftOverBits = 0;
			uint leftOverCount = 0;
			int length = charCount;
			int posn = 0;

			if (leftOverCount == 0) {
				for (; start < end; start++, posn++) {
					if (*start < 0x80) {
						chars [posn] = (char)*start;
					} else {
						break;
					}
				}
			}

			// Convert the bytes into the output buffer.
			uint ch;
			uint leftBits = leftOverBits;
			uint leftSoFar = (leftOverCount & (uint)0x0F);
			uint leftSize = ((leftOverCount >> 4) & (uint)0x0F);

			for(; start < end; start++) {
				// Fetch the next character from the byte buffer.
				ch = (uint)*start;
				if (leftSize == 0) {
					// Process a UTF-8 start character.
					if (ch < (uint)0x0080) {
						// Single-byte UTF-8 character.
						chars[posn++] = (char)ch;
					} else if ((ch & (uint)0xE0) == (uint)0xC0) {
						// Double-byte UTF-8 character.
						leftBits = (ch & (uint)0x1F);
						leftSoFar = 1;
						leftSize = 2;
					} else if ((ch & (uint)0xF0) == (uint)0xE0) {
						// Three-byte UTF-8 character.
						leftBits = (ch & (uint)0x0F);
						leftSoFar = 1;
						leftSize = 3;
					} else if ((ch & (uint)0xF8) == (uint)0xF0) {
						// Four-byte UTF-8 character.
						leftBits = (ch & (uint)0x07);
						leftSoFar = 1;
						leftSize = 4;
					} else if ((ch & (uint)0xFC) == (uint)0xF8) {
						// Five-byte UTF-8 character.
						leftBits = (ch & (uint)0x03);
						leftSoFar = 1;
						leftSize = 5;
					} else if ((ch & (uint)0xFE) == (uint)0xFC) {
						// Six-byte UTF-8 character.
						leftBits = (ch & (uint)0x03);
						leftSoFar = 1;
						leftSize = 6;
					} else {
						// Invalid UTF-8 start character.
						throw new InvalidProgramException ("Invalid UTF8 sequence found!");
					}
				} else {
					// Process an extra byte in a multi-byte sequence.
					if ((ch & (uint)0xC0) == (uint)0x80) {
						leftBits = ((leftBits << 6) | (ch & (uint)0x3F));
						if (++leftSoFar >= leftSize) {
							// We have a complete character now.
							if (leftBits < (uint)0x10000) {
								// is it an overlong ?
								bool overlong = false;
								switch (leftSize) {
								case 2:
									overlong = (leftBits <= 0x7F);
									break;
								case 3:
									overlong = (leftBits <= 0x07FF);
									break;
								case 4:
									overlong = (leftBits <= 0xFFFF);
									break;
								case 5:
									overlong = (leftBits <= 0x1FFFFF);
									break;
								case 6:
									overlong = (leftBits <= 0x03FFFFFF);
									break;
								}
								if (overlong) {
									throw new InvalidProgramException ("Invalid UTF8 sequence found!");
								}
								if ((leftBits & 0xF800) == 0xD800) {
									// UTF-8 doesn't use surrogate characters
									throw new InvalidProgramException ("Invalid UTF8 sequence found!");
								}
								chars [posn++] = (char)leftBits;
							} else if (leftBits < (uint)0x110000) {
								if ((posn + 2) > length) {
									throw new InvalidProgramException ("Invalid UTF8 sequence found!");
								}
								leftBits -= (uint)0x10000;
								chars[posn++] = (char)((leftBits >> 10) +
								                       (uint)0xD800);
								chars[posn++] =
									(char)((leftBits & (uint)0x3FF) + (uint)0xDC00);
							} else {
								throw new InvalidProgramException ("Invalid UTF8 sequence found!");
							}
							leftSize = 0;
						}
					}
				}
			}
			// Return the final length to the caller.
			return posn;
		}

		public void Close ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			stream.Dispose ();
		}
	}
}

