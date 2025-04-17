using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
    public class SandFunnelFixer : MonoBehaviour
    {
        private OWRigidbody _sandFunnel;
        private OWRigidbody _ashTwin;
        private OWRigidbody _emberTwin;
        private Transform _scaleRoot;

        public void Awake()
        {
            _sandFunnel = this.GetAttachedOWRigidbody();
            _ashTwin = Locator.GetAstroObject(AstroObject.Name.TowerTwin).GetOWRigidbody();
            _emberTwin = Locator.GetAstroObject(AstroObject.Name.CaveTwin).GetOWRigidbody();
            _scaleRoot = transform.Find("ScaleRoot");

            if (TryGetComponent(out AlignWithTargetBody alignWithTargetBody))
            {
                Component.DestroyImmediate(alignWithTargetBody);
                FixedUpdateManager._alignWithDirections.RemoveDestroyedElements();
            }

            if (TryGetComponent(out InitialMotion initialMotion))
                Component.DestroyImmediate(initialMotion);
        }

        public void Start()
        {
            MatchPostitionAndVelocity();
            Rescale();
        }

        public void LateUpdate()
        {
            Rescale();
        }

        public void FixedUpdate()
        {
            MatchPostitionAndVelocity();
            PointToEmberTwin();
        }

        private void MatchPostitionAndVelocity()
        {
            _sandFunnel.SetPosition(_ashTwin.GetPosition());
            _sandFunnel.SetVelocity(_ashTwin.GetVelocity());
        }

        private void Rescale()
        {
            if (_scaleRoot.localScale.z == 2) return;
            Vector3 localPosition = _scaleRoot.localScale;
            localPosition.z = 2;
            _scaleRoot.localScale = localPosition;
        }

        private void PointToEmberTwin()
        {
            transform.LookAt(_emberTwin.transform);
        }
    }
}
