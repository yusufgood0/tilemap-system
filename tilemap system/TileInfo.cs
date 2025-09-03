using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tilemap_system
{
    internal class TileInfo
    {
        public static TileInfo[] _tileDictionary; //this array is used to store all tileinfo in the game, like a dictionary

        string _name;
        Color _textures; // solid color temporarily as I do not have texture support
        int _MineTime; 
        ToolProficiency? _proficiencies; // list of proficiencies for this tile
        Item.ToolStats.Tiers _tier; // tier of the tile, used for mining speed and other properties

        public static bool operator ==(TileInfo a, TileInfo b) => a.GetName() == b.GetName();        
        public static bool operator !=(TileInfo a, TileInfo b) => !(a == b);

        public TileInfo(string name, Color texture, int mineTime, ToolProficiency? Proficiencies = null, Item.ToolStats.Tiers tier = Item.ToolStats.Tiers.None)
        {
            _name = name;
            _textures = texture;
            _MineTime = mineTime;
            _proficiencies = Proficiencies;
            _tier = tier;
        }
        /* Old initialization code
    public static void ItemSetup(TileInfo[] tiles)
    {
        _tileInfo = new TileInfo[tiles.Count()];
        for (int i = 0; i < tiles.Count(); i++)
        {
            TileInfo._tileInfo[i] = tiles[i];
        }
    }
        */
        public static void Initialize()
        {
            _tileDictionary = new TileInfo[]
            {
                new Empty(),
                new Grass(),
                new Stone(),
                new DiamondBlock(),
            };
        }
        public bool TierRequirmentsMet(Item.ToolStats.Tiers tier) => (tier >= this._tier);
        public bool HasProficiency(ToolProficiency proficiency) => (_proficiencies & proficiency) != 0;
        public static TileInfo[] TileDictionary => _tileDictionary; //this array is used to store all tileinfo in the game, like a dictionary
        public string GetName() => _name;
        public Color GetTexture() => _textures;
        public int GetMineTime() => _MineTime;

        class ContainerTile : TileInfo
        {
            Container _container { get; set; }
            public ContainerTile(int slotAmount, string name, Color texture, int mineTime, ToolProficiency? Proficiencies = null, Item.ToolStats.Tiers tier = Item.ToolStats.Tiers.None) : base(
                name: name,
                texture: texture,
                mineTime: mineTime,
                Proficiencies: Proficiencies,
                tier: tier
                )
            {
                _container = new Container(slotAmount);
            }
        }
        class Empty : TileInfo
        {
            public Empty() : base(
                name: "Empty",
                texture: Color.White,
                mineTime: 1
                )
            { }
        }
        class Grass : TileInfo
        {
            public Grass() : base(
                name:"Grass", 
                texture: Color.Green,
                mineTime: 1000, 
                Proficiencies: ToolProficiency.Shovel
                ) 
            { }
        }
        class Stone : TileInfo
        {
            public Stone() : base(
                name: "Stone",
                texture: Color.Gray,
                mineTime: 2500,
                Proficiencies: ToolProficiency.Pickaxe,
                tier: Item.ToolStats.Tiers.wood
                )
            { }
        }
        class DiamondBlock : TileInfo
        {
            public DiamondBlock() : base(
                name: "Diamond Block",
                texture: Color.LightSkyBlue,
                mineTime: 5000,
                Proficiencies: ToolProficiency.Pickaxe,
                tier: Item.ToolStats.Tiers.iron
                )
            { }
        }
        class Chest : ContainerTile
        {
            public Chest() : base(
                slotAmount: 27,
                name: "Chest",
                texture: Color.SandyBrown,
                mineTime: 2000,
                Proficiencies: ToolProficiency.Axe
                )
            { }
        }
    }
}
