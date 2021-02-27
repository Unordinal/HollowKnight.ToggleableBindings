using System;
using System.Reflection;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    internal static class EmbeddedAssetLoader
    {
        public static Sprite UnknownBindingDefault { get; }

        public static Sprite UnknownBindingSelected { get; }

        private static Vector2 Half => new Vector2(0.5f, 0.5f);

        static EmbeddedAssetLoader()
        {
            var unknownDefaultTex = LoadAsTexture2D("GG_UI_pieces_unk_off.png");
            UnknownBindingDefault = Sprite.Create(unknownDefaultTex, GetRectForTexture(unknownDefaultTex), Half, 64);
            
            var unknownSelectedTex = LoadAsTexture2D("GG_UI_pieces_unk_on.png");
            UnknownBindingSelected = Sprite.Create(unknownSelectedTex, GetRectForTexture(unknownSelectedTex), Half, 64);
        }

        private static Texture2D LoadAsTexture2D(string assetPath)
        {
            try
            {
                var asm = Assembly.GetCallingAssembly();
                using (var stream = asm.GetManifestResourceStream($"{nameof(ToggleableBindings)}.Assets.{assetPath}"))
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);

                    Texture2D texture2D = new Texture2D(2, 2);
                    bool success = texture2D.LoadImage(data);
                    if (!success)
                        throw new Exception("ImageConversion.LoadImage() failed.");

                    return texture2D;
                }
            }
            catch (Exception ex)
            {
                ToggleableBindings.Instance.LogError("Couldn't load embedded resource: " + ex.Message);
                throw;
            }
        }

        private static Rect GetRectForTexture(Texture2D texture)
        {
            return new(0f, 0f, texture.width, texture.height);
        }
    }
}