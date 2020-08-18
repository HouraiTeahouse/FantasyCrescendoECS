using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HouraiTeahouse.FantasyCrescendo {

public interface IView<T> {
  void UpdateView(in T model);
}

public sealed class AggregateView<T> : IView<T> {

  readonly IView<T>[]  _subviews;

  public AggregateView(IEnumerable<IView<T>> subviews) {
    _subviews = subviews.ToArray();
  }

  public void UpdateView(in T model) {
    foreach (var view in _subviews) {
      view.UpdateView(model);
    }
  }

  public static IView<T> FromGameObject(GameObject go) {
    var components = go.GetComponentsInChildren<IView<T>>();
    if (components.Length == 1) {
      return components[0];
    }
    return new AggregateView<T>(components);
  }

}

}