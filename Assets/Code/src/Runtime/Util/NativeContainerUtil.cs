using System;
using Unity.Collections;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public static class NativeMultiHashMapExtensions {

  public static NativeArray<TValue>? CopyValuesForKey<TKey, TValue>(this ref NativeMultiHashMap<TKey, TValue> map, TKey key) 
                                                                    where TKey : struct, IEquatable<TKey>
                                                                    where TValue : struct {
    if (!map.ContainsKey(key)) return null;

    var count = map.CountValuesForKey(key);
    var iterator = map.GetValuesForKey(key);
    var values = new NativeArray<TValue>(count, Allocator.Temp);
    for (var i = 0; i < count && iterator.MoveNext(); i++) {
      values[i] = iterator.Current;
    }

    return values;
  }

}


}