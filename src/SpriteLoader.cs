using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static RiftAllseer.Plugin;

namespace RiftAllseer {
    public static class SpriteLoader {
        public static Dictionary<string, Sprite> LoadedSprites;
        public static string basePath = "";
        public static void Setup(){
            LoadedSprites = new Dictionary<string, Sprite>();
        }

        public static void SetBasePath(string path){
            basePath = path;
        }
        public static void LoadSprite(string imageFile, string spriteName, int imgHeight, int imgWidth, Vector2? pivot = null, float pixelsPerUnit=50.0f){
            pivot ??= new Vector2(0.5f, 0.5f); // default pivot is center of sprite.
            string fullImagePath = Path.Join(basePath, imageFile);

            if (!File.Exists(fullImagePath)){
                Log.LogError($"Unable to find file at '{fullImagePath}'");
                return;
            }

            Log.LogInfo($"Loading {spriteName} at '{fullImagePath}'");
            Texture2D customTexture = new Texture2D(imgHeight, imgWidth, GraphicsFormat.R8G8B8A8_UNorm, 1, TextureCreationFlags.None);

            try {
                customTexture.LoadImage(File.ReadAllBytes(fullImagePath));
            } catch (Exception e) {
                Log.LogError($"Failed to load file at '{fullImagePath}'");
                Log.LogError($"{e.ToString()}");
                return;
            }

            Rect rect = new Rect(0, 0, customTexture.width, customTexture.height);
            
            // TODO automatically adjust the pixelsToUnits
            Sprite customSprite = Sprite.Create(customTexture, rect, pivot.Value, pixelsPerUnit);

            if (LoadedSprites.ContainsKey(spriteName)){
                Log.LogWarning($"Overriding '{spriteName}' with new sprite. Are you sure you wanted to do this?");
            }
            LoadedSprites.Add(spriteName, customSprite);
        }

        public static Sprite GetSprite(string spriteName){
            if (LoadedSprites.ContainsKey(spriteName)){
                return LoadedSprites[spriteName];
            } else {
                return null;
            }
        }
    }

}