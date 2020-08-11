using System;
using System.Buffers;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Serialization;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

/// <summary>
/// An abstract class for in-memory binary readers and writers.
/// </summary>
public abstract unsafe class BinaryBuffer {

  protected byte* _start, _current, _end;

  /// <summary>
  /// The size in bytes of the contents written to the buffer.
  /// </summary>
  public int Size => (int)(_current - _start);

  /// <summary>
  /// The current total capacity of the underlying buffer.
  /// </summary>
  public int Capacity => (int)(_end - _start);

  protected bool IsValid => _start != null && _current != null && _end != null &&
    Capacity >= 0 && Size >= 0 && Capacity >= Size;

  /// <summary>
  /// Creates a read-only view of the underlying contents in the buffer.
  /// </summary>
  /// <remarks>
  /// Note this does not copy the contents of the underlying buffer, and 
  /// will be invalidated if additional data is written to the buffer. There
  /// are zero safety checks to ensure that the underlying buffer isn't being
  /// written to while the view is valid.
  /// </remarks>
  /// <returns>a ReadOnlySpan<byte> referencing</returns>
  public ReadOnlySpan<byte> AsReadOnlySpan() {
    return new ReadOnlySpan<byte>(_start, Size);
  }

  public unsafe byte[] ToArray() {
    var buffer = new byte[Size];
    fixed (byte* ptr = buffer) {
      UnsafeUtility.MemCpy(ptr, _start, Size);
    }
    return buffer;
  }

  public unsafe uint XXHash() {
    return Unity.Core.XXHash.Hash32(_start, Size);
  }

  public FixedBinaryReader ToReader() {
    return new FixedBinaryReader(_start, Size);
  }
  
  /// <summary>
  /// Seeks a position in the buffer.
  /// </summary>
  /// <remarks>
  /// This only does bounds checking in debug mode. No bounds checking
  /// will be done in release builds.
  /// </remarks>
  /// <param name="position">the position in the buffer.</param>
  public void Seek(int position) {
    Assert.IsTrue(position >= 0 && position < Capacity);
    _current = _start + position;
  }

}

/// <summary>
/// A growable GC-less in-memory BinaryWriter implementation that uses 
/// UnsafeUtility functions to allocate unmanaged memory buffers.
/// </summary>
/// <remarks>
/// This type must be manually disposed to deallocate the underlying 
/// buffer.
/// 
/// This type is not threadsafe by itself and should not be used from 
/// multiple threads at the same time.
/// 
/// This type is not Burst compatible as it is a managed type.
/// 
/// All safety checks and assertions will not be present in release builds.
/// </remarks>
public unsafe class DynamicBinaryWriter : BinaryBuffer, BinaryWriter {

  public DynamicBinaryWriter(int capacity) {
    Init(capacity);
  }

  /// <summary>
  /// Writes a contiguous region of bytes to the buffer. If 
  /// the buffer's internal capacity is exceeded, the buffer will
  /// be resized until it can support the write. This will invalidate all
  /// ReadOnlySpans created from the writer.
  /// </summary>
  /// <param name="buf">the start of the buffer to write from</param>
  /// <param name="bufSize">the size of the buffer to write from</param>
  public void WriteBytes(void* buf, int bufSize) {
    Assert.IsTrue(IsValid);
    if (_current + bufSize > _end) {
      ResizeBuffer(Size + bufSize);
    }
    UnsafeUtility.MemCpy(_current, buf, bufSize);
    _current += bufSize;
    Assert.IsTrue(IsValid);
  }

  /// <summary>
  /// Resets the internal buffer to a specified capacity and clears the
  /// contents of the buffer. This will invalidate all ReadOnlySpans created 
  /// from the writer.
  /// </summary>
  /// <param name="capacity">the new maximum capacity of the underlying buffer..</param>
  public void Reset(int capacity) {
    Dispose();
    Init(capacity);
  }

  /// <summary>
  /// Trims exess capacity off of the buffer based on the current
  /// size of the contents in the buffer. This will invalidate all ReadOnlySpans 
  /// created from the writer.
  /// </summary>
  public void Trim() {
    var size = Size;
    byte* buffer = AllocateBuffer(size);
    UnsafeUtility.MemCpy(buffer, _start, size);
    UnsafeUtility.Free(_start, Allocator.Persistent);
    _current = buffer + size;
    _end = buffer + size;
    _start = buffer;
    Assert.IsTrue(IsValid);
  }

  /// <summary>
  /// Disposes of the writer and the underlying buffer.
  /// </summary>
  public void Dispose() {
    if (_start != null) {
      UnsafeUtility.Free(_start, Allocator.Persistent);
      _start = null;
    }
  }

  void ResizeBuffer(int minSize) {
    Assert.IsTrue(IsValid);
    Assert.IsTrue(minSize > Size && Capacity > Size);
    var size = Size;
    var capacity = Capacity;
    while (capacity < minSize) {
      capacity *= 2;
    }
    Assert.IsTrue(capacity >= minSize && capacity >= size && capacity >= Capacity);
    byte* buffer = AllocateBuffer(capacity);
    UnsafeUtility.MemCpy(buffer, _start, size);
    UnsafeUtility.Free(_start, Allocator.Persistent);
    _current = buffer + size;
    _end = buffer + capacity;
    _start = buffer;
    Assert.IsTrue(IsValid);
    Assert.AreEqual(capacity, Capacity);
    Assert.AreEqual(size, Size);
  }

  void Init(int capacity) {
    var buffer = AllocateBuffer(capacity);
    _start = buffer;
    _current = buffer;
    _end = buffer + capacity;
  }

  static byte* AllocateBuffer(int capacity) {
    Assert.IsTrue(capacity >= 0);
    return (byte*)UnsafeUtility.Malloc(capacity, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent);
  }

}

/// <summary>
/// A fixed size GC-less in-memory BinaryReader implementation.
/// </summary>
/// <remarks>
/// The underlying buffer must outlive the reader's use of it. As a result,
/// the type does not require manual disposal.
/// 
/// This type is not threadsafe by itself and should not be used from 
/// multiple threads at the same time.
/// 
/// This type is not Burst compatible as it is a managed type.
/// 
/// All safety checks and assertions will not be present in release builds.
/// </remarks>
public unsafe class FixedBinaryReader : BinaryBuffer, BinaryReader {

  public FixedBinaryReader(void* buf, int bufSize) {
    _start = (byte*)buf;
    _current = (byte*)buf;
    _end = (byte*)buf + bufSize;
    Assert.IsTrue(IsValid);
  }

  /// <summary>
  /// Reads a contiguous region of bytes to the buffer. If 
  /// the buffer's internal capacity is exceeded, an error will
  /// be raised.
  /// </summary>
  /// <param name="buf">the start of the buffer to read into</param>
  /// <param name="bufSize">the size of the buffer to write into</param>
  public void ReadBytes(void* buf, int size) {
    if (_current + size >= _end) {
      throw new IndexOutOfRangeException("FixedBinaryReader is out of bounds!");
    }
    Assert.IsTrue(IsValid);
    UnsafeUtility.MemCpy(buf, _current, size);
    _current += size;
  }

  void IDisposable.Dispose() { }

}

}