using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo {

/// <summary>
/// A singleton manager for enabling and disabling the global
/// loading screen.
/// </summary>
public class LoadingScreen : MonoBehaviour {

  public static LoadingScreen Instance { get; private set; }
  static List<Task> Tasks = new List<Task>();

  public int PoolSize => Tasks.Count;

#pragma warning disable 0649
  [SerializeField] Object[] ViewObjects;
#pragma warning restore 0649

  public static bool IsLoading {
    get {
      if (Tasks.Count <= 0) return false;
      foreach (var task in Tasks) {
        if (!task.IsCompleted) return true;
      }
      return false;
    }
  }

  void Awake() {
    Instance = this;
    UpdateActive();
  }

  void Update() => UpdateActive();

  void UpdateActive() {
    var isLoading = IsLoading;
    foreach (var view in ViewObjects) {
      ObjectUtility.SetActive(view, isLoading);
    }
    if (Tasks.Count > 0) {
      Tasks.RemoveAll(t => t.IsCompleted);
    }
  }

  /// <summary>
  /// Adds a task to the pool to await. Does not await 
  /// it's completion. Does nothing if the task is null
  /// or is already completed.
  /// </summary>
  /// <param name="task"></param>
  public static void AddTask(Task task) {
    if (!IsAwaitableTask(task)) return;
    Tasks.Add(task);
    if (Instance != null) {
      Instance.UpdateActive();
    }
  }

  /// <summary>
  /// Adds a task to the pool to await, and awaits 
  /// it's completion. Does nothing if the task is null
  /// or is already completed. Does not wait for the
  /// rest of the pool to complete.
  /// </summary>
  /// <param name="task">the task to await the completion of</param>
  public static async Task Await(Task task) {
    if (!IsAwaitableTask(task)) return;
    AddTask(task);
    await task;
  }

  /// <summary>
  /// Waits for all of the tasks in the pool to
  /// complete. This includes all tasks added to the
  /// pool since since starting to to wait. The task
  /// pool should be empty by the end of this.
  /// </summary>
  public static async Task AwaitAll() {
    while (IsLoading) {
      await Task.WhenAll(Tasks);
    }
  }

  static bool IsAwaitableTask(Task task) {
    return task != null && !task.IsCompleted;
  }

}

}