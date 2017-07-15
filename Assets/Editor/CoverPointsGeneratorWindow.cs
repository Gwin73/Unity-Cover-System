using UnityEditor;
using UnityEngine;

public class CoverPointsGeneratorWindow: EditorWindow
{
    [SerializeField]
     CoverPointsGenerator cpg;

    [MenuItem("Window/CoverPointsGenerator")]
    public static void ShowWindow() =>
        GetWindow(typeof(CoverPointsGeneratorWindow));

    void OnGUI()
    {
        Init();

        GUILayout.Label("Settings", EditorStyles.boldLabel);

        cpg.CoverPoint = EditorGUILayout.ObjectField("Prefab", cpg.CoverPoint, typeof(GameObject), true) as GameObject;
        cpg.CoverPointParent = EditorGUILayout.ObjectField("Scene Parent", cpg.CoverPointParent, typeof(Transform), true) as Transform;
        cpg.CoverPointsDistance = EditorGUILayout.FloatField("Desired distance", cpg.CoverPointsDistance);
        cpg.MaxInnerEdgeLength = EditorGUILayout.FloatField("Max inner-edge length", cpg.MaxInnerEdgeLength);

        if (GUILayout.Button("Generate"))
            cpg.Generate();

        var coverCount = GameObject.FindGameObjectsWithTag(cpg.CoverPoint.tag)?.Length ?? 0;
        EditorGUI.BeginDisabledGroup(coverCount == 0);
        if (GUILayout.Button("Remove"))
            cpg.Remove();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField($"Count: {coverCount}");
    }

    void Init()
    {
        if (cpg == null)
            cpg = new CoverPointsGenerator();

        if (cpg.CoverPointParent == null)
            cpg.CoverPointParent = GameObject.FindGameObjectWithTag("CoverPointParent").transform;
    }
}