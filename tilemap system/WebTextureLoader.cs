using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tilemap_system
{
    public class MinecraftTextures
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10),
            DefaultRequestHeaders =
            {
                { "User-Agent", "MinecraftTextureLoader/1.0" }
            }
        };

        private GraphicsDevice _graphicsDevice;
        private Texture2D _errorTexture;

        // Initialize the texture loader with required MonoGame components
        public MinecraftTextures(GraphicsDevice graphicsDevice, Texture2D errorTexture)
        {
            _graphicsDevice = graphicsDevice;
            _errorTexture = errorTexture;
        }

        // Get a Minecraft texture by its name (e.g. "stone", "diamond_sword")
        public async Task<Texture2D> GetTextureAsync(string itemName)
        {
            if (_graphicsDevice == null)
                throw new InvalidOperationException("Call Initialize() first");

            // Try different texture URL's
            string[] possibleUrls =
            {
                $"https://minecraft.wiki/images/Invicon_{ToTitleCase(itemName)}.png?format=original",
                $"https://minecraft.wiki/images/{itemName.Replace(" ", "_")}_JE5.png",
                $"https://minecraft.wiki/images/Invicon_{ToTitleCase(itemName)}.png",
                $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/1.20.4/assets/minecraft/textures/item/{itemName.Replace(" ", "_")}.png"
            };

            foreach (var url in possibleUrls)
            {
                try
                {
                    byte[] imageData = await _httpClient.GetByteArrayAsync(url);
                    using (var stream = new System.IO.MemoryStream(imageData))
                    {
                        return Texture2D.FromStream(_graphicsDevice, stream);
                    }
                }
                catch
                {
                    continue; // Try next URL 
                }
            }

            return _errorTexture;
        }

        private static string ToTitleCase(string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str).Replace(" ", "_");
        }
    }
}