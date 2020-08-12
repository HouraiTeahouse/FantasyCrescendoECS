using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class CharacterFrameDataEditorWindow : EditorWindow {

  [MenuItem("Window/Fantasy Crescendo/Frame Data")]
  public static void ShowWindow() {
    var window = GetWindow<CharacterFrameDataEditorWindow>();
    window.titleContent = new GUIContent("Frame Data");
  }

  void OnEnable() {
    SetRoot("Assets/Code/src/Editor/CharacterFrameDataEditorWindow.uxml");
    AddStyleSheet("Assets/Code/src/Editor/CharacterFrameDataEditorWindow.uss");
  }

  void OnGUI() {
    // Set the container height to the window
    GetElementByName("Container").style.height = new StyleLength(position.height);
  }

  void SetRoot(string path) {
    var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
    rootVisualElement.Add(visualTree.CloneTree());
  }

  void AddStyleSheet(string path) {
    var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
    rootVisualElement.styleSheets.Add(styleSheet);
  }

  VisualElement GetElementByName(string name) {
    return rootVisualElement.Q<VisualElement>(name);
  }

}

}