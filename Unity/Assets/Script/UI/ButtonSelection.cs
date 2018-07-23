/*
 * ButtonSelection.cs
 *
 * UI event triggers.
 * (Placed under "Canvas" GameObject)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSelection : MonoBehaviour 
{
	// Vive controller
	public ControllerState controller;
	// display on/off of menu
	public Image panelImage;

	void Awake ()
	{
		// hide sub menu
		panelImage.GetComponent<CanvasGroup>().alpha = 0f;
		panelImage.GetComponent<CanvasGroup> ().interactable = false;
	}
	
	void Update () 
	{
	}
}
