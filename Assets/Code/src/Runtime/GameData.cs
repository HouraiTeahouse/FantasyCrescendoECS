using HouraiTeahouse.Attributes;
using System;
using UnityEngine;
using Random = System.Random;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#endif 

namespace HouraiTeahouse.FantasyCrescendo {

public abstract class GameData : ScriptableObject, IEntity {

  [SerializeField, ReadOnly] uint _id;

  public uint Id => _id;

  public bool IsSelectable = true;
  public bool IsVisible = true;
  public bool IsDebug = false;

  void Reset() => RegenerateID();

  [ContextMenu("Regenerate ID")]
  void RegenerateID() {
    var newId = new Random().Next();
    _id = (uint)newId + (uint)Int32.MaxValue;
  }

}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class GameDataIdAttribute : PropertyAttribute {
  public Type DataTypeRestriction { get; }

  public GameDataIdAttribute(Type dataType = null) {
    if (dataType != null && !typeof(GameData).IsAssignableFrom(dataType)) {
      throw new ArgumentException($"Type must derive from GameData: {dataType}!");
    }
    DataTypeRestriction = dataType;
  }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GameDataIdAttribute))]
public class GameDataIdAttributeDrawer : PropertyDrawer {

  Dictionary<string, GameData> _cachedData;

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var dataAttribute = attribute as GameDataIdAttribute;
    var displayType = dataAttribute.DataTypeRestriction ?? typeof(GameData);
    _cachedData = _cachedData ?? new Dictionary<string, GameData>();
    GameData data;
    if (!_cachedData.TryGetValue(property.propertyPath, out data)) {
      var options = LoadAllOfType(displayType);
      if (options.TryGetValue((uint)property.longValue, out GameData loadedData)) {
        data = loadedData;
      }
    }
    data = (GameData)EditorGUI.ObjectField(position, label, data, displayType, false);
    property.longValue = (data != null) ? data.Id : 0;
    _cachedData[property.propertyPath] = data;
  }

  Dictionary<uint, GameData> LoadAllOfType(Type type) {
    return AssetDatabase.FindAssets($"t:{type.Name}")
                         .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                         .Select(path => AssetDatabase.LoadAssetAtPath<GameData>(path))
                         .Where(data => data != null)
                         .ToDictionary(data => data.Id, data => data);
  }

}
#endif

}