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
        var spritesFolderPath = "Assets/Resources/Board/Dungeon"; // TODO
        var prefabsFolderPath = "Assets/Resources/Test/"; // TODO

        var foldersNames = GetFolderNames(spritesFolderPath);
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

    private static IList<string> GetFolderNames(string path)
    {
        var dir = new DirectoryInfo(path);
        var info = dir.GetDirectories();

        return info.Select(i => i.Name).ToList();
    }

    private static IList<string> GetFilesNames(string path)
    {
        var dir = new DirectoryInfo(path);
        var info = dir.GetFiles();

        return info.Select(i => i.Name).ToList();
    }

    private static List<Sprite> LoadSprites(string path)
    {
        var sprites = new List<Sprite>();
        var spritesNames = GetFilesNames(path);
        
        foreach (var spriteName in spritesNames)
        {
            sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(path + spriteName));
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