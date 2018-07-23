/*
 * SerialCommThread.cs
 *
 * Get data from SerialHandler and parse it.
 * (Placed under "GlobalManager" GameObject)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SerialCommThread : MonoBehaviour
{
	public SerialHandler serialHandler;

	public int bend;
	public int release;

	void Awake()
	{
		// set event handler for data retrieval
		serialHandler.OnDataReceived += OnDataReceived;
		bend = 0;
		release = 0;
	}

	void Update()
	{
	}

	void OnDataReceived(string message)
	{
		// split concatenated data
		var data = message.Split(new string[]{"\t"}, System.StringSplitOptions.None);

		// if not enough data is sent, exit method
		if (data.Length < 2) return;

		//Debug.Log(data.Length);
		//Debug.Log(data[0]);

		try
		{
			bend = int.Parse(data[0]);
			release = int.Parse(data[1]);
		} 
		catch (System.Exception e) 
		{
			Debug.LogWarning(e.Message);
		}
	}
}