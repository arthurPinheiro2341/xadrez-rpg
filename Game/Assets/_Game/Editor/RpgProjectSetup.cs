using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XadrezRpg.Combat;
using XadrezRpg.Turns;

namespace XadrezRpg.Editor
{
    public static class RpgProjectSetup
    {
        [MenuItem("Xadrez RPG/Configure RPG")]
        public static void Configure()
        {
            const string directory = "Assets/_Game/Resources";
            if (!AssetDatabase.IsValidFolder(directory)) AssetDatabase.CreateFolder("Assets/_Game", "Resources");
            string path = directory + "/RpgBalance.asset";
            var balance = AssetDatabase.LoadAssetAtPath<RpgBalance>(path);
            if (balance == null)
            {
                balance = ScriptableObject.CreateInstance<RpgBalance>();
                AssetDatabase.CreateAsset(balance, path);
            }
            PieceSpriteImportSettings.ConfigureAll();
            var scene = EditorSceneManager.OpenScene(BoardSceneCreator.ScenePath);
            foreach (var root in scene.GetRootGameObjects())
                foreach (var controller in root.GetComponentsInChildren<ChessGameController>(true))
                {
                    var serialized = new SerializedObject(controller);
                    serialized.FindProperty("rpgBalance").objectReferenceValue = balance;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("RPG configured: balance, scene and class sprites.");
        }
    }
}
