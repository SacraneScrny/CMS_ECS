using Unity.Entities;

namespace Logic.CMS.ECS
{
    public static class PrefabUtils
    {
        public static bool GetPrefab<T>(this DynamicBuffer<T> buffer, uint hashKey, out T prefab) where T : unmanaged, IBufferElementData, PrefabECS.IPrefabBuffer
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].GetHash == hashKey)
                {
                    prefab = buffer[i];
                    return true;
                }
            }
            prefab = default;
            return false;
        }
    }
}