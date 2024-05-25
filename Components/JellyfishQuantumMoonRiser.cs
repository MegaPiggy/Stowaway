using System;
using UnityEngine;

namespace Stowaway.Components
{
    [RequireComponent(typeof(JellyfishController))]
    public class JellyfishQuantumMoonRiser : MonoBehaviour
    {
        public QuantumOrbit _orbit;
        private JellyfishController _controller;
        private bool _isLocked;

        public void Start()
        {
            _controller = GetComponent<JellyfishController>();
            _orbit = _controller._jellyfishBody.GetOrigParentBody().GetComponent<QuantumOrbit>();
            GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
        }

        public void OnDestroy()
        {
            GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
        }

        public void Update()
        {
            if (_isLocked)
            {
                _controller._isRising = true;
            }
        }

        public void OnQuantumMoonStateChanged(OWRigidbody qmBody)
        {
            if (qmBody == null || _orbit == null) return;

            if (_orbit._stateIndex == qmBody.GetComponent<QuantumMoon>().GetStateIndex())
            {
                _isLocked = true;
                _controller._isRising = true;
            }
            else
            {
                _isLocked = false;
                _controller._isRising = false;
            }
        }
    }
}
