/*
 * PlaneInit.cs
 *
 * initialize plane
 * (placed under "PlaneNoGrid" GameObject)
 */

using UnityEngine;
using System.Collections;

public class PlaneInit : MonoBehaviour 
{
	public PlaneGrid plane;

	void Start () 
	{
		transform.position = new Vector3 (plane.lengthScale/2f, 0f, plane.widthScale/2f);
	}
}
