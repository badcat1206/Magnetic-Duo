using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// [일회용] Trap의 예전 상자 리스폰 설정(canDestroyBox + 좌표)을
/// 새 설정(respawnNBox / respawnSBox)으로 변환합니다.
///
/// 예전 규칙: canDestroyBox == true 이고 해당 극성의 좌표가 (0,0)이 아닐 때만 리스폰.
/// 새 규칙:   respawnNBox / respawnSBox 체크박스로 직접 결정.
///
/// 실행 후 결과를 확인했으면 이 파일과 Trap.cs의 마이그레이션 전용 필드를 삭제하세요.
/// </summary>
public static class TrapRespawnMigration
{
    private const string ReportFileName = "TrapRespawnMigrationReport.txt";
    private const float PositionTolerance = 0.01f;

    [MenuItem("Tools/Migration/Trap 상자 리스폰 설정 변환")]
    private static void Run()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Trap 상자 리스폰 설정 변환",
            "Assets 안의 모든 프리팹과 씬(_Recovery 제외)을 열어 Trap 설정을 변환하고 저장합니다.\n\n" +
            "실행 전에 Git 커밋 상태를 확인하세요.",
            "실행", "취소");
        if (!ok) return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var previousSetup = EditorSceneManager.GetSceneManagerSetup();
        var report = new StringBuilder();
        int trapCount = 0, changedCount = 0, warningCount = 0;

        try
        {
            // 1. 프리팹 먼저 (씬의 프리팹 인스턴스가 바뀐 기본값을 받도록)
            report.AppendLine("===== Prefabs =====");
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (IsExcluded(path)) continue;
                EditorUtility.DisplayProgressBar("Trap 마이그레이션 (프리팹)", path, (float)i / prefabGuids.Length);

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool dirty = false;
                    foreach (Trap trap in root.GetComponentsInChildren<Trap>(true))
                    {
                        trapCount++;
                        if (Migrate(trap, path, null, report, ref warningCount)) { dirty = true; changedCount++; }
                    }
                    if (dirty) PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            // 2. 씬
            report.AppendLine();
            report.AppendLine("===== Scenes =====");
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                if (IsExcluded(path)) continue;
                EditorUtility.DisplayProgressBar("Trap 마이그레이션 (씬)", path, (float)i / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var boxes = new List<RespawnableBox>(
                    Object.FindObjectsByType<RespawnableBox>(FindObjectsInactive.Include, FindObjectsSortMode.None));

                bool dirty = false;
                foreach (Trap trap in Object.FindObjectsByType<Trap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    trapCount++;
                    if (Migrate(trap, path, boxes, report, ref warningCount)) { dirty = true; changedCount++; }
                }

                if (dirty)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (previousSetup.Length > 0 && System.Array.TrueForAll(previousSetup, s => !string.IsNullOrEmpty(s.path)))
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        string summary = $"Trap {trapCount}개 검사 / {changedCount}개 변경 / 경고 {warningCount}개";
        report.Insert(0, summary + "\n\n");

        string reportPath = Path.Combine(Directory.GetCurrentDirectory(), ReportFileName);
        File.WriteAllText(reportPath, report.ToString(), Encoding.UTF8);

        Debug.Log($"[TrapRespawnMigration] {summary}\n리포트: {reportPath}\n\n{report}");
        EditorUtility.DisplayDialog("Trap 마이그레이션 완료",
            $"{summary}\n\n리포트: {ReportFileName} (프로젝트 폴더)", "확인");
    }

    /// <returns>값이 실제로 바뀌었으면 true</returns>
    private static bool Migrate(Trap trap, string assetPath, List<RespawnableBox> sceneBoxes,
                                StringBuilder report, ref int warningCount)
    {
        var so = new SerializedObject(trap);
        bool canDestroyBox = so.FindProperty("canDestroyBox").boolValue;
        Vector2 nPoint = so.FindProperty("nBoxSpawnPoint").vector2Value;
        Vector2 sPoint = so.FindProperty("sBoxSpawnPoint").vector2Value;

        // 예전 Trap.RespawnBox()와 동일한 판정 (Vector2 != 연산자 그대로 사용)
        bool newN = canDestroyBox && nPoint != Vector2.zero;
        bool newS = canDestroyBox && sPoint != Vector2.zero;

        SerializedProperty nProp = so.FindProperty("respawnNBox");
        SerializedProperty sProp = so.FindProperty("respawnSBox");

        bool changed = false;
        // 값이 다를 때만 쓰기 → 불필요한 프리팹 오버라이드 방지
        if (nProp.boolValue != newN) { nProp.boolValue = newN; changed = true; }
        if (sProp.boolValue != newS) { sProp.boolValue = newS; changed = true; }
        if (changed) so.ApplyModifiedPropertiesWithoutUndo();

        if (newN || newS)
        {
            report.AppendLine($"{assetPath} :: {GetHierarchyPath(trap.transform)}  →  N={newN}, S={newS}");
        }

        // 씬에서만: 예전 트랩 좌표와 상자의 시작 위치가 다르면 경고
        if (sceneBoxes != null)
        {
            if (newN) CheckSpawnPoint("NBox", nPoint, sceneBoxes, report, ref warningCount);
            if (newS) CheckSpawnPoint("SBox", sPoint, sceneBoxes, report, ref warningCount);
        }

        return changed;
    }

    private static void CheckSpawnPoint(string tag, Vector2 oldPoint, List<RespawnableBox> boxes,
                                        StringBuilder report, ref int warningCount)
    {
        var matching = boxes.FindAll(b => b.CompareTag(tag));
        if (matching.Count == 0) return;

        bool anyAtPoint = matching.Exists(b => Vector2.Distance(b.transform.position, oldPoint) <= PositionTolerance);
        if (!anyAtPoint)
        {
            warningCount++;
            report.AppendLine($"    ⚠ {tag}: 예전 트랩 좌표 {oldPoint}와 시작 위치가 같은 상자가 없음 " +
                              $"→ 상자의 RespawnableBox.respawnPoint 설정 필요");
        }
        if (matching.Count > 1)
        {
            warningCount++;
            report.AppendLine($"    ⚠ {tag}가 {matching.Count}개: 예전엔 모두 {oldPoint}로, 이제는 각자 시작 위치로 리스폰됨");
        }
    }

    private static bool IsExcluded(string path)
    {
        return path.Contains("/_Recovery/");
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
