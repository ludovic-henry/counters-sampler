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
using System.Collections.Generic;

namespace XamarinProfiler.Core.Reader
{
	// gc_event_name (profiler/decode.c)
	// defined in metadata/profiler.h
	public enum MonoGCEvent
	{
		Start,
		MarkStart,
		MarkEnd,
		ReclaimStart,
		ReclaimEnd,
		End,
		PreStopWorld,
		PostStopWorld,
		PreStartWorld,
		PostStartWorld
	}

	// sample_type_name (profiler/decode.c)
	// Defined in profiler/proflog.h
	// TODO: we are missing SAMPLE_LAST
	public enum SampleType
	{
		Cycles = 1,
		Instructions = 2,
		CacheMisses = 3,
		CacheRefs = 4,
		Branches = 5,
		BranchMisses = 6
	}

	// get_root_name (profiler/decode.c)
	// Defined in metadata/profiler.h
	public enum MonoProfileGCRootType : ulong
	{
		Pinning = 1 << 8,
		WeakRef = 2 << 8,
		Interior = 4 << 8,
		/* the above are flags, the type is in the low 2 bytes */
		Stack = 0,
		Finalizer = 1,
		Handle = 2,
		Other = 3,
		Misc = 4, /* Misc: could be stack, handle, etc. */
		TypeMask = 0xff
	}

	/// <summary>
	/// The event listener observes all events read in a buffer.
	/// </summary>
	public abstract class EventListener
	{
		BufferDescriptor currentBuffer;
		Dictionary<ulong, object> CountersSampleValues = new Dictionary<ulong, object>();

		/// <summary>
		/// Gets or sets the current buffer.
		/// </summary>
		public virtual BufferDescriptor CurrentBuffer {
			get {
				return currentBuffer;
			}
			set {
				currentBuffer = value;
				if (currentBuffer != null) {
					Time = currentBuffer.Header.TimeBase;
					MethodBase = currentBuffer.Header.MethodBase;
				}
				
				OnBufferSet (EventArgs.Empty);
			}
		}

		/// <summary>
		/// Occurs when buffer set.
		/// </summary>
		public event EventHandler BufferSet;

		protected virtual void OnBufferSet (EventArgs e)
		{
			var handler = BufferSet;
			if (handler != null)
				handler (this, e);
		}

		/// <summary>
		/// The current time. (The buffer updates it)
		/// </summary>
		public ulong Time;

		/// <summary>
		/// The current method base. (The buffer updates it)
		/// </summary>
		public long MethodBase;

		#region TYPE_EXCEPTION Events
		/// <summary>
		/// [time diff: uleb128] nanoseconds since last timing
		/// if exinfo.low3bits == TypeException.Clause
		///     [clause type: uleb128] finally/catch/fault/filter
		///     [clause num: uleb128] the clause number in the method header
		///     [method: sleb128] MonoMethod* as a pointer difference from the last such
		///                       pointer or the buffer method_base
		/// </summary>
		/// <param name="clauseType">finally/catch/fault/filter</param>
		/// <param name="clauseNum">the clause number in the method header</param>
		public virtual void HandleExceptionClause (ulong clauseType, ulong clauseNum)
		{
		}

		/// <summary>
		///  [time diff: uleb128] nanoseconds since last timing
		///  if exinfo.low3bits == TypeException.Throw
		///      [object: sleb128] the object that was thrown as a difference from obj_base
		///      If the TypeException.ExceptionBt flag is set, a backtrace follows.
		/// </summary>
		/// <param name="obj">the object that was thrown as a difference from obj_base If the TypeException.ExceptionBt flag is set, a backtrace follows.</param>
		/// <param name="backtrace">The backtrace</param>
		public virtual void HandleExceptionThrow (long obj, Backtrace backtrace)
		{
		}
		#endregion

		#region TYPE_GC Events
		/// <summary>
		/// [time diff: uleb128] nanoseconds since last timing
		/// if exinfo == TypeGc.Resize
		///     [heap_size: uleb128] new heap size
		/// </summary>
		/// <param name="heapSize">The new heap size.</param>
		public virtual void HandleResizeGc (ulong heapSize)
		{
		}

		/// <summary>
		/// [time diff: uleb128] nanoseconds since last timing
		/// if exinfo == TypeGc.Event
		///     [event type: uleb128] GC event (MONO_GC_EVENT_* from profiler.h)
		///     [generation: uleb128] GC generation event refers to
		/// </summary>
		/// <param name="gcEventType">The GC event</param>
		/// <param name="generation">GC generation event refers to</param>
		public virtual void HandleGc (MonoGCEvent gcEventType, ulong generation)
		{
		}

		/// <summary>
		/// [time diff: uleb128] nanoseconds since last timing
		/// if exinfo == TypeGc.HandleCreated
		///     [handle_type: uleb128] GC handle type (System.Runtime.InteropServices.GCHandleType)
		///                            upper bits reserved as flags
		///     [handle: uleb128] GC handle value
		///     [objaddr: sleb128] object pointer differences from obj_base
		/// </summary>
		/// <param name="handleType">The GC handle type (System.Runtime.InteropServices.GCHandleType)</param>
		/// <param name="handle">The GC handle value</param>
		/// <param name="objAddr">The object pointer differences from obj_base.</param>
		public virtual void HandleHandleCreatedGc (System.Runtime.InteropServices.GCHandleType handleType, ulong handle, long objAddr)
		{
		}

		/// <summary>
		/// [time diff: uleb128] nanoseconds since last timing
		/// if exinfo == TypeGc.HandleDestroyed
		///     [handle_type: uleb128] GC handle type (System.Runtime.InteropServices.GCHandleType)
		///                            upper bits reserved as flags
		///     [handle: uleb128] GC handle value
		/// </summary>
		/// <param name="handleType">GC handle type (System.Runtime.InteropServices.GCHandleType)</param>
		/// <param name="handle">GC handle value</param>
		public virtual void HandleHandleDestroyedGc (System.Runtime.InteropServices.GCHandleType handleType, ulong handle)
		{
		}

		/// <summary>
		/// [time diff: uleb128] nanoseconds since last timing
		/// if exinfo == TypeGc.Move
		///    [num_objects: uleb128] number of object moves that follow
		///    [objaddr: sleb128]+ num_objects object pointer differences from obj_base
		///                        num is always an even number: the even items are the old
		///                        addresses, the odd numbers are the respective new object addresses
		/// </summary>
		/// <param name="objAddr">The num_objects object pointer differences from obj_base.</param>
		public virtual void HandleMoveGc (long[] objAddr)
		{
		}
		#endregion

		#region TYPE_ALLOC Events
		/// <summary>
		/// exinfo: flags: TYPE_ALLOC_BT
		/// [time diff: uleb128] nanoseconds since last timing
		/// [ptr: sleb128] class as a byte difference from ptr_base
		/// [obj: sleb128] object address as a byte difference from obj_base
		/// [size: uleb128] size of the object in the heap
		/// If the TYPE_ALLOC_BT flag is set, a backtrace follows.
		/// </summary>
		/// <param name="ptr">The class as a byte difference from ptr_base.</param>
		/// <param name="obj">The object address as a byte difference from obj_base.</param>
		/// <param name="size">The size of the object in the heap.</param>
		/// <param name="backtrace">The backtrace.</param>
		public virtual void HandleAlloc (long ptr, long obj, ulong size, Backtrace backtrace)
		{
		}
		#endregion

		#region TYPE_METADATA Events
		/// <summary>
		/// exinfo: flags: TYPE_LOAD_ERR
		/// [time diff: uleb128] nanoseconds since last timing
		/// [mtype: byte] metadata type, one of: TYPE_CLASS, TYPE_IMAGE, TYPE_ASSEMBLY, TYPE_DOMAIN,
		/// TYPE_THREAD
		/// [pointer: sleb128] pointer of the metadata type depending on mtype
		/// if mtype == TYPE_CLASS
		///     [image: sleb128] MonoImage* as a pointer difference from ptr_base
		///     [flags: uleb128] must be 0
		///     [name: string] full class name
		/// </summary>
		/// <param name="pointer">The pointer of the metadata type depending on mtype.</param>
		/// <param name="image">MonoImage* as a pointer difference from ptr_base</param>
		/// <param name="name">full class/image file or thread name </param>
		public virtual void HandleMetaDataClass (long pointer, long image, string name)
		{
		}

		/// <summary>
		/// exinfo: flags: TYPE_LOAD_ERR
		/// [time diff: uleb128] nanoseconds since last timing
		/// [mtype: byte] metadata type, one of: TYPE_CLASS, TYPE_IMAGE, TYPE_ASSEMBLY, TYPE_DOMAIN,
		/// TYPE_THREAD
		/// [pointer: sleb128] pointer of the metadata type depending on mtype
		/// if mtype == TYPE_IMAGE
		///     [flags: uleb128] must be 0
		///     [name: string] image file name
		/// </summary>
		/// <param name="pointer">The pointer of the metadata type depending on mtype.</param>
		/// <param name="name">full class/image file or thread name</param>
		public virtual void HandleMetaDataImage (long pointer, string name)
		{
		}

		/// <summary>
		/// exinfo: flags: TYPE_LOAD_ERR
		/// [time diff: uleb128] nanoseconds since last timing
		/// [mtype: byte] metadata type, one of: TYPE_CLASS, TYPE_IMAGE, TYPE_ASSEMBLY, TYPE_DOMAIN,
		/// TYPE_THREAD
		/// [pointer: sleb128] pointer of the metadata type depending on mtype
		/// if mtype == TYPE_THREAD
		///     [flags: uleb128] must be 0
		///     [name: string] thread name
		/// </summary>
		/// <param name="pointer">The pointer of the metadata type depending on mtype</param>
		/// <param name="name">full class/image file or thread name</param>
		public virtual void HandleMetaDataThread (long pointer, string name)
		{
		}
		#endregion

		#region TYPE_METHOD Events
		/// <summary>
		/// type method format:
		/// type: TYPE_METHOD
		/// exinfo: one of: TypeMethod.Leave, TypeMethod.Enter, TypeMethod.ExcLeave, TypeMethod.Jit
		/// [time diff: uleb128] nanoseconds since last timing
		/// [method: sleb128] MonoMethod* as a pointer difference from the last such
		///                   pointer or the buffer method_base
		/// </summary>
		public virtual void HandleMethodEnter ()
		{
		}

		/// <summary>
		/// type method format:
		/// type: TYPE_METHOD
		/// exinfo: one of: TypeMethod.Leave, TypeMethod.Enter, TypeMethod.ExcLeave, TypeMethod.Jit
		/// [time diff: uleb128] nanoseconds since last timing
		/// [method: sleb128] MonoMethod* as a pointer difference from the last such
		///                   pointer or the buffer method_base
		/// </summary>
		public virtual void HandleMethodExcLeave ()
		{
		}

		/// <summary>
		/// type method format:
		/// type: TYPE_METHOD
		/// exinfo: one of: TypeMethod.Leave, TypeMethod.Enter, TypeMethod.ExcLeave, TypeMethod.Jit
		/// [time diff: uleb128] nanoseconds since last timing
		/// [method: sleb128] MonoMethod* as a pointer difference from the last such
		///                   pointer or the buffer method_base
		/// if exinfo == TypeMethod.Jit
		///     [code address: sleb128] pointer to the native code as a diff from ptr_base
		///     [code size: uleb128] size of the generated code
		///     [name: string] full method name
		/// </summary>
		/// <param name="codeAddress">The pointer to the native code as a diff from ptr_base.</param>
		/// <param name="codeSize">The size of the generated code.</param>
		/// <param name="name">The full method name.</param>
		public virtual void HandleMethodJit (long codeAddress, ulong codeSize, string name)
		{
		}

		/// <summary>
		/// type method format:
		/// type: TYPE_METHOD
		/// exinfo: one of: TypeMethod.Leave, TypeMethod.Enter, TypeMethod.ExcLeave, TypeMethod.Jit
		/// [time diff: uleb128] nanoseconds since last timing
		/// [method: sleb128] MonoMethod* as a pointer difference from the last such
		///                   pointer or the buffer method_base
		/// </summary>
		public virtual void HandleMethodLeave ()
		{
		}
		#endregion

		#region TYPE_MONITOR Events
		/// <summary>
		/// exinfo: TYPE_MONITOR_BT flag and one of: MONO_PROFILER_MONITOR_(CONTENTION|FAIL|DONE)
		/// [time diff: uleb128] nanoseconds since last timing
		/// [object: sleb128] the lock object as a difference from obj_base
		/// if exinfo.low3bits == MONO_PROFILER_MONITOR_CONTENTION
		/// If the TYPE_MONITOR_BT flag is set, a backtrace follows.
		/// </summary>
		/// <param name="obj">the lock object as a difference from obj_base</param>
		/// <param name="backtrace">The backtrace</param>
		public virtual void HandleMonitor (long obj, Backtrace backtrace)
		{
		}
		#endregion

		#region TYPE_SAMPLE Events
		/// <summary>
		/// if exinfo == TypeSample.Hit
		/// [sample_type: uleb128] type of sample (SAMPLE_*)
		/// [timestamp: uleb128] nanoseconds since startup (note: different from other timestamps!)
		/// [count: uleb128] number of following instruction addresses
		/// [ip: sleb128]* instruction pointer as difference from ptr_base
		/// </summary>
		/// <param name="type">The type of sample (SAMPLE_*).</param>
		/// <param name="timeStamp">Nanoseconds since startup (note: different from other timestamps!).</param>
		/// <param name="instructionPointer">The instruction pointer as difference from ptr_base</param>
		public virtual void HandleSampleHit (SampleType type, ulong timeStamp, long[] instructionPointer)
		{
		}

		/// <summary>
		/// if exinfo == TypeSample.UBin
		///     [time diff: uleb128] nanoseconds since last timing
		///     [address: sleb128] address where binary has been loaded
		///     [offset: uleb128] file offset of mapping (the same file can be mapped multiple times)
		///     [size: uleb128] memory size
		///     [name: string] binary name
		/// </summary>
		/// <param name="address">The address where binary has been loaded.</param>
		/// <param name="offset">The file offset of mapping (the same file can be mapped multiple times).</param>
		/// <param name="size">The memory size</param>
		/// <param name="name">The binary name</param>
		public virtual void HandleSampleUBin (long address, ulong offset, ulong size, string name)
		{
		}	

		/// <summary>
		/// if exinfo == TypeSample.USym
		///     [address: sleb128] symbol address as a difference from ptr_base
		///     [size: uleb128] symbol size (may be 0 if unknown)
		///     [name: string] symbol name
		/// </summary>
		/// <param name="address">The symbol address as a difference from ptr_base.</param>
		/// <param name="size">The symbol size (may be 0 if unknown).</param>
		/// <param name="name">The symbol name.</param>
		public virtual void HandleSampleUSym (long address, ulong size, string name)
		{
		}
		#endregion

		#region TYPE_HEAP Events
		/// <summary>
		/// if exinfo == TypeSample.Counters
		///     [len: uleb128] number of counters
		///     for i = 0 to len
		///         [section: uleb128] section name of counter
		///         [name: string] name of counter
		///         [type: uleb128] type name of counter
		///         [unit: uleb128] unit name of counter
		///         [variance: uleb128] variance name of counter
		///         [index: uleb128] unique index of counter
		/// </summary>
		/// <param name="counters">List of counters, without value.</param>
		public virtual void HandleSampleCountersDesc (List<Tuple<ulong, string, ulong, ulong, ulong, ulong>> counters)
		{
		}

		/// <summary>
		/// if exinfo == TypeSample.Counters
		///     [timestamp: uleb128] time since first sampling
		///     while true
		///         [index: uleb128] unique index of counter
		///         if index == 0
		///             break
		///         [type: uleb128] type of counter value
		///         if type == string
		///             [isnull: uleb128]
		///             if isnull == 1:
		///                 [value: string] counter value
		///         else
		///             [value: uleb128/sleb128/double] counter value, type determined by using type
		/// </summary>
		/// <param name="timestamp">Sample timestamp</param>
		/// <param name="values">List of counters values. Counters are identified by their index.</param>
		public virtual void HandleSampleCounters (ulong timestamp, List<Tuple<ulong, ulong, object>> values)
		{
		}

		/// <summary>
		/// if exinfo == TypeHeap.Start
		///     [time diff: uleb128] nanoseconds since last timing
		/// </summary>
		public virtual void HandleHeapStart ()
		{
		}

		/// <summary>
		/// if exinfo == TypeHeap.End
		///     [time diff: uleb128] nanoseconds since last timing
		/// </summary>
		public virtual void HandleHeapEnd ()
		{
		}

		/// <summary>
		/// if exinfo == TypeHeap.Object
		///     [object: sleb128] the object as a difference from obj_base
		///     [class: sleb128] the object MonoClass* as a difference from ptr_base
		///     [size: uleb128] size of the object on the heap
		///     [num_refs: uleb128] number of object references
		///     if (format version > 1) each referenced objref is preceded by a
		///                             uleb128 encoded offset: the first offset is from the object address
		///                             and each next offset is relative to the previous one
		///     [objrefs: sleb128]+ object referenced as a difference from obj_base
		///                         The same object can appear multiple times, but only the first time
		///                         with size != 0: in the other cases this data will only be used to
		///                         provide additional referenced objects.
		/// </summary>
		/// <param name="obj">the object as a difference from obj_base</param>
		/// <param name="clas">the object MonoClass* as a difference from ptr_base</param>
		/// <param name="size">size of the object on the heap</param>
		/// <param name="relOffset">The relative offsets.</param>
		/// <param name="objRefs">object referenced as a difference from obj_base</param>
		public virtual void HandleHeapObject (long obj, long clas, ulong size, ulong[] relOffset, long[] objRefs)
		{
		}

		/// <summary>
		/// if exinfo == TypeHeap.Root
		///     [num_roots: uleb128] number of root references
		///     [num_gc: uleb128] number of major gcs
		///     [object: sleb128] the object as a difference from obj_base
		///     [root_type: uleb128] the root_type: MonoProfileGCRootType (profiler.h)
		///     [size: uleb128] size of the object on the heap
		///     [extra_info: uleb128] the extra_info value
		///     object, root_type and extra_info are repeated num_roots times
		/// </summary>
		/// <param name="numRoots">The number of root references</param>
		/// <param name="numGc">The number of major gcs.</param>
		/// <param name="obj">The object as a difference from obj_base.</param>
		/// <param name="rootType">The root_type: MonoProfileGCRootType (profiler.h).</param>
		/// <param name="extraInfo">The extra info value.</param>
		public virtual void HandleHeapRoot (ulong numRoots, ulong numGc, long[] obj, MonoProfileGCRootType[] rootType, ulong[] extraInfo)
		{
		}
		#endregion

		#region Other Event stuff?
		public virtual void HandleBufferStartRead ()
		{
		}
		
		public virtual void HandleBufferEndRead ()
		{
		}
		#endregion

		long FixPointer (LogHeader logHeader, long ptr)
		{
			if (logHeader.PtrSize == 8)
				return ptr;
			return ptr & 0xffffffffL;
		}

		internal void ReadBufferEvents (LogHeader logHeader, BufferDescriptor buffer, CachedBinaryReader reader)
		{
			CurrentBuffer = buffer;
			HandleBufferStartRead ();
			
			while (!reader.IsBufferEmpty) {
				byte info = reader.ReadByte ();
				var type = (byte)(info & 0xF);
				byte extendedInfo = (byte)(info & 0xF0);
				ulong timeDiff;
				switch (type) {
				case EventType.Method: {
					timeDiff = reader.ReadULeb128 ();
					Time += timeDiff;
					buffer.RecordTime (Time);
					switch (extendedInfo) {
					case TypeMethod.Leave:
						var method = reader.ReadSLeb128 ();
						MethodBase += method;
						HandleMethodLeave ();
						break;
					case TypeMethod.Enter:
						method = reader.ReadSLeb128 ();
						MethodBase += method;
						HandleMethodEnter ();
						break;
					case TypeMethod.ExcLeave:
						method = reader.ReadSLeb128 ();
						MethodBase += method;
						HandleMethodExcLeave ();
						break;
					case TypeMethod.Jit:
						method = reader.ReadSLeb128 ();
						MethodBase += method;
						var codeAddress = FixPointer (logHeader, buffer.Header.PtrBase + reader.ReadSLeb128 ());
						var codeSize = reader.ReadULeb128 ();
						var name = reader.ReadNullTerminatedString ();
						HandleMethodJit (codeAddress, codeSize, name);
						break;
					default:
						throw new InvalidOperationException ("Unknown method event type:" + extendedInfo);
					}
					break;
				}
				case EventType.Alloc: {
					timeDiff = reader.ReadULeb128 ();
					Time += timeDiff;
					buffer.RecordTime (Time);
					
					var ptr = reader.ReadSLeb128 ();
					var obj = reader.ReadSLeb128 ();
					var size = reader.ReadULeb128 ();
					Backtrace backtrace;
					if ((extendedInfo & TypeAlloc.BacktraceBit) == TypeAlloc.BacktraceBit) {
						backtrace = Backtrace.Read (reader);
					} else {
						backtrace = null;
					}
					HandleAlloc (ptr, obj, size, backtrace);
					break;
				}
				case EventType.Gc: {
					timeDiff = reader.ReadULeb128 ();
					Time += timeDiff;
					buffer.RecordTime (Time);

					switch (extendedInfo) {
					case TypeGc.Event:

						var gcEventType = (MonoGCEvent)reader.ReadULeb128 ();
						var generation = reader.ReadULeb128 ();
						HandleGc (gcEventType, generation);
						break;
					case TypeGc.Resize:
						var heapSize = reader.ReadULeb128 ();
						HandleResizeGc (heapSize);
						break;
					case TypeGc.Move:
						ulong num = reader.ReadULeb128 ();
						var objAddr = new long[num];
						for (ulong i = 0; i < num; i++) {
							objAddr [i] = reader.ReadSLeb128 ();
						}
						HandleMoveGc (objAddr);
						break;
					case TypeGc.HandleCreated:
						var handleType = (System.Runtime.InteropServices.GCHandleType)reader.ReadULeb128 ();
						var handle = reader.ReadULeb128 ();
						var obja = reader.ReadSLeb128 ();
						HandleHandleCreatedGc (handleType, handle, obja);
						break;
					case TypeGc.HandleDestroyed:
						handleType = (System.Runtime.InteropServices.GCHandleType)reader.ReadULeb128 ();
						handle = reader.ReadULeb128 ();
						HandleHandleDestroyedGc (handleType, handle);
						break;
					default:
						throw new InvalidOperationException ("unknown gc type:" + extendedInfo);
					}
					break;
				}
				case EventType.Metadata: {
					timeDiff = reader.ReadULeb128 ();
					Time += timeDiff;
					buffer.RecordTime (Time);
					
					byte mtype = reader.ReadByte ();
					long pointer = reader.ReadSLeb128 ();
					
					switch (mtype) {
					case TypeMetadata.Class:
						var image = reader.ReadSLeb128 ();
						var flags = reader.ReadULeb128 ();
						if (flags != 0)
							throw new Exception ("Flags should be 0");
						var name = reader.ReadNullTerminatedString ();
						HandleMetaDataClass (pointer, image, name);
						break;
					case TypeMetadata.Image:
						flags = reader.ReadULeb128 ();
						if (flags != 0)
							throw new Exception ("Flags should be 0");
						name = reader.ReadNullTerminatedString ();
						HandleMetaDataImage (pointer, name);
						break;
					case TypeMetadata.Thread:
						flags = reader.ReadULeb128 ();
						if (flags != 0)
							throw new Exception ("Flags should be 0");
						name = reader.ReadNullTerminatedString ();
						HandleMetaDataThread (pointer, name);
						break;
					default:
						throw new InvalidOperationException ("Unknown metadata event type:" + type);
					}
					break;
				}
				case EventType.Exception: {
					timeDiff = reader.ReadULeb128 ();
					Time += timeDiff;
					buffer.RecordTime (Time);
					
					switch (extendedInfo & (TypeException.BacktraceBit - 1)) {
					case TypeException.Clause:
						var clauseType = reader.ReadULeb128 ();
						var clauseNum = reader.ReadULeb128 ();
						var method = reader.ReadSLeb128 ();
						MethodBase += method;
						HandleExceptionClause (clauseType, clauseNum);
						break;
						
					case TypeException.Throw:
						var obj = reader.ReadSLeb128 ();
						Backtrace backtrace;
						if ((extendedInfo & TypeException.BacktraceBit) == TypeException.BacktraceBit) {
							backtrace = Backtrace.Read (reader);
						} else {
							backtrace = null;
						}
						HandleExceptionThrow (obj, backtrace);
						break;
						
					default:
						throw new InvalidOperationException ("Unknown exception event type:" + (extendedInfo & (TypeException.BacktraceBit - 1)));
					}
					break;
				}
				case EventType.Monitor: {
					timeDiff = reader.ReadULeb128 ();
					Time += timeDiff;
					buffer.RecordTime (Time);
					
					var obj = reader.ReadSLeb128 ();
					byte ev = (byte)((extendedInfo >> 4) & 0x3);
					Backtrace backtrace;
					if (ev == TypeMonitor.ProfilerMonitorContention && (extendedInfo & TypeMonitor.BacktraceBit) == TypeMonitor.BacktraceBit) {
						backtrace = Backtrace.Read (reader);
					} else {
						backtrace = null;
					}
					HandleMonitor (obj, backtrace);
					break;
				}
				case EventType.Sample:
					switch (extendedInfo) {
					case TypeSample.Hit:
						var sampleType = (SampleType)reader.ReadULeb128 ();
						var timeStamp = reader.ReadULeb128 ();
						var count = reader.ReadULeb128 ();
						var instructionPointers = new long[count];
						for (ulong i = 0; i < count; i++) {
							instructionPointers[i] = FixPointer (logHeader, buffer.Header.PtrBase + reader.ReadSLeb128 ());
						}
						buffer.RecordTime (timeStamp);
						HandleSampleHit (sampleType, timeStamp, instructionPointers);
						break;
					case TypeSample.USym:
						var address = FixPointer (logHeader, buffer.Header.PtrBase + reader.ReadSLeb128 ());
						var size = reader.ReadULeb128 ();
						var name = reader.ReadNullTerminatedString ();
						HandleSampleUSym (address, size, name);
						break;
					case TypeSample.UBin:
						timeDiff = reader.ReadULeb128 ();
						Time += timeDiff;
						buffer.RecordTime (Time);
						
						address = reader.ReadSLeb128 ();
						var offset = reader.ReadULeb128 ();
						size = reader.ReadULeb128 ();
						name = reader.ReadNullTerminatedString ();
						
						HandleSampleUBin (address, offset, size, name);
						break;
					case TypeSample.CountersDesc:
						var counters = new List<Tuple<ulong, string, ulong, ulong, ulong, ulong>> ();
						var len = reader.ReadULeb128 ();

						for (ulong i = 0; i < len; i++) {
							var csection = reader.ReadULeb128 ();
							var cname = reader.ReadNullTerminatedString ();
							var ctype = reader.ReadULeb128 ();
							var cunit = reader.ReadULeb128 ();
							var cvariance = reader.ReadULeb128 ();
							var cindex = reader.ReadULeb128 ();

							counters.Add (Tuple.Create<ulong, string, ulong, ulong, ulong, ulong> (
								csection, cname, ctype, cunit, cvariance, cindex
							));
						}

						HandleSampleCountersDesc (counters);
						break;
					case TypeSample.Counters:
						var samples = new List<Tuple<ulong, ulong, object>> ();
						var timestamp = reader.ReadULeb128 ();

						while (true) {
							var sindex = reader.ReadULeb128 ();
							if (sindex == 0)
								break;

							object sval;
							var stype = reader.ReadULeb128 ();
							switch (stype) {
							case TypeCountersSample.Int:
							case TypeCountersSample.Long:
							case TypeCountersSample.Word:
							case TypeCountersSample.TimeInterval:
								CountersSampleValues [sindex] = sval = reader.ReadSLeb128 ()
									+ (CountersSampleValues.ContainsKey (sindex) ? (long)(CountersSampleValues [sindex]) : 0);
								break;
							case TypeCountersSample.UInt:
							case TypeCountersSample.ULong:
								CountersSampleValues [sindex] = sval = reader.ReadULeb128 ()
									+ (CountersSampleValues.ContainsKey (sindex) ? (ulong)(CountersSampleValues [sindex]) : 0);
								break;
							case TypeCountersSample.Double:
								sval = reader.ReadDouble ();
								break;
							case TypeCountersSample.String:
								sval = reader.ReadULeb128 () == 1 ? reader.ReadNullTerminatedString () : null;
								break;
							default:
								throw new InvalidOperationException ("Unknown counter sample type:" + stype);
							}

							samples.Add (Tuple.Create<ulong, ulong, object> (sindex, stype, sval));
						}

						HandleSampleCounters (timestamp, samples);
						break;
					default:
						throw new InvalidOperationException ("Unknown sample event:" + extendedInfo);
					}
					break;
				case EventType.Heap: {
					switch (extendedInfo) {
					case TypeHeap.Start:
						timeDiff = reader.ReadULeb128 ();
						Time += timeDiff;
						buffer.RecordTime (Time);
						
						HandleHeapStart ();
						break;
					case TypeHeap.End:
						timeDiff = reader.ReadULeb128 ();
						Time += timeDiff;
						buffer.RecordTime (Time);
						
						HandleHeapEnd ();
						break;
					case TypeHeap.Object:
						var obj = reader.ReadSLeb128 ();
						var cl = reader.ReadSLeb128 ();
						var size = reader.ReadULeb128 ();
						ulong num = reader.ReadULeb128 ();
						var objectRefs = new long[num];
						var relOffsets = new ulong[num];
						for (ulong i = 0; i < num; i++) {
							relOffsets [i] = reader.ReadULeb128 ();
							objectRefs [i] = reader.ReadSLeb128 ();
						}
						HandleHeapObject (obj, cl, size, relOffsets, objectRefs);
						break;
					case TypeHeap.Root:
						ulong numRoots = reader.ReadULeb128 ();
						var numGc = reader.ReadULeb128 ();
						var objects = new long [numRoots];
						var rootTypes = new MonoProfileGCRootType [numRoots];
						var extraInfos = new ulong [numRoots];
						for (ulong i = 0; i < numRoots; i++) {
							objects[i] = reader.ReadSLeb128 ();
							rootTypes[i] = (MonoProfileGCRootType)reader.ReadULeb128 ();
							extraInfos[i] = reader.ReadULeb128 ();
						}
							HandleHeapRoot (numRoots, numGc, objects, rootTypes, extraInfos);
						break;
					default:
						throw new InvalidOperationException ("Unknown heap event type:" + extendedInfo);
					}
					break;
				}
				default:
					throw new InvalidOperationException ("invalid event type " + type);	
				}
			}

			CurrentBuffer = null;
			HandleBufferEndRead ();
		}
	}

}
