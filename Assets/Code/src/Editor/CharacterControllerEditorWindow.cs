using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class CharacterControllerEditorWindow : GraphViewEditorWindow  {

  [MenuItem("Window/Fantasy Crescendo/Character Controller")]
  public static void ShowWindow() {
    var window = GetWindow<CharacterControllerEditorWindow>();
    window.titleContent = new GUIContent("Character Controller");
    window.Show();
  }

  public override IEnumerable<GraphView> graphViews => new [] { _graphView };

  CharacterControllerGraphView _graphView;

  void OnEnable() {
    _graphView = new CharacterControllerGraphView();
    _graphView.StretchToParentSize();
    rootVisualElement.Add(_graphView);
    rootVisualElement.Add(GenerateToolbar());
  }

  void OnDisable() {
    rootVisualElement.Remove(_graphView);
  }

  Toolbar GenerateToolbar() {
    var toolbar = new Toolbar();
    var newStateButton = new ToolbarButton(() => _graphView.CreateState("New State"));
    newStateButton.text = "Add State";
    toolbar.Add(newStateButton);
    return toolbar;
  }

  public class CharacterControllerGraphView : GraphView {

    public CharacterControllerGraphView() {
      const string stylesheetPath = "Assets/Code/src/Editor/CharacterControllerEditorWindow.uss";
      styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesheetPath));

      var grid = new GridBackground { name = "Grid" };
      Insert(0, grid);
      grid.StretchToParentSize();

      this.AddManipulator(new ContentDragger());
      this.AddManipulator(new SelectionDragger());
      this.AddManipulator(new RectangleSelector());
      
      SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
    }

    public StateNode CreateState(string stateName) {
      var state = new StateNode {
        title = stateName
      };
      AddElement(state);
      return state;
    }

    public override List<Port> GetCompatiblePorts(Port start, NodeAdapter nodeAdapter) {
      var compatiblePorts = new List<Port>();
      ports.ForEach(port => {
        if (start == port || start.node == port.node) return;
        compatiblePorts.Add(port);
      });
      return compatiblePorts;
    }
  }

  public class StateNode : Node {
    public StateNode() {
      var inputs = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
      var outputs = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));

      inputs.portName = "Inputs";
      outputs.portName = "Outputs";

      inputContainer.Add(inputs);
      outputContainer.Add(outputs);

      RefreshExpandedState();
      RefreshPorts();
    }

    public override bool IsRenamable() => true;
  }

}

}