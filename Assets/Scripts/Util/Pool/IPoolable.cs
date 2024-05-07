using UnityEngine;

namespace Util.Pool
{
    public interface IPoolable
    {
        void Awake();
        void Create();
        void Active();
        void Inactive();
        GameObject GetGameObject();
        Transform GetTransform();
    }
}