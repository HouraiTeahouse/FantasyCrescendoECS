using System;
using Unity.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public static class NativeMultiHashMapExtensions {

  public static NativeArray<TValue>? CopyValuesForKey<TKey, TValue>(this ref NativeMultiHashMap<TKey, TValue> map, TKey key) 
                                                                    where TKey : struct, IEquatable<TKey>
                                                                    where TValue : struct {
    if (!map.ContainsKey(key)) return null;

    var count = map.CountValuesForKey(key);
    var iterator = map.GetValuesForKey(key);
    var values = new NativeArray<TValue>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (var i = 0; i < count && iterator.MoveNext(); i++) {
      values[i] = iterator.Current;
    }

    return values;
  }

}

public static class NativeArrayExtensions {

  /// <summary>
  /// Gets a ReadOnlySpan view of the same memory held by a NativeArray.
  /// Must have read access to the NativeArray.
  /// </summary>
  public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeArray<T> array) where T : struct {
    return new ReadOnlySpan<T>(NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(array), array.Length);
  }

  /// <summary>
  /// Gets a Span view of the same memory held by a NativeArray.
  /// Must have read and write access to the NativeArray.
  /// </summary>
  public static unsafe Span<T> AsSpan<T>(this NativeArray<T> array) where T : struct {
    return new Span<T>(NativeArrayUnsafeUtility.GetUnsafePtr(array), array.Length);
  }

  /// <summary>
  /// Computes the 32-bit XXHash of the memory held by a NativeArray.
  /// Must have read access to the NativeArray.
  /// </summary>
  public static unsafe uint XXHash32<T>(this NativeArray<T> array, uint seed = 0) 
                                        where T : struct {
    return XXHash.Hash32((byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(array), GetByteLength(array), seed);
  }

  /// <summary>
  /// Computes the 64-bit XXHash of the memory held by a NativeArray.
  /// Must have read access to the NativeArray.
  /// </summary>
  public static unsafe ulong XXHash64<T>(this NativeArray<T> array, uint seed = 0) 
                                        where T : struct {
    return XXHash.Hash64((byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(array), GetByteLength(array), seed);
  }

  /// <summary>
  /// Gets the length of a NativeArray in bytes.
  /// </summary>
  public static int GetByteLength<T>(this NativeArray<T> array) where T : struct {
    return array.Length * UnsafeUtility.SizeOf<T>();
  }

}

public static class NativeSliceExtensions {
  
  /// <summary>
  /// Gets a ReadOnlySpan view of the same memory held by a NativeSlice.
  /// Must have read access to the NativeSlice.
  /// </summary>
  public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeSlice<T> slice) where T : struct {
    return new ReadOnlySpan<T>(NativeSliceUnsafeUtility.GetUnsafeReadOnlyPtr(slice), slice.Length);
  }

  /// <summary>
  /// Gets a Span view of the same memory held by a NativeSlice.
  /// Must have read and write access to the NativeSlice.
  /// </summary>
  public static unsafe Span<T> AsSpan<T>(this NativeSlice<T> slice) where T : struct {
    return new Span<T>(NativeSliceUnsafeUtility.GetUnsafePtr(slice), slice.Length);
  }

  /// <summary>
  /// Computes the 32-bit XXHash of the memory held by a NativeSlice.
  /// Must have read access to the NativeSlice.
  /// </summary>
  public static unsafe uint XXHash32<T>(this NativeSlice<T> slice, uint seed = 0) 
                                        where T : struct {
    return XXHash.Hash32((byte*)NativeSliceUnsafeUtility.GetUnsafeReadOnlyPtr(slice), GetByteLength(slice), seed);
  }

  /// <summary>
  /// Computes the 64-bit XXHash of the memory held by a NativeSlice.
  /// Must have read access to the NativeSlice.
  /// </summary>
  public static unsafe ulong XXHash64<T>(this NativeSlice<T> slice, uint seed = 0) 
                                        where T : struct {
    return XXHash.Hash64((byte*)NativeSliceUnsafeUtility.GetUnsafeReadOnlyPtr(slice), GetByteLength(slice), seed);
  }

  /// <summary>
  /// Gets the length of a NativeSlice in bytes.
  /// </summary>
  public static int GetByteLength<T>(this NativeSlice<T> slice) where T : struct {
    return slice.Length * UnsafeUtility.SizeOf<T>();
  }

}

public static class SpanExtensions {

  /// <summary>
  /// Copies the memory held by the Span into a newly allocated NativeArray.
  /// </summary>
  public static unsafe NativeArray<T> ToNativeArray<T>(this Span<T> span, Allocator allocator) where T : struct {
    var array = new NativeArray<T>(span.Length, allocator);
    span.CopyTo(array.AsSpan());
    return array;
  }

  /// <summary>
  /// Copies the memory held by the ReadOnlySpan into a newly allocated NativeArray.
  /// </summary>
  public static unsafe NativeArray<T> ToNativeArray<T>(this ReadOnlySpan<T> span, Allocator allocator) where T : struct {
    var array = new NativeArray<T>(span.Length, allocator);
    span.CopyTo(array.AsSpan());
    return array;
  }

}

}