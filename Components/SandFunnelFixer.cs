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

        public void Awake()
        {
            _sandFunnel = this.GetAttachedOWRigidbody();
            _ashTwin = Locator.GetAstroObject(AstroObject.Name.TowerTwin).GetOWRigidbody();
        }

        public void Start()
        {
            MatchPostitionAndVelocity();
        }

        public void Update()
        {
            MatchPostitionAndVelocity();
        }

        private void MatchPostitionAndVelocity()
        {
            _sandFunnel.SetPosition(_ashTwin.GetPosition());
            _sandFunnel.SetVelocity(_ashTwin.GetVelocity());
        }
    }
}
