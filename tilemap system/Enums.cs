using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace tilemap_system
{
    public class DepthLayers
    {
        public const float WorldMin = 0f;
        public const float WorldMax = 0.099f;
        public const float ContainerBox = 0.1f;
        public const float SelectedItemHighlight = 0.105f;
        public const float ContainerItem = 0.11f;
        public const float ContainerString = 0.12f;
        public const float MouseItem = 0.13f;
        public const float MouseItemString = 0.14f;
        public const float PauseScreenFilter = 0.2f;
        public const float PauseMenuSliderBackGround = 0.21f;
        public const float PauseMenuSliderForeGround = 0.22f;
        public const float PauseMenuSliderString = 0.23f;
    }
    [Flags]
    public enum ToolProficiency : byte
    {
        Pickaxe = 1 << 0,
        Axe = 1 << 1,
        Shovel = 1 << 2,
    }
    public enum GameMode
    {
        Survival,
        Creative
    }
    public enum Keybind
    {
        up = Keys.W,
        down = Keys.S,
        Left = Keys.A,
        Right = Keys.D,
        Jump = Keys.Space,
        sneak = Keys.LeftShift,
        inventory = Keys.E,
        pause = Keys.Escape,
        DropItem = Keys.Q
    }
    public enum ItemValue
    {
        Empty = 0,
        Grass = 1,
        Stone = 2,
        DiamondBlock = 3,
        StonePickaxe = 4,
        IronPickaxe = 5,
    }
    public enum GameState
    {
        Playing,
        Paused,
        Inventory
    }

}
