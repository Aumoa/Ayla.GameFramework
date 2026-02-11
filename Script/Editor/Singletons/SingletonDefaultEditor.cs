using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Ayla;

[CustomEditor(typeof(SingletonDefault))]
public class SingletonDefaultEditor : Editor
{
    private const string kFoldoutKey = nameof(Ayla) + "." + nameof(SingletonDefaultEditor) + ".{0}." + nameof(m_Foldout);

    private static Type[]? s_AllTargetTypes;
    private static readonly Dictionary<string, MonoScript> s_GUIDToMonoScriptCache = new();
    private static GUILayoutOption? s_DeleteButtonWidthLayoutCache;

    [InitializeOnLoadMethod]
    private static async void Initialize()
    {
        try
        {
            await ReflectionUtility.WaitForInitializeAsync();
            using var scope1 = ListPool<Type>.Get(out var allTypes);
            ReflectionUtility.GetTypes(Predicate, allTypes);
            s_AllTargetTypes = allTypes.ToArray();

            return;

            static bool Predicate(Type type)
            {
                return type.IsAbstract == false && type.IsAssignableTo(typeof(SingletonData));
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private string? m_TargetGUID;
    private SerializedProperty m_SingletonDatasProperty = null!;
    private SerializedProperty m_Rows = null!;
    private Type[]? m_UnresolvedMonoScripts;
    private HashSet<string>? m_DuplicatedMonoScriptTypenameSet;
    private string[]? m_UnresolvedMonoScriptTypenames;
    private int m_SelectedIndex;

    private Dictionary<string, bool> m_Foldout = new();
    private readonly Dictionary<SingletonData, Editor> m_CachedEditors = new();

    private void OnEnable()
    {
        m_SingletonDatasProperty = serializedObject.FindProperty("m_SingletonDatas");
        Debug.Assert(m_SingletonDatasProperty != null);
        m_Rows = m_SingletonDatasProperty!.FindPropertyRelative("m_Rows");
        Debug.Assert(m_Rows != null);

        if (serializedObject.isEditingMultipleObjects)
        {
            return;
        }

        var assetPath = AssetDatabase.GetAssetPath(serializedObject.targetObject);
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid))
        {
            guid = serializedObject.targetObject.GetInstanceID().ToString();
        }

        m_TargetGUID = guid;
        var foldouts = EditorPrefs.GetString(string.Format(kFoldoutKey, guid));
        m_Foldout.AddRange(foldouts.Split('|').Select(f => new KeyValuePair<string, bool>(f, false)));
    }

    private void OnDisable()
    {
        // 캐시된 에디터들 정리
        foreach (var editor in m_CachedEditors.Values)
        {
            if (editor != null)
            {
                DestroyImmediate(editor);
            }
        }
        m_CachedEditors.Clear();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (serializedObject.isEditingMultipleObjects)
        {
            EditorGUILayout.LabelField(SingletonDefaultText.MultipleEditCannotSupport);
            return;
        }

        if (s_AllTargetTypes == null)
        {
            EditorGUILayout.LabelField(SingletonDefaultText.Loading);
            return;
        }

        if (m_UnresolvedMonoScripts == null)
        {
            m_UnresolvedMonoScripts = GetUnresolvedMonoScripts();
            m_DuplicatedMonoScriptTypenameSet = m_UnresolvedMonoScripts
                .Select(t => t.Name)
                .GroupBy(t => t)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();
            m_UnresolvedMonoScriptTypenames = new string[m_UnresolvedMonoScripts.Length];
            for (int i = 0; i < m_UnresolvedMonoScripts.Length; ++i)
            {
                m_UnresolvedMonoScriptTypenames[i] = GetClassDisplayName(m_UnresolvedMonoScripts[i]);
            }
        }

        int arraySize = m_Rows.arraySize;
        if (arraySize > 0)
        {
            for (int i = 0; i < arraySize; ++i)
            {
                var element = m_Rows.GetArrayElementAtIndex(i);
                element.Next(true);
                var monoScript = GUIDToMonoScript(element.stringValue);
                string displayName = monoScript ? GetClassDisplayName(monoScript.GetClass()) : "<Missing Script>";

                bool foldout = m_Foldout.GetValueOrDefault(displayName, true);
                using (GUIScope.Changed())
                using (GUIScope.Horizontal())
                {
                    foldout = EditorGUILayout.Foldout(foldout, displayName);
                    if (GUI.changed)
                    {
                        m_Foldout[displayName] = foldout;
                        SaveFoldouts();
                    }

                    if (GUILayout.Button("X", s_DeleteButtonWidthLayoutCache ??= GUILayout.Width(24)))
                    {
                    }
                }

                if (foldout)
                {
                    using (EditorGUIScope.Indent())
                    {
                        element.Next(false); // Value
                        var singletonData = element.objectReferenceValue as SingletonData;
                        if (singletonData != null)
                        {
                            // 캐시된 에디터 사용 또는 새로 생성
                            if (!m_CachedEditors.TryGetValue(singletonData, out var singletonEditor) || 
                                singletonEditor == null)
                            {
                                singletonEditor = CreateEditor(singletonData);
                                if (singletonEditor != null)
                                {
                                    m_CachedEditors[singletonData] = singletonEditor;
                                }
                            }

                            if (singletonEditor != null)
                            {
                                singletonEditor.OnInspectorGUI();
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Missing SingletonData asset");
                        }
                    }
                }
            }
        }

        if (m_UnresolvedMonoScripts.Length == 0)
        {
            using (GUIScope.Color(Color.green))
            {
                EditorGUILayout.LabelField(SingletonDefaultText.NoElementsToAdd);
            }
        }
        else
        {
            using (EditorGUIScope.Horizontal())
            {
                m_SelectedIndex = EditorGUILayout.Popup(m_SelectedIndex, m_UnresolvedMonoScriptTypenames);
                if (GUILayout.Button(SingletonDefaultText.Add))
                {
                    var typeToCreate = m_UnresolvedMonoScripts[m_SelectedIndex];
                    var dataAsset = CreateInstance(typeToCreate);
                    dataAsset.hideFlags |= HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(dataAsset, serializedObject.targetObject);
                    var guid = GetMonoScriptGuidFromType(typeToCreate);

                    serializedObject.Update();
                    int arrayIndex = m_Rows.arraySize;
                    m_Rows.InsertArrayElementAtIndex(arrayIndex);
                    var addedArrayElement = m_Rows.GetArrayElementAtIndex(arrayIndex);
                    addedArrayElement.Next(true);  // Key
                    addedArrayElement.stringValue = guid;
                    addedArrayElement.Next(false);  // Value
                    addedArrayElement.objectReferenceValue = dataAsset;
                    serializedObject.ApplyModifiedProperties();
                }

                m_UnresolvedMonoScripts = null;
                m_UnresolvedMonoScriptTypenames = null;
            }
        }

        return;

        string GetClassDisplayName(Type @class)
        {
            Debug.Assert(m_DuplicatedMonoScriptTypenameSet != null);

            var simpleName = @class.Name;
            if (m_DuplicatedMonoScriptTypenameSet!.Contains(simpleName))
            {
                return ObjectNames.NicifyVariableName(simpleName) + $"({@class.AssemblyQualifiedName})";
            }
            else
            {
                return ObjectNames.NicifyVariableName(simpleName);
            }
        }
    }

    private void SaveFoldouts()
    {
        if (m_TargetGUID == null)
        {
            return;
        }

        var foldouts = string.Join('|', m_Foldout.Where(kv => kv.Value == false).Select(kv => kv.Key));
        EditorPrefs.SetString(string.Format(kFoldoutKey, m_TargetGUID), foldouts);
    }

    private Type[] GetUnresolvedMonoScripts()
    {
        using var scope1 = ListPool<Type>.Get(out var unresolvedMonoScripts);
        var dictionary = (IDictionary<string, SingletonData>)m_SingletonDatasProperty.boxedValue;
        foreach (var type in s_AllTargetTypes!)
        {
            var guid = GetMonoScriptGuidFromType(type);
            if (!dictionary.ContainsKey(guid))
            {
                unresolvedMonoScripts.Add(type);
            }
        }

        return unresolvedMonoScripts.ToArray();
    }

    private static string GetMonoScriptGuidFromType(Type type)
    {
        if (TryGetMonoScriptGuidFromType(type, out var guid))
        {
            return guid;
        }

        throw new InvalidOperationException($"Failed to get MonoScript GUID for type {type.FullName}.");
    }

    private static bool TryGetMonoScriptGuidFromType(Type type, [NotNullWhen(true)] out string? guid)
    {
        var monoScript = MonoScriptUtility.GetMonoScript(type);
        if (monoScript)
        {
            var assetPath = AssetDatabase.GetAssetPath(monoScript);
            guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!string.IsNullOrEmpty(guid))
            {
                return true;
            }
        }

        guid = null;
        return false;
    }

    private static MonoScript? GUIDToMonoScript(string guid)
    {
        if (s_GUIDToMonoScriptCache.TryGetValue(guid, out var monoScript))
        {
            return monoScript;
        }

        if (!GUID.TryParse(guid, out var guid2))
        {
            return null;
        }

        monoScript = AssetDatabase.LoadAssetByGUID<MonoScript>(guid2);
        if (monoScript == null)
        {
            return null;
        }

        s_GUIDToMonoScriptCache.Add(guid, monoScript);
        return monoScript;
    }
}
