#if UNITY_EDITOR
using UnityEngine;
using Zenject;

namespace SceneContext.Controller
{
    public class DebugController : ITickable
    {
        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Time.timeScale = 0.05f;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                Time.timeScale = 1;
            }
        }
    }
}
#endif