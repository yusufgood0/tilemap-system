using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_system
{
    internal class storageInfo
    {
        ItemValue[] _ItemValues;
        int[] _stackSizes;
        static int _maxStackSize = 64;
        public enum ItemValue
        {
            Empty = 0,
            Grass = 1,
            Stone = 2,
        }
        public storageInfo(int storageCapacity)
        {
            _ItemValues = new ItemValue[storageCapacity];
            _stackSizes = new int[storageCapacity];
            for (int i = 0; i < storageCapacity; i++)
            {
                _ItemValues[i] = (ItemValue)(i % 3);
                _stackSizes[i] = 40;
            }
        }
        public void CheckSlotInteractions(MouseState mouseState, Rectangle[] inventoryRects, ref storageInfo mouseHeldItem)
        {
            for (int i = 0; i < inventoryRects.Length; i++)
            {
                if (inventoryRects[i].Contains(mouseState.X, mouseState.Y))
                {
                    Interact(i, ref mouseHeldItem, 0);
                }
            }
        }
        public int RemoveItems(int index, int amount)
        {
            int initialStackSize = _stackSizes[index];
            _stackSizes[index] -= amount;
            if (_stackSizes[index] <= 0)
            {
                _ItemValues[index] = storageInfo.ItemValue.Empty;
                _stackSizes[index] = 0;
            }
            return initialStackSize - _stackSizes[index];
        }
        private int AddItems(int index, int amount, ItemValue item)
        {
            int initialStackSize = _stackSizes[index];
            if (_stackSizes[index] == 0)
            {
                _ItemValues[index] = item;
            }
            _stackSizes[index] = Math.Min(_maxStackSize, _stackSizes[index] + amount);
            return _stackSizes[index] - initialStackSize;
        }
        private void TransferTo(ref storageInfo source, int sourceIndex, int destinationIndex, int amount)
        {
            if (source.getItemValue(sourceIndex) == getItemValue(destinationIndex))
                source.RemoveItems(sourceIndex, AddItems(destinationIndex, amount, source.getItemValue(sourceIndex)));
            else
            {
                ItemValue item = _ItemValues[destinationIndex];
                int stackSize = _stackSizes[destinationIndex];

                //_storageSlots[destinationIndex] = source.getItem(sourceIndex);
                //_stackSize[destinationIndex] = source.getStackCount(sourceIndex);
                RemoveItems(destinationIndex, getStackCount(destinationIndex));
                AddItems(destinationIndex, source.getStackCount(sourceIndex), source.getItemValue(sourceIndex));

                source.RemoveItems(sourceIndex, source.getStackCount(sourceIndex));
                source.AddItems(sourceIndex, stackSize, item);
            }
        }
        private void TransferFrom(ref storageInfo destination, int sourceIndex, int destinationIndex, int amount)
        {
            if (getItemValue(sourceIndex) == destination.getItemValue(destinationIndex))
                RemoveItems(sourceIndex, destination.AddItems(destinationIndex, amount, getItemValue(sourceIndex)));
            else
            {
                ItemValue item = _ItemValues[destinationIndex];
                int stackSize = _stackSizes[destinationIndex];

                destination.RemoveItems(destinationIndex, destination.getStackCount(destinationIndex));
                destination.AddItems(destinationIndex, getStackCount(sourceIndex), getItemValue(sourceIndex));

                RemoveItems(sourceIndex, getStackCount(sourceIndex));
                AddItems(sourceIndex, stackSize, item);
            }
        }
        private void Interact(int Bindex, ref storageInfo A, int Aindex)
        {
            if (A.getItemValue(0) != storageInfo.ItemValue.Empty)
            {
                TransferTo(ref A, Aindex, Bindex, A.getStackCount(Aindex));
            }
            else
            {
                TransferFrom(ref A, Bindex, Aindex, getStackCount(Bindex));

            }
        }
        private static void Interact(ref storageInfo A, int Aindex, ref storageInfo B, int Bindex)
        {
            if (A.getItemValue(0) != storageInfo.ItemValue.Empty)
            {
                B.TransferTo(ref A, Aindex, Bindex, A.getStackCount(Aindex));
            }
            else
            {
                A.TransferTo(ref B, Bindex, Aindex, B.getStackCount(Bindex));
            }
        } // static version of method


        public int Count => _ItemValues.Count();
        public int getStackCount(int index) => _stackSizes[index];
        public ItemValue getItemValue(int index) => _ItemValues[index];
        public string GetItemName(int index) => Item._items[(int)_ItemValues[index]].itemName();
        public Texture2D GetItemTexture(int index) => Item._items[(int)_ItemValues[index]].itemTexture();
        public int? GetBlockID(int index) => Item._items[(int)_ItemValues[index]].itemBlockID();
    }
}
