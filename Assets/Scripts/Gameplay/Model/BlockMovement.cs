using UnityEngine;

namespace Gameplay.Model
{
    public enum BlockMovement
    {
        Free,
        [InspectorName("Up-Down")] UpDown,
        [InspectorName("Left-Right")] LeftRight
    }
}
