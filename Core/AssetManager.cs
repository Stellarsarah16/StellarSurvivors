using System.IO.Enumeration;

namespace StellarSurvivors.Core;
using Raylib_cs;
using System.Collections.Generic;
using System.IO;

public static class AssetManager
{
    private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    public static void LoadTextures(string textureDirectory = "Assets/Textures")
    {
        if (!Directory.Exists(textureDirectory))
        {
            Raylib.TraceLog(TraceLogLevel.Warning, $"[AssetManager] Texture directory not found: {textureDirectory}");
            return;
        }
        
        var textureFiles = Directory.GetFiles(textureDirectory, "*.png",  SearchOption.AllDirectories);
        
        Raylib.TraceLog(TraceLogLevel.Info, $"[AssetManager] Found {textureFiles.Length} .png files in directory.");

        foreach (var textureFile in textureFiles)
        {
            string name = Path.GetFileNameWithoutExtension(textureFile);
            
            if (_textures.ContainsKey(name))
            {
                Raylib.TraceLog(TraceLogLevel.Warning, $"[AssetManager] Duplicate texture file: {name}");
                continue;
            }
            
            Texture2D texture = Raylib.LoadTexture(textureFile);
            Raylib.SetTextureFilter(texture, TextureFilter.Point);
            _textures.Add(name, texture);
            Raylib.TraceLog(TraceLogLevel.Debug, $"[AssetManager] Loaded texture: {name} from {textureFile}");
        }
    }
    
    public static Texture2D GetTexture(string name)
    {
        if (_textures.TryGetValue(name, out Texture2D texture))
        {
            return texture;
        }

        // --- UPDATED ERROR LOG ---
        // This is the error you're seeing. We'll make it more helpful.
        Raylib.TraceLog(TraceLogLevel.Error, $"[AssetManager] Failed to find texture with key: '{name}'");

        // --- NEW LOG ---
        // This is the "dictionary dump." It will show us all keys that *are* available.
        if (_textures.Count == 0)
        {
            Raylib.TraceLog(TraceLogLevel.Warning, "[AssetManager] The texture dictionary is completely empty.");
        }
        else
        {
            string availableKeys = string.Join(", ", _textures.Keys);
            Raylib.TraceLog(TraceLogLevel.Warning, $"[AssetManager] Available texture keys are: [{availableKeys}]");
        }
        return default;
    }
    public static void UnloadAllTextures()
    {
        foreach (var texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }
        _textures.Clear();
        Raylib.TraceLog(TraceLogLevel.Info, "[AssetManager] Unloaded all textures.");
    }
}