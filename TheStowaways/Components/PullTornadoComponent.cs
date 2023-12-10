using UnityEngine;

namespace TheStowaways.Components
{
    internal class PullTornadoComponent : MonoBehaviour
    {
        private void OnEnable()
        {
            var tc = gameObject.GetComponent<TornadoController>();
            if (tc != null)
            {
                tc._wander = false;
            }
        }

        private void OnDisable()
        {
            var tc = gameObject.GetComponent<TornadoController>();
            if (tc != null)
                tc._wander = true;
        }
    }
}