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
    internal class StorageInfo
    {
        ItemValue[] _ItemValues;
        int[] _stackSizes;
        static int _maxStackSize = 64;
        static List<StorageSlot> _interactList = new(); //list of thinks to be interacted with when holding down the mouse
        static StorageSlot _currentSlot = new(); //slot currently being hovered
        public enum ItemValue
        {
            Empty = 0,
            Grass = 1,
            Stone = 2,
        }
        public StorageInfo(int storageCapacity)
        {
            _ItemValues = new ItemValue[storageCapacity];
            _stackSizes = new int[storageCapacity];
            for (int i = 0; i < storageCapacity; i++)
            {
                _ItemValues[i] = (ItemValue)(i % 3);
                _stackSizes[i] = 32;
                if (_ItemValues[i] == ItemValue.Empty)
                {
                    _stackSizes[i] = 0;
                }
            }
        }
        public void CheckSlotInteractions(MouseState mouseState, MouseState previousMouseState, Rectangle[] inventoryRects, ref StorageInfo mouseHeldItem) // check for interactions with storage containter
        {
            if (General.OnDoubleClick(mouseState, previousMouseState))
            {
                for (int i = 0; i < inventoryRects.Length; i++)
                {
                    if (getItemValue(i) == mouseHeldItem.getItemValue(0))
                    {
                        RemoveItems(i, mouseHeldItem.AddItems(0, getStackCount(i), getItemValue(i)));
                    }
                }
            }
            if (General.OnLeftPress(mouseState, previousMouseState))
            {
                for (int i = 0; i < inventoryRects.Length; i++)
                {
                    if (inventoryRects[i].Contains(mouseState.X, mouseState.Y))
                    {
                        LeftClickInteract(i, ref mouseHeldItem, 0);
                    }
                }
            }
            else if (General.OnRightReleased(mouseState, previousMouseState) && _interactList.Count > 0)
            {
                foreach (StorageSlot storageSlot in _interactList)
                {
                    StorageInfo storageInfo = storageSlot.GetStorageContainer();
                    if (storageInfo.getItemValue(storageSlot.GetIndex()) == ItemValue.Empty || storageInfo.getItemValue(storageSlot.GetIndex()) == mouseHeldItem.getItemValue(0))
                    {
                        mouseHeldItem.RemoveItems(0, storageInfo.AddItems(storageSlot.GetIndex(), 1, mouseHeldItem.getItemValue(0)));
                    }
                }
                _interactList.Clear();
            }
            else if (previousMouseState.RightButton == ButtonState.Pressed)
            {
                for (int i = 0; i < inventoryRects.Length; i++)
                {
                    if (inventoryRects[i].Contains(mouseState.X, mouseState.Y))
                    {
                        if (_currentSlot != new StorageSlot(this, i))
                        {
                            _currentSlot = new StorageSlot(this, i);

                            if (mouseHeldItem.getItemValue(0) != StorageInfo.ItemValue.Empty)
                            {
                                StorageSlot thisStorageSlot = new(this, i);
                                if (!_interactList.Contains(thisStorageSlot))
                                {
                                    _interactList.Add(thisStorageSlot);
                                }
                            }
                        }
                        if (mouseState.RightButton == ButtonState.Released && _interactList.Count == 0)
                        {
                            RightClickInteract(i, ref mouseHeldItem, 0);
                        }
                    }
                }
            }
        }
        public int RemoveItems(int index, int amount)
        {
            int initialStackSize = _stackSizes[index];
            _stackSizes[index] -= amount;
            if (_stackSizes[index] <= 0 || StorageInfo.ItemValue.Empty == _ItemValues[index])
            {
                _ItemValues[index] = StorageInfo.ItemValue.Empty;
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
            if (StorageInfo.ItemValue.Empty == _ItemValues[index]) //for safety
            {
                _stackSizes[index] = 0;
            }
            return _stackSizes[index] - initialStackSize;
        }
        private void TransferTo(ref StorageInfo source, int sourceIndex, int destinationIndex, int amount)
        {
            if (source.getItemValue(sourceIndex) == getItemValue(destinationIndex))
                source.RemoveItems(sourceIndex, AddItems(destinationIndex, amount, source.getItemValue(sourceIndex)));
            else
            {
                ItemValue item = _ItemValues[destinationIndex];
                int stackSize = _stackSizes[destinationIndex];

                RemoveItems(destinationIndex, getStackCount(destinationIndex));
                AddItems(destinationIndex, source.getStackCount(sourceIndex), source.getItemValue(sourceIndex));

                source.RemoveItems(sourceIndex, source.getStackCount(sourceIndex));
                source.AddItems(sourceIndex, stackSize, item);
            }
        }
        private void TransferFrom(ref StorageInfo destination, int sourceIndex, int destinationIndex, int amount)
        {
            if (getItemValue(sourceIndex) == destination.getItemValue(destinationIndex))
            {
                RemoveItems(sourceIndex, destination.AddItems(destinationIndex, amount, getItemValue(sourceIndex)));
            }
            else
            {
                ItemValue item = destination.getItemValue(destinationIndex);
                int stackSize = destination.getStackCount(destinationIndex);

                destination.RemoveItems(destinationIndex, destination.getStackCount(destinationIndex));
                destination.AddItems(destinationIndex, getStackCount(sourceIndex), getItemValue(sourceIndex));

                RemoveItems(sourceIndex, getStackCount(sourceIndex));
                AddItems(sourceIndex, stackSize, item);
            }
        }
        private void LeftClickInteract(int ContainerIndex, ref StorageInfo MouseHeldItem, int MouseIndex)
        {

            if (MouseHeldItem.getItemValue(MouseIndex) != StorageInfo.ItemValue.Empty)
            {
                TransferTo(ref MouseHeldItem, MouseIndex, ContainerIndex, MouseHeldItem.getStackCount(MouseIndex));
            }
            else
            {
                TransferFrom(ref MouseHeldItem, ContainerIndex, MouseIndex, getStackCount(ContainerIndex));
            }
        }
        private void RightClickInteract(int ContainerIndex, ref StorageInfo mouseHeldItem, int MouseIndex)
        {
            if (mouseHeldItem.getItemValue(0) != ItemValue.Empty)
            {
                TransferTo(ref mouseHeldItem, MouseIndex, ContainerIndex, 1);
            }
            else if (mouseHeldItem.getItemValue(0) == ItemValue.Empty && getItemValue(ContainerIndex) != StorageInfo.ItemValue.Empty)
            {
                RemoveItems(ContainerIndex, mouseHeldItem.AddItems(MouseIndex, (int)Math.Ceiling(getStackCount(ContainerIndex) / 2f), getItemValue(ContainerIndex)));
            }
            //else
            //{
            //    TransferFrom(ref MouseHeldItem, ContainerIndex, MouseIndex, getStackCount(ContainerIndex));
            //}
        }
        private static void Interact(ref StorageInfo A, int Aindex, ref StorageInfo B, int Bindex)
        {
            if (A.getItemValue(0) != StorageInfo.ItemValue.Empty)
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
        public string GetItemName(int index) => Item._items[(int)_ItemValues[index]].ItemName();
        public Texture2D GetItemTexture(int index) => Item._items[(int)_ItemValues[index]].ItemTexture();
        public int? GetBlockID(int index) => Item._items[(int)_ItemValues[index]].ItemBlockID();
        public static List<StorageSlot> getInteractList { get => _interactList; }
    }
}
