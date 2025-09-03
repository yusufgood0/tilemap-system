using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using static System.Reflection.Metadata.BlobBuilder;

namespace tilemap_system
{
    internal class Container
    {
        ItemSlot[] _slots; // this array is used to store all items in the container, like a dictionary
        static List<(int, int)> _SlotsToProcess = new();
        public static List<(int, int)> SlotsToProcess { get => _SlotsToProcess; }

        public Container(int slotAmount)
        {
            _slots = new ItemSlot[slotAmount];
            for (int i = 0; i < slotAmount; i++)
            {
                _slots[i] = new ItemSlot(); // initialize each slot to empty
            }
        }

        static void ClearSlotsToProcess()
        {
            _SlotsToProcess.Clear();
        }
        public static void DrawAllSlots(SpriteBatch spriteBatch, Texture2D backgroundTexture, SpriteFont font, ref Container[] container, ref Rectangle[][] slotRects)
        {
            Color color;

            for (int i = 0; i < slotRects.Count(); i++)
            {
                for (int j = 0; j < slotRects[i].Length; j++)
                {
                    if (_SlotsToProcess.Count > 1 && _SlotsToProcess.Contains((i, j)))
                    {
                        color = Color.LightGreen; // highlight color for slots being processed
                    }
                    else
                    {
                        color = Color.LightSlateGray; // default color for slots not being processed
                    }
                    container[i]._slots[j].DrawSlot(spriteBatch, backgroundTexture, font, slotRects[i][j], color);
                }
            }

        }
        public static void CheckInteraction(ref ItemSlot source, ref Container[] container, ref Rectangle[][] slotRects, MouseState mouseState, MouseState previousMouseState)
        {
            Point point = new Point(mouseState.X, mouseState.Y);
            if (General.OnDoubleClick(mouseState, previousMouseState))
            {
                for (int k = 0; k < source.ItemType.MaxStackSize; k++)
                {
                    for (int i = 0; i < slotRects.Count(); i++)
                    {
                        for (int j = 0; j < slotRects[i].Length; j++)
                        {
                            if (container[i]._slots[j].ItemType == source.ItemType && container[i]._slots[j].Amount == k)
                            {
                                ItemSlot.Transfer(ref container[i]._slots[j], ref source, container[i]._slots[j].Amount);
                            }
                        }
                    }
                }
            }
            if (General.OnLeftReleased(mouseState, previousMouseState))
            {
                if (_SlotsToProcess.Count > 1) // if there are slots to process, process them
                {
                    // Find the corresponding container and slot index
                    while (source.Amount != 0)
                    {
                        foreach ((int, int) index in _SlotsToProcess)
                        {
                            // Transfer one item from source to the corresponding slot
                            source.TransferOne(ref container[index.Item1]._slots[index.Item2]);
                        }
                    }
                    _SlotsToProcess.Clear(); // clear the list after processing
                    return; // exit after processing all slots
                }
                for (int i = 0; i < slotRects.Count(); i++)
                {
                    for (int j = 0; j < slotRects[i].Length; j++)
                    {
                        if (slotRects[i][j].Contains(point))
                        {
                            ItemSlot.NormalInteract(ref source, ref container[i]._slots[j]);
                            _SlotsToProcess.Clear(); // clear the list after processing
                            return; // exit after interacting with the first slot found
                        }
                    }
                }
            }
            else if (General.OnRightReleased(mouseState, previousMouseState))
            {
                if (_SlotsToProcess.Count > 1) // if there are slots to process, process them
                {
                    // Find the corresponding container and slot index
                    foreach ((int, int) index in _SlotsToProcess)
                    {
                        // Transfer one item from source to the corresponding slot
                        source.TransferOne(ref container[index.Item1]._slots[index.Item2]);
                    }
                    _SlotsToProcess.Clear(); // clear the list after processing
                    return; // exit after processing all slots
                }
                for (int i = 0; i < slotRects.Count(); i++)
                {
                    for (int j = 0; j < slotRects[i].Length; j++)
                    {
                        if (slotRects[i][j].Contains(point))
                        {
                            ItemSlot.AlternateInteract(ref source, ref container[i]._slots[j]);
                            _SlotsToProcess.Clear(); // clear the list after processing
                            return; // exit after interacting with the first slot found
                        }
                    }
                }
            }
            else if (previousMouseState.RightButton == ButtonState.Pressed || previousMouseState.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < slotRects.Count(); i++)
                {
                    for (int j = 0; j < slotRects[i].Length; j++)
                    {
                        if (slotRects[i][j].Contains(point) && !_SlotsToProcess.Contains((i, j)))
                        {
                            _SlotsToProcess.Add((i, j));
                        }
                    }
                }
            }
        }
        public ref ItemSlot GetSlot(int index) => ref _slots[index];
        public HashSet<Item> itemPalette
        {
            get
            {
                HashSet<Item> itemValues = new();
                foreach (ItemSlot slot in _slots)
                {
                    itemValues.Add(slot.ItemType);
                }
                return itemValues;
            }
        }
        public int GetSlotCount => _slots.Length;
    }
}
