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
        private float _originalUpperLimit;
        private float _originalUpwardsAcceleration;

        public void Start()
        {
            _controller = GetComponent<JellyfishController>();
            _originalUpperLimit = _controller._upperLimit;
            _originalUpwardsAcceleration = _controller._upwardsAcceleration;
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
                _controller._upperLimit = 400;
                _controller._upwardsAcceleration = 25;
            }
            else
            {
                _isLocked = false;
                _controller._isRising = false;
                _controller._upperLimit = _originalUpperLimit;
                _controller._upwardsAcceleration = _originalUpwardsAcceleration;
            }
        }
    }
}
