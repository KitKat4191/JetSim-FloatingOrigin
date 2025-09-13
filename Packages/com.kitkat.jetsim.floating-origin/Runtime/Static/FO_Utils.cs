
using UnityEngine;
using VRC.SDKBase;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    public static class FO_Utils
    {
        /// <summary>
        /// Teleports the <paramref name="player"/> to <paramref name="targetPos"/> without changing their rotation.
        /// </summary>
        /// <param name="player">The player to teleport, this has to be the local player.</param>
        /// <param name="targetPos">The position to teleport the player to.</param>
        /// <param name="lerpOnRemote">Whether to interpolate the player's movement for remote players.</param>
        public static void TeleportTo(this VRCPlayerApi player, Vector3 targetPos, bool lerpOnRemote = false)
        {
            VRCPlayerApi.TrackingData origin = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);

            Vector3 offset = player.GetPosition() - origin.position;
            Vector3 teleportPos = targetPos - offset;

            player.TeleportTo(teleportPos, origin.rotation,
                VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, lerpOnRemote);
        }
        
        #region ARRAY EXTENSIONS

        public static T[] AddUnique<T>(this T[] array, T item)
        {
            if (item == null || array.Contains(item)) return array;

            int length = array.Length;
            var newArray = new T[length + 1];

            array.CopyTo(newArray, 0);
            
            newArray[length] = item;

            return newArray;
        }

        public static T[] Remove<T>(this T[] array, T item)
        {
            int itemIndex = System.Array.IndexOf(array, item);
            if (itemIndex == -1) return array; // The item isn't in the array so we don't need to remove it.

            int length = array.Length;
            if (length <= 1) return new T[0];

            array[itemIndex] = default;
            // Put the last item in place of the empty slot to fill the gap.
            int newLength = length - 1;
            Swap(array, itemIndex, newLength);

            // Shrink the array by 1
            var newArray = new T[newLength];

            System.Array.Copy(array, newArray, newLength);
            return newArray;
        }

        public static void Swap<T>(this T[] array, int a, int b)
        {
            // ReSharper disable once SwapViaDeconstruction
            T temp = array[a];
            array[a] = array[b];
            array[b] = temp;
        }

        public static bool Contains<T>(this T[] array, T item) => System.Array.IndexOf(array, item) > -1;

        #endregion // ARRAY EXTENSIONS
    }
}
