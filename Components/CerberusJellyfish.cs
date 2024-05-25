using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
    public class CerberusJellyfish : MonoBehaviour
    {
        private OWRigidbody _planetBody;
        private QuantumOrbit _orbit;
        private bool _hasBeenLocked;
        private float _riseTime = 30;
        private float _startTime;
        private float _endTime;

        public void Start()
        {
            _planetBody = this.GetAttachedOWRigidbody().GetOrigParentBody();
            _orbit = _planetBody.GetComponent<QuantumOrbit>();
            GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
        }

        public void OnDestroy()
        {
            GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
        }

        public void FixedUpdate()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(-21, -7, GetProgress()), transform.localPosition.z);
        }

        public float GetProgress()
        {
            return _hasBeenLocked ? Mathf.Clamp01(Mathf.InverseLerp(_startTime, _endTime, Time.time)) : 0;
        }

        public void OnQuantumMoonStateChanged(OWRigidbody qmBody)
        {
            if (qmBody == null || _orbit == null) return;

            if (_orbit._stateIndex == qmBody.GetComponent<QuantumMoon>().GetStateIndex() && !_hasBeenLocked)
            {
                _hasBeenLocked = true;
                _startTime = Time.time;
                _endTime = Time.time + _riseTime;
            }
        }
    }
}
