namespace VRTK.Examples
{
	using UnityEngine;

	public class TouchEvent : VRTK_InteractableObject
	{
		public ButtonSelection button;

		public override void StartUsing(VRTK_InteractUse usingObject)
		{
			//button.OnTouch ();
		}

		public override void StopUsing(VRTK_InteractUse usingObject)
		{
		}
	}
}