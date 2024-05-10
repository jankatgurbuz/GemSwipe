using UnityEngine;

namespace SceneContext.View
{
    [CreateAssetMenu(fileName = "MovementSettings", menuName = "MovementSettings")]
    public class SOMovementSettings : ScriptableObject
    {
        public AnimationCurve AnimationCurve;
        public float MovementSpeed = 0.25f;
    }
}
