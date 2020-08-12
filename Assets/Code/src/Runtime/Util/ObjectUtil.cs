using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public static class ObjectUtil {

  public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component {
    var result = gameObject.GetComponent<T>();
    return result != null ? result : gameObject.AddComponent<T>();
  }

}


}