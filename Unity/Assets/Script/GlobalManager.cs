/*
 * GlobalManager.cs
 *
 * stores important global variables and states (e.g., sensor data from MCU)
 * (placed under "GlobalManager" GameObject)
 */

using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class GlobalManager : MonoBehaviour 
{
	// controller sphere
	public MeshRenderer controllerMesh;

	// incoming sensor data
	public SerialCommThread serial;
	public SerialHandler serialHandler;
	public int bendData;
	public int isReleased;
	public bool isSetupDone;

	// force feedback mode
	public int forceMode;

	// for switching camera
	public GameObject capture;
	public GameObject vrcam;

	void Awake() 
	{
		bendData = 0;
		isReleased = 0;
		forceMode = 0; // 0:no force, 1:squeeze, 2:custom, 3:grasp, 4:click, 13:sphere
		controllerMesh.material.color = new Vector4 (0, 0, 0, 1);
		isSetupDone = false;
	}
	
	void Update() 
	{
		// put sensor data to global variable
		bendData = serial.bend;
		isReleased = serial.release;
		//Debug.Log(bendData);

		// change force mode using keyboard for testing
		if (Input.GetKeyDown ("space")) 
		{
			serialHandler.Write ("0");
			forceMode = 0;
			controllerMesh.material.color = new Vector4 (0, 0, 0, 1);
		}

		// disable haptics for test mode
		if (Input.GetKeyDown ("x")) 
		{
			serialHandler.Write ("9");
		}
	}

	// triggered when selected one of the mode buttons on menu UI
	public void OnModeSelected(int index)
	{
		if (index == 1) 
		{
			serialHandler.Write("1");
			forceMode = 1;
			controllerMesh.material.color = new Vector4 (1, 1, 1, 1);
		} 
		else if (index == 2) 
		{
			serialHandler.Write ("2");
			forceMode = 2;
			controllerMesh.material.color = new Vector4 (1, 1, 1, 1);
		}
		else if (index == 3) 
		{
			serialHandler.Write ("3");
			forceMode = 3;
			controllerMesh.material.color = new Vector4 (1, 1, 1, 1);
		}
		else if (index == 4) 
		{
			serialHandler.Write ("4");
			forceMode = 4;
			controllerMesh.material.color = new Vector4 (1, 1, 1, 1);
		}
		else if (index == 13) 
		{
			serialHandler.Write ("0");
			forceMode = 13;
			controllerMesh.material.color = new Vector4 (0, 0, 0, 1);
		}
	}
}