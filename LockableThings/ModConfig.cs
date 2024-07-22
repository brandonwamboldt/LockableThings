using StardewModdingAPI;

namespace LockableThings
{
    public class ModConfig
    {
        public SButton UnlockKeybind { get; set; } = SButton.LeftControl;
        public bool ToggleLock { get; set; } = false;
        public bool LockItemSigns { get; set; } = true;
        public bool LockTextSigns { get; set; } = true;
        public bool LockCrabPots { get; set; } = true;
        public bool LockFloors { get; set; } = true;
        public bool LockDecorations { get; set; } = false;
        public bool LockFurniture { get; set; } = false;
    }
}
