/*
 * ControllerState.cs
 *
 * Keeps position and button state of Vive controller.
 * (Placed under SteamVR controller object)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerState : MonoBehaviour
{
	// pointer for Vive controller
	public VRTK.VRTK_StraightPointerRenderer pointer;

	// global manager
	public GlobalManager global;

	// component under Canvas object that controls menu UI
	public ButtonSelection menu;

	GameObject sphere;
	public Transform transform;
	public bool isTrigger;
	int pressHold;

	void Awake()
	{
		transform = GetComponent<Transform>();
		isTrigger = false;
		pressHold = 0;
	}

	void Update()
	{
		// position coordinate of controller
		transform = GetComponent<Transform>();

		// button state of controller
		var trackedObject = GetComponent<SteamVR_TrackedObject>();
		var device = SteamVR_Controller.Input((int)trackedObject.index);
		//Debug.Log(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger));

		// trigger menu UI and activate pointer to appear when button pressed
		if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			if (menu.panelImage.GetComponent<CanvasGroup> ().interactable && !pointer.IsValidCollision()) 
			{
				menu.panelImage.GetComponent<CanvasGroup> ().alpha = 0f;
				menu.panelImage.GetComponent<CanvasGroup> ().interactable = false;
				pointer.validCollisionColor.a = 0;
				pointer.invalidCollisionColor.a = 0;
			} 
			else if (!menu.panelImage.GetComponent<CanvasGroup> ().interactable)
			{
				menu.panelImage.GetComponent<CanvasGroup> ().alpha = 1f;
				menu.panelImage.GetComponent<CanvasGroup> ().interactable = true;
				pointer.validCollisionColor.a = 1;
				pointer.invalidCollisionColor.a = 1;
			}
		}

		// undo previous stroke
		if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
		{
			OnUndoPress ();
			pressHold = 1;
		}
		// delete all sketches when press+hold
		if (device.GetPress(SteamVR_Controller.ButtonMask.Grip))
		{
			pressHold++;
			if (pressHold > 100) 
			{
				OnDeleteAll ();
			}
		}
		if (device.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
		{
			pressHold = 0;
		}

		// when in sphere mode, change its size or place it on a scene with touchpad
		if (GameObject.Find ("Sphere")) 
		{
			// change sphere size using touchpad
			if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
			{
				Vector2 touchPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
				float scale = Mathf.Clamp(map(touchPos.x, -1f, 1f, 0.01f, 0.8f), 0.01f, 0.8f);
				sphere.transform.localScale = new Vector3 (scale, scale, scale);
			}
			// place it on a scene
			if (device.GetPressDown (SteamVR_Controller.ButtonMask.Touchpad)) 
			{
				Vector3 tempPos = sphere.transform.position;
				sphere.transform.parent = null;
				sphere.name = "SphereFixed";
				sphere.transform.position = tempPos;
				// create a new one
				OnSphereSelected();
			}
		}

		// test painting mode with controller
		if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			if (global.forceMode != 13) 
			{
				isTrigger = true;
			}
		}
		else if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
		{
			isTrigger = false;
		}
	}

	float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}

	// undo previous stroke
	public void OnUndoPress()
	{
		GameObject[] all = GameObject.FindGameObjectsWithTag ("paint");
		if (all.Length > 0) 
		{
			Destroy (all [all.Length - 1]);
		}
	}

	// delete all strokes
	public void OnDeleteAll()
	{
		GameObject[] all = GameObject.FindGameObjectsWithTag ("paint");
		for (int i = 0; i < all.Length; i++)
		{
			Destroy (all [i]);
		}
	}

	public void OnSphereSelected()
	{
		if (!GameObject.Find ("Sphere")) 
		{
			sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			sphere.tag = "paint";
			sphere.GetComponent<Collider> ().enabled = false;
			sphere.transform.rotation = this.transform.rotation;
			sphere.transform.position = this.transform.position;
			sphere.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
			sphere.transform.parent = this.transform;
			sphere.transform.localPosition = new Vector3 (0, 0, 0.8f);
		}
	}

	public void OnSphereDestroy()
	{
		if (GameObject.Find ("Sphere")) 
		{
			Destroy (GameObject.Find ("Sphere"));
		}
	}
}
