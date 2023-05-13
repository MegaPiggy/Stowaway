using UnityEngine;

namespace TheStowaways.Components
{
    internal class PullTornadoComponent : MonoBehaviour
    {        
        void OnEnable()
        {
            var tc = gameObject.GetComponent<TornadoController>();
            if (tc != null)
            {
                tc._wander = false;
            }
        }

        void OnDisable()
        {
            var tc = gameObject.GetComponent<TornadoController>();
            if (tc != null)
                tc._wander = true;
        }
    }
}
