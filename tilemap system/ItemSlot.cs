using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tilemap_system
{
    internal struct ItemSlot
    {
        Item _itemType;
        int _amount;
        public Item ItemType { get => _itemType; }
        public int Amount { get => _amount; }

        public static bool operator ==(ItemSlot a, ItemSlot b)
        {
            return a.ItemType == b.ItemType && a.Amount == b.Amount;
        }
        public static bool operator !=(ItemSlot a, ItemSlot b)
        {
            return !(a == b);
        }
        public static ItemSlot operator +(ItemSlot a, int b)
        {
            return new ItemSlot(a.ItemType, a.Amount + b);
        }
        public static ItemSlot operator -(ItemSlot a, int b)
        {
            if (a.Amount == b)
            {
                return new ItemSlot(Item.GetItem(ItemValue.Empty), 0);
            }
            else if (a.Amount > b)
            {
                return new ItemSlot(a.ItemType, a.Amount - b);
            }
            throw new InvalidOperationException("Cannot subtract more than available amount.");
        }

        public void DrawSlot(SpriteBatch spriteBatch, Texture2D backgroundTexture, SpriteFont font, Rectangle rectangle, Color squareColor)
        {
            spriteBatch.Draw(backgroundTexture, rectangle, null, squareColor, 0, new(), 0, DepthLayers.ContainerBox);
            if (ItemType != Item.GetItem(ItemValue.Empty))
            {
                if (ItemType.ItemTexture != null)
                {
                    spriteBatch.Draw(ItemType.ItemTexture, rectangle, null, Color.White, 0, new(), 0, DepthLayers.ContainerItem);
                }
                if (Amount > 1)
                {
                    spriteBatch.DrawString(font, Amount.ToString(), new(rectangle.Left, rectangle.Top), Color.White, 0, new(), 1, SpriteEffects.None, DepthLayers.ContainerString);
                }
            }
        }
        public void DrawSlot(SpriteBatch spriteBatch, SpriteFont font, Rectangle rectangle)
        {
            if (ItemType != Item.GetItem(ItemValue.Empty))
            {
                if (ItemType.ItemTexture != null)
                {
                    spriteBatch.Draw(ItemType.ItemTexture, rectangle, null, Color.White, 0, new(), 0, DepthLayers.MouseItem);
                }
                if (Amount > 1)
                {
                    spriteBatch.DrawString(font, Amount.ToString(), new(rectangle.Left, rectangle.Top), Color.White, 0, new(), 1, SpriteEffects.None, DepthLayers.MouseItemString);
                }
            }
        }

        public static void NormalInteract(ref ItemSlot source, ref ItemSlot destination)
        {
            if (destination.ItemType == Item.GetItem(ItemValue.Empty))
            {
                // if destination is empty, copy source to destination
                destination = source;
                source = new ItemSlot(Item.GetItem(ItemValue.Empty), 0);
            }
            else if (source.ItemType == Item.GetItem(ItemValue.Empty))
            {
                // if source is empty, copy destination to source
                source = destination;
                destination = new ItemSlot(Item.GetItem(ItemValue.Empty), 0);
            }
            else if (destination.Amount == destination.ItemType.MaxStackSize || source.ItemType != destination.ItemType)
            {
                // swap
                (source, destination) = (destination, source);
            }
            else // the only possible scenario is the items are the same and destination is not full
            {
                // so move items from the source to the destination
                Transfer(ref source, ref destination, source.Amount);
            }
        }
        public static void AlternateInteract(ref ItemSlot source, ref ItemSlot destination, bool transferOnly = false)
        {
            if (source.ItemType == Item.GetItem(ItemValue.Empty))
            {
                // reverse transfer and split
                Transfer(ref destination, ref source, (int)MathF.Ceiling(destination.Amount / 2f));
            }
            else if (destination.ItemType == Item.GetItem(ItemValue.Empty) || source.ItemType == destination.ItemType || transferOnly)
            {
                // if destination is empty, transfer one item
                source.TransferOne(ref destination);
            }
        }


        public void TransferOne(ref ItemSlot destination) //method transfer one item to avoid overflows
        {
            if ((this.ItemType == destination.ItemType || Item.GetItem(ItemValue.Empty) == destination.ItemType) && this.Amount > 0 && destination.Amount < destination.ItemType.MaxStackSize)
            {
                Transfer(ref this, ref destination, 1);
            }
        }
        public static void Transfer(ref ItemSlot source, ref ItemSlot destination, int transferAmount) //method transfer to avoid overflows
        {
            /* safety
            if (source.ItemValue != destination.ItemValue)
            {
                return;
            }
            */
            int stackSize = source.ItemType.MaxStackSize;

            // Calculate maximum possible transfer
            int maxSourceCanGive = source.Amount;
            int maxDestinationCanTake = stackSize - destination.Amount;

            // Determine actual safe transfer amount
            int safeTransfer = Math.Min(transferAmount, maxSourceCanGive);
            safeTransfer = Math.Min(safeTransfer, maxDestinationCanTake);
            safeTransfer = Math.Max(safeTransfer, 0); // Ensure not negative

            // Execute transfer
            if (destination.Amount == 0)
            {
                destination._itemType = source.ItemType; // Initialize destination if it was empty
            }
            source = source - safeTransfer;
            destination = destination + safeTransfer;
        }

        public DroppedItem DropOne(Vector3 position, Vector3 speed)
        {
            /* 
             * Here you would typically add the dropped item to a global list of dropped items
             * For example: DroppedItems.Add(myItemSlot.Drop);
             */

            var returnvalue = new DroppedItem(ItemType, 1, position, speed);
            _amount = _amount - 1;
            if (_amount == 0)
            {
                _itemType = Item.GetItem(ItemValue.Empty); // Clear the slot if no items left
            }
            return returnvalue;
        }
        public DroppedItem Drop(Vector3 position, Vector3 speed)
        {
            /* 
             * Here you would typically add the dropped item to a global list of dropped items
             * For example: DroppedItems.Add(myItemSlot.Drop);
             */

            var returnvalue = new DroppedItem(ItemType, Amount, position, speed);
            _itemType = Item.GetItem(ItemValue.Empty);
            _amount = 0; // Clear the slot after dropping
            return returnvalue;
        }
        public ItemSlot(Item item, int amount)
        {
            _itemType = item;
            _amount = amount;
        }
        public ItemSlot()
        {
            _itemType = Item.GetItem(ItemValue.DiamondBlock);
            _amount = 5;
        }
    }
}
