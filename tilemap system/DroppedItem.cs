using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tilemap_system.tilemap_system;

namespace tilemap_system
{
    internal class DroppedItem
    {
        static int _drawSize = 128;

        ItemSlot _itemSlot;
        Vector3 _position;
        Vector3 _speed;
        DateTime _StartTime = DateTime.Now;
        Point _drawPos;
        int _finalDrawSize;
        float _distanceSquared;
        bool _shouldDraw = false;
        public ItemSlot GetItemSlot { get => _itemSlot; }
        public int GetLifespan => (int)(DateTime.Now - _StartTime).TotalMilliseconds;
        public Item ItemType { get => _itemSlot.ItemType; }
        public int Amount { get => _itemSlot.Amount; }
        public Vector3 Position { get => _position; }
        public Vector3 Speed { get => _speed; }

        public DroppedItem(Item itemType, int amount, Vector3 position, Vector3 speed = new())
        {
            _itemSlot = new ItemSlot(itemType, amount);
            _position = position;
            _speed = speed;
        }

        public void UpdateDrawInfo(
            Point screenSize,
            float FOV,
            Vector3 cameraPosition,
            float pitch,
            float yaw)
        {
            Vector3 Position = this.Position - new Vector3(0, 10, 3);
            Vector3 relativePos = Position - cameraPosition;

            Vector3 rotatedrelativePos = General.RotateVector(relativePos, yaw, pitch);

            if (rotatedrelativePos.Z < 0) { _shouldDraw = false; return; } // Object is behind the camera

            // 1. Calculate size (world units to screen units)
            float scale = 50 / rotatedrelativePos.Z;
            int finalSize = (int)(_drawSize * scale);

            if (finalSize < 4) { _shouldDraw = false; return; }

            // 2. calculate screen Position
            float fov_scale = 1f / MathF.Tan(FOV / 2);
            Point screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * fov_scale * (screenSize.X / 2) + (screenSize.X / 2)),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * fov_scale * (screenSize.Y / 2) + (screenSize.Y / 2))
            );

            // 3. Center the object
            screenPos.X -= finalSize / 2;
            screenPos.Y -= finalSize / 2;

            //update drawInfo
            _drawPos = screenPos;
            _finalDrawSize = finalSize;
            _distanceSquared = Vector3.DistanceSquared(cameraPosition, Position);
            _shouldDraw = true;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_shouldDraw)
                spriteBatch.Draw(
                    _itemSlot.ItemType.ItemTexture,
                    new Rectangle(_drawPos.X, _drawPos.Y, _finalDrawSize, _finalDrawSize),
                    null,
                    General.Multiply(Color.White, ((500f - MathF.Sqrt(_distanceSquared)) / 100000f)),
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    DepthLayers.WorldMin + DepthLayers.WorldMax / _distanceSquared
                );
        }
        public void Physics()
        {
            _speed.Y += 0.1f; // gravity++++
            _position += _speed;
            if (World.GetTileCopyFromIndex(Tile.GetTileIndex(_position), out Tile tile1) && tile1.Isfull) { _position -= _speed; _speed *= -0.1f; }
            //_position.X += _speed.X;
            //if (World.GetTileCopyFromIndex(Tile.GetTileIndex(_position), out Tile tile1) && tile1.Isfull) { _position.X -= _speed.X; _speed.X *= -0.1f; }
            //_position.Y += _speed.Y;
            //if (World.GetTileCopyFromIndex(Tile.GetTileIndex(_position), out Tile tile2) && tile2.Isfull) { _position.Y -= _speed.Y; _speed.Y *= -0.1f; }
            //_position.Z += _speed.Z;
            //if (World.GetTileCopyFromIndex(Tile.GetTileIndex(_position), out Tile tile3) && tile3.Isfull) { _position.Z -= _speed.Z; _speed.Z *= -0.1f; }

            _speed *= 0.99f; // Simulate friction
            if (_speed.Length() < 0.01f) // Stop if speed is very low
            {
                _speed = Vector3.Zero;
            }
        }
        public bool Pickup(ref Container[] Containers) //returns true if the dropped item is empty and should be destroyed
        {
            HashSet<Item> itemPallete = new();
            for (int k = 0; k < Containers.Length; k++)
            {
                foreach (Item item in Containers[k].itemPalette)
                {
                    itemPallete.Add(item);
                }
            }

            bool inventoryContainsItem = itemPallete.Contains(_itemSlot.ItemType);
            bool inventoryContainsEmpty = itemPallete.Contains(Item.GetItem(ItemValue.Empty));
            if (!inventoryContainsItem && !inventoryContainsEmpty)
            {
                return false; // Item type not in any container's palette
            }
            if (inventoryContainsItem)
            {
                for (int k = 0; k < Containers.Length; k++)
                {
                    if (Containers[k].itemPalette.Contains(_itemSlot.ItemType))
                    {
                        for (int i = 0; i < Containers[k].GetSlotCount; i++)
                        {
                            var inspectingSlot = Containers[k].GetSlot(i);
                            if (inspectingSlot.ItemType == ItemType && _itemSlot.ItemType.MaxStackSize != inspectingSlot.Amount)
                            {
                                ItemSlot.Transfer(ref _itemSlot, ref Containers[k].GetSlot(i), Amount);
                                if (_itemSlot.ItemType == Item.GetItem(ItemValue.Empty))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            if (inventoryContainsEmpty)
            {
                for (int k = 0; k < Containers.Length; k++)
                {
                    for (int i = 0; i < Containers[k].GetSlotCount; i++)
                    {
                        if (Containers[k].GetSlot(i).ItemType == Item.GetItem(ItemValue.Empty))
                        {
                            Containers[k].GetSlot(i) = _itemSlot;
                            _itemSlot = new ItemSlot(Item.GetItem(ItemValue.Empty), 0);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
