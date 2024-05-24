using UnityEngine;

namespace Stowaway.Components
{
    internal class EnjoySignComponent : MonoBehaviour
    {
        private void Awake()
        {
            var text = GetComponentInParent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.fontSize = 65;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
        }

        private void Update()
        {
            var text = GetComponentInParent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = "ENJOY YOUR TRAVELS!";
            }
        }
    }
}