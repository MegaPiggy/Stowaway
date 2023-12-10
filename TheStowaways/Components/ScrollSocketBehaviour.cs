using NewHorizons.Utility;
using UnityEngine;

namespace TheStowaways.Components
{
    internal class ScrollSocketBehaviour : MonoBehaviour
    {
        public const string ScrollName = "OPC Schematic Scroll w RearSecret";
        public const string ScrollPath = "ConstructionYardIsland_Body/OPC Schematic Scroll w RearSecret/NomaiWallText/Arc 1 - Child of -1/OPC Schematic REAR_TEXT Wall";

        private ScrollSocket _parent;
        private OWItem _scrollObject = null;

        public void Awake()
        {
            _parent = GetComponentInParent<ScrollSocket>();
            _parent.OnSocketablePlaced += onSocketablePlaced;
            _parent.OnSocketableRemoved += onSocketableRemoved;
        }

        private void onSocketablePlaced(OWItem item)
        {
            if (item.name == ScrollName)
            {
                _scrollObject = item;
                var child = item.gameObject.FindChild("NomaiWallText/Arc 1 - Child of -1/OPC Schematic REAR_TEXT Wall");
                if (!child)
                    return;
                var text = child.GetComponent<NomaiWallText>();
                if (text)
                {
                    text.Show();
                }
            }
        }

        private void onSocketableRemoved(OWItem item)
        {
            if (_scrollObject != null)
            {
                var child = item.gameObject.FindChild("NomaiWallText/Arc 1 - Child of -1/OPC Schematic REAR_TEXT Wall");
                if (!child)
                    return;
                var text = child.GetComponent<NomaiWallText>();
                if (text)
                {
                    text.Hide(0.9f);
                }
                _scrollObject = null;
            }
        }
    }
}