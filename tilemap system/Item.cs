using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace tilemap_system
{
    internal struct Item
    {
        public static Item[] _items; //this array is used to store all items in the game, like a dictionary

        string _name;
        Texture2D _textures;
        int? _blockID;

        public static bool operator ==(Item a, Item b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Item a, Item b)
        {
            return !a.Equals(b);
        }

        public Item(string name, Texture2D texture, int? blockID)
        {
            _name = name;
            _textures = texture;
            _blockID = blockID;
        }
        public static void ItemSetup(Item[] items)
        {
            _items = new Item[items.Count()];
            for (int i = 0; i < items.Count(); i++)
            {
                Item._items[i] = items[i];
            }
        }
        public string itemName() => _name;
        public Texture2D itemTexture() => _textures;
        public int? itemBlockID() => _blockID;
    }
}
