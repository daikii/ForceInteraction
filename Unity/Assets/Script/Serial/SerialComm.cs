using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class serialComm : MonoBehaviour
{
	public string port;
	public int rate;
	SerialPort stream;
	public int sensorVal;

	void Awake() 
	{
		port = "/dev/cu.usbserial-DJ003J6Y";
		rate = 9600;
		stream = new SerialPort(port, rate);
		stream.Open();
		sensorVal = 0;
	}

	void Update () 
	{
		// receive sensor data
		string tempVal = stream.ReadLine ();
		sensorVal = int.Parse (tempVal);
		print (sensorVal);

		//stream.WriteLine();
	}
}
