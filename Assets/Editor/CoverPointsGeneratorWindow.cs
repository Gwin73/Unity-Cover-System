using System.Linq;
using UnityEditor;
using UnityEngine;
using static CoverPointsGenerator;

public class CoverPointsGeneratorWindow: EditorWindow
{
    [SerializeField]
    private CoverPointsGenerator cpg;

    [MenuItem("Window/CoverPointsGenerator")]
    public static void ShowWindow() =>
        GetWindow(typeof(CoverPointsGeneratorWindow));

    private void OnGUI()
    {
        if (cpg == null)
            cpg = new CoverPointsGenerator(); //TODO: Static instead?

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        cpg.CoverPoint = EditorGUILayout.ObjectField("Prefab", cpg.CoverPoint, typeof(GameObject), true) as GameObject;
        if (cpg.CoverPointParent == null)
            cpg.CoverPointParent = GameObject.FindGameObjectWithTag("CoverPointParent").transform;
        cpg.CoverPointParent = EditorGUILayout.ObjectField("Scene Parent", cpg.CoverPointParent, typeof(Transform), true) as Transform;
        cpg.CoverPointsDistance = EditorGUILayout.FloatField("Desired distance", cpg.CoverPointsDistance);
        cpg.MaxInnerEdgeLength = EditorGUILayout.FloatField("Max inner-edge length", cpg.MaxInnerEdgeLength);

        if (GUILayout.Button("Generate"))
            cpg.Generate();

        //cpg.CoverPoint?.tag
        var coverPoints = GameObject.FindGameObjectsWithTag("CoverPoint");
        var coverCount = coverPoints?.Length ?? 0;

        EditorGUI.BeginDisabledGroup(coverCount == 0);
        if (GUILayout.Button("Remove"))
            cpg.Remove();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField($"Count: {coverCount}");
    }
}
