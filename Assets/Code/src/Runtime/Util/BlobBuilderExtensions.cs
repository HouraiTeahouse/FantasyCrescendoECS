using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public static class BlobBuilderExtensions {

  public static BlobBuilderArray<T> Construct<T>(this ref BlobBuilder builder, ref BlobArray<T> array,
                                                 IEnumerable<T> values) where T : struct {
    return builder.Construct<T>(ref array, values?.ToArray() ?? new T[0]);
  }

}

}