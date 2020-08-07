using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HouraiTeahouse.FantasyCrescendo {

/// <summary>
/// A iniitalizer component that loads dynamically loadable data into
/// the global Registry.
/// </summary>
public class DataLoader : MonoBehaviour {

  static TaskCompletionSource<object> _loading = new TaskCompletionSource<object>();

  [SerializeField] AssetLabelReference[] LoadedLabels;

  static Type[] ValidImportTypes = new [] {
    typeof(SceneData),
    typeof(Character)
  };

  async void Start() {
    var labelsLoaded = 0;
    await Task.WhenAll(LoadedLabels.Select(async label => {
      var results = await Addressables.LoadAssetsAsync<UnityEngine.Object>(label.RuntimeKey, null).Task;
      foreach (var info in results) {
        if (info != null && !Register(info)) {
          Addressables.Release(info);
        }
      } 
      labelsLoaded++;
      if (labelsLoaded >= LoadedLabels.Length) {
        _loading.TrySetResult(new object());
        Debug.Log("Finished loading data");
      }
    }));
  }

  /// <summary>
  /// A hold until the 
  /// </summary>
  /// <returns></returns>
  public static Task WaitUntilLoaded() {
    return _loading.Task;
  }

  void RegisterAll(IEnumerable<UnityEngine.Object> data) {
    foreach (var datum in data) {
      Register(datum);
    }
  }

  bool Register(UnityEngine.Object data) {
    var dataObj = data as IEntity;
    if (dataObj == null) return false;
    foreach (var type in ValidImportTypes) {
      if (type.IsInstanceOfType(data)) {
        Registry.Register(type, dataObj);
        Debug.Log($"Registered {type.Name}: {data.name} ({dataObj.Id})");
        return true;
      }
    }
    return false;
  }

}

}