using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace tilemap_system
{
    internal class storageInfo
    {
        Item[] _storageSlots;
        int[] _stackSize;
        static int _maxStackSize = 64;
        public storageInfo(int storageSlots)
        {
            _storageSlots = new Item[storageSlots];
            _stackSize = new int[storageSlots];
            for (int i = 0; i < storageSlots; i++)
            {
                _storageSlots[i] = Item._items[i % 3];
                _stackSize[i] = 40;
            }
        }
        public int RemoveItems(int index, int amount)
        {
            int initialStackSize = _stackSize[index];
            _stackSize[index] -= amount;
            if (_stackSize[index] <= 0)
            {
                _storageSlots[index] = Item._items[0];
                _stackSize[index] = 0;
            }
            return initialStackSize - _stackSize[index];
        }
        public int AddItems(int index, int amount, Item item)
        {
            int initialStackSize = _stackSize[index];
            if (_stackSize[index] == 0)
            {
                _storageSlots[index] = item;
            }
            _stackSize[index] = Math.Min(_maxStackSize, _stackSize[index] + amount);
            return _stackSize[index] - initialStackSize;
        }
        public void TransferTo(ref storageInfo source, int sourceIndex, int destinationIndex, int amount)
        {
            if (source.getItem(sourceIndex) == getItem(destinationIndex))
                source.RemoveItems(sourceIndex, AddItems(destinationIndex, amount, source.getItem(sourceIndex)));
            else
            {
                Item item = _storageSlots[destinationIndex];
                int stackSize = _stackSize[destinationIndex];

                //_storageSlots[destinationIndex] = source.getItem(sourceIndex);
                //_stackSize[destinationIndex] = source.getStackCount(sourceIndex);
                RemoveItems(destinationIndex, getStackCount(destinationIndex));
                AddItems(destinationIndex, source.getStackCount(sourceIndex), source.getItem(sourceIndex));

                source.RemoveItems(sourceIndex, source.getStackCount(sourceIndex));
                source.AddItems(sourceIndex, stackSize, item);
            }
        }
        public static void Interact(ref storageInfo A, int Aindex, ref storageInfo B, int Bindex)
        {
            if (A.getItem(0) != Item._items[0])
            {
                B.TransferTo(ref A, Aindex, Bindex, A.getStackCount(Aindex));
            }
            else
            {
                A.TransferTo(ref B, Bindex, Aindex, B.getStackCount(Bindex));
            }
        }


        public int Count => _storageSlots.Count();
        public int getStackCount(int index) => _stackSize[index];
        public Item getItem(int index) => _storageSlots[index];
        public string GetItemName(int index) => _storageSlots[index].itemName();
        public Texture2D GetItemTexture(int index) => _storageSlots[index].itemTexture();
        public int? GetBlockID(int index) => _storageSlots[index].itemBlockID();
    }
}
