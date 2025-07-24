using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tilemap_system
{
    internal struct TileInfo
    {
        public static TileInfo[] _tileInfo; //this array is used to store all tileinfo in the game, like a dictionary

        string _name;
        Color _textures; // color temporarily as i do not have texture support
        int _MineTime; // color temporarily as i do not have texture support

        public static bool operator ==(TileInfo a, TileInfo b) => a.GetName() == b.GetName();
        
        public static bool operator !=(TileInfo a, TileInfo b) => !(a.GetName() == b.GetName());
        

        public TileInfo(string name, Color texture, int MineTime)
        {
            _name = name;
            _textures = texture;
            _MineTime = MineTime;
        }
        public static void ItemSetup(TileInfo[] tiles)
        {
            _tileInfo = new TileInfo[tiles.Count()];
            for (int i = 0; i < tiles.Count(); i++)
            {
                TileInfo._tileInfo[i] = tiles[i];
            }
        }
        public readonly string GetName() => _name;
        public readonly Color GetTexture() => _textures;
        public readonly int GetMineTime() => _MineTime;
    }
}
