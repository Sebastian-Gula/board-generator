using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{
    [MenuItem("Generate/Prefabs")]
    public static void GeneratePrefabs()
    {
        var dictionaryInfoHelper = new DictionaryInfoHelper();
        var spritesFolderPath = "Assets/Entities/BoardGenerators/Dungeon/Sprites/";
        var prefabsFolderPath = "Assets/Resources/Board/Dungeon/";
        var foldersNames = dictionaryInfoHelper.GetFolderNames(spritesFolderPath);

        foreach (var folderName in foldersNames)
        {
            var spritesPath = spritesFolderPath + folderName;
            var prefabsPath = prefabsFolderPath + folderName;

            if (!Directory.Exists(prefabsPath))
            {
                Directory.CreateDirectory(prefabsPath);
            }

            var sprites = LoadSprites(spritesPath);
            CreatePrefabs(prefabsPath, sprites);
        }
    }

    private static List<Sprite> LoadSprites(string path)
    {
        var dictionaryInfoHelper = new DictionaryInfoHelper();
        var sprites = new List<Sprite>();
        var spritesNames = dictionaryInfoHelper
            .GetFilesNames(path)
            .Where(name => name.EndsWith(".png")).ToList();
        
        foreach (var spriteName in spritesNames)
        {
            sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(path + "/" + spriteName));
        }

        return sprites;
    }

    private static void CreatePrefabs(string prefabsPath, List<Sprite> sprites)
    {
        foreach (var sprite in sprites)
        {
            var gameObject = new GameObject();
            gameObject.AddComponent(typeof(SpriteRenderer));
            var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabsPath + "/" + sprite.name + ".prefab");
            DestroyImmediate(gameObject);
        }
    }
}