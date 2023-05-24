using UnityEngine;

namespace TheStowaways.Components
{
    internal class EnjoySignComponent : MonoBehaviour
    {
        void Awake()
        {
            var text = GetComponentInParent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.fontSize = 65;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

        }

        void Update()
        {
            var text = GetComponentInParent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = "ENJOY YOUR TRAVELS!";
            }
        }
    }
}
