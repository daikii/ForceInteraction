using UnityEngine;
using System.Collections;

public class CameraPos : MonoBehaviour 
{	
	private int posNum;
	private int index;
	
	private bool isMove;
	public float height;
	public float depth;
	public Vector3 center;
	
	void Start() 
	{
		height = 1.7f;
		depth = -3.2f;

		isMove = false;
		center = new Vector3 (-0.4f, 1.2f, -0.73f);
		
		transform.position = new Vector3 (0, height, depth);
		transform.LookAt (center);
	}
	
	void Update() 
	{
		//////// start auto-pan with key-press ////////
			
		if (Input.GetKeyDown("l"))
		{
			if (!isMove) 
			{
				isMove = true;
			} 
			else 
			{
				isMove = false;
				//transform.position = new Vector3 (0, height, depth);
				//transform.LookAt (center);
			}
		}
		if (Input.GetKeyDown("p"))
		{
			transform.position = new Vector3 (0, height, depth);
			transform.LookAt (center);
		}
			
		//////// update transformation ////////

		if (isMove)
		{
			transform.RotateAround (center, Vector3.up, 50 * Time.deltaTime);
			transform.LookAt (center);
		}
		else
		{
			//////// modify camera angle ////////

		}
	}
}
