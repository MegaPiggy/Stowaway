using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(OverheadDetector))]
	public abstract class NomaiTugger : MonoBehaviour
	{
		protected OverheadDetector _overheadDetector;
		private bool _canOpenAndClose = false;
		public bool canOpenAndClose => _canOpenAndClose;

		public virtual void Start()
		{
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
			_overheadDetector.DefaultDirectMoonClamps();
			_overheadDetector.OnMoonOverhead += OnMoonOverhead;
			_overheadDetector.OnMoonNoLongerOverhead += OnMoonNoLongerOverhead;
		}

		public abstract void OnMoonOverhead(OWRigidbody bodyOverhead);
		public abstract void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead);

		public void SetCanOpenAndClose(bool canOpenAndClose)
		{
			_canOpenAndClose = canOpenAndClose;
		}

	}
}
