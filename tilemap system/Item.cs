using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static tilemap_system.Tile;

namespace tilemap_system
{
    internal class Item
    {
        string _name;
        Texture2D _texture;
        int _maxStackSize;

        public static List<(Texture2D, string)> TexturesToSave { get; } = new List<(Texture2D, string)>(); // used to save textures to disk after loading them from the web
        public static MinecraftTextures _TextureLoader; // used to load textures from the web

        private static readonly string _LogDirectory = Path.Combine(Environment.CurrentDirectory, "HomemadeMinecraft", "assets", "textures", "items"); //used to save textures once loaded
        private static GraphicsDevice _graphicsDevice;
        public struct ToolStats
        {
            static readonly float[] mineSpeeds = new float[] { 1.5f, 1.8f, 3, 2.3f, 2.5f, 3 };
            public static float GetMineSpeed(Tiers tier) => mineSpeeds[(int)tier];
            public enum Tiers {None = 0, wood = 1, stone = 2, iron = 3, gold = 4, diamond = 5, netherite = 6 }
        }
        public static Item[] _itemDictionary; //this array is used to store all items in the game, like a dictionary

        public static bool operator ==(Item a, Item b)
        {
            return a.ItemName == b.ItemName;
        }
        public static bool operator !=(Item a, Item b)
        {
            return !(a == b);
        }

        /* Old initialization code
        public static void ItemSetup(Item[] items)
        {
            _items = new Item[items.Count()];
            for (int i = 0; i < items.Count(); i++)
            {
                Item._items[i] = items[i];
            }
        }
        */
        public static void Initialize(GraphicsDevice graphicsDevice, Texture2D errorTexture)
        {
            _TextureLoader = new MinecraftTextures(graphicsDevice, errorTexture);
            _graphicsDevice = graphicsDevice;
            if (!File.Exists(_LogDirectory))
            {
                Directory.CreateDirectory(_LogDirectory);
                Debug.WriteLine($"Created directory: {_LogDirectory}");
            }

            // ensure the enum values align with the item dictionary enum
            _itemDictionary = new Item[]
            {
                /* Null Item */
                new Empty(),

                /* blocks */
                new Grass(),
                new Stone(),
                new DiamondBlock(),


                /* tools */
                new StonePickaxe(),
                new IronPickaxe()
            };
        }
        public Item(string name, int maxStackSize = 64)
        {
            _name = name;
            _maxStackSize = maxStackSize;
            LoadTextureFromWebAsync(name);
        }
        async Task LoadTextureFromWebAsync(string archiveItemName)
        {
            string saveDirectory = Path.Combine(_LogDirectory, $"ITEM {archiveItemName.Replace(" ", "_")}.png");

            if (File.Exists(saveDirectory))
            {
                _texture = Texture2D.FromFile(_graphicsDevice, saveDirectory); // loads texture from disk if it exists
            }
            else
            {
                _texture = await _TextureLoader.GetTextureAsync(archiveItemName); // gets texture from online
                lock (TexturesToSave)
                {
                    TexturesToSave.Add((_texture, saveDirectory)); // adds it to the list to be saved later (so we dont have to find it online again)
                }
            }
            Debug.WriteLine($"{archiveItemName} loaded: {_texture != null}"); //debug, states if the texture was loaded successfully
        }
        public static void SaveTextures()
        {
            lock (TexturesToSave)
            {
                foreach ((Texture2D, string) tuple in TexturesToSave)
                {
                    Texture2D texture = tuple.Item1;
                    string saveDirectory = tuple.Item2;

                    using (Stream stream = File.Create(saveDirectory))
                    {
                        texture.SaveAsPng(stream, texture.Width, texture.Height);
                        stream.Flush();
                    }
                }
                TexturesToSave.Clear(); // Clear the list after saving
            }
        }
        public static Item GetItem(ItemValue value) => _itemDictionary[(int)value];
        public int MaxStackSize { get => _maxStackSize; }
        public string ItemName { get => _name; }
        public Texture2D ItemTexture { get => _texture; }
        public TileID? ItemBlockID { get => this is Block block ? block.BlockID : null; }
        public ToolProficiency? Proficiencies { get => this is Tool tool ? tool.Proficiencies : null; }
        public float? MineSpeedBonus { get => this is Tool tool ? tool.MineSpeedBonus : null; }
        public ToolStats.Tiers Tier { get => this is Tool tool ? tool.Tier : ToolStats.Tiers.None; }


        class Block : Item
        {
            TileID _blockID;
            public TileID BlockID { get => _blockID; }
            public Block(string name, TileID blockID) : base(
                name: name
                )
            {
                _blockID = blockID;
            }
        }
        class Tool : Item
        {
            ToolProficiency _proficiencies;
            float _mineSpeedMultiplier;
            ToolStats.Tiers _tier;
            public ToolProficiency Proficiencies { get => _proficiencies; }
            public float MineSpeedBonus { get => _mineSpeedMultiplier; }
            public ToolStats.Tiers Tier { get => _tier; }
            public Tool(string name, ToolProficiency proficiency, float mineSpeedMultiplier, ToolStats.Tiers tier) : base(
                name: name,
                maxStackSize: 1
                )
            {
                _proficiencies = proficiency;
                _mineSpeedMultiplier = mineSpeedMultiplier;
                _tier = tier;
            }
        }
        class Empty : Item
        {
            public Empty() : base(
                name: "Empty"
                )
            { }
        }

        class Grass : Block
        {
            public Grass() : base(
                name: "Grass Block",
                blockID: TileID.Grass
                )
            { }
        }
        class Stone : Block
        {
            public Stone() : base(
                name: "Stone",
                blockID: TileID.Stone
                )
            { }
        }
        class StonePickaxe : Tool
        {
            public StonePickaxe() : base(
                name: "Stone Pickaxe",
                proficiency: ToolProficiency.Pickaxe,
                mineSpeedMultiplier: ToolStats.GetMineSpeed(ToolStats.Tiers.stone),
                tier: ToolStats.Tiers.stone
                )
            { }

        }
        class DiamondBlock : Block
        {
            public DiamondBlock() : base(
                name: "Block Of Diamond",
                blockID: TileID.DiamondBlock
                )
            { }
        }
        class IronPickaxe : Tool
        {
            public IronPickaxe() : base(
                name: "Iron Pickaxe",
                proficiency: ToolProficiency.Pickaxe,
                mineSpeedMultiplier: ToolStats.GetMineSpeed(ToolStats.Tiers.iron),
                tier: ToolStats.Tiers.iron
                )
            { }
        }
    }
}
