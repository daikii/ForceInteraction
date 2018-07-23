/*
 * SketchMeshAll.cs
 *
 * Real-time drawing that creates mesh models including ribbon, 3d, and sphere particles.
 * (Placed under "SketchGenerator" GameObject)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SketchMeshAll : MonoBehaviour
{
	// global manager
	public GlobalManager global;

	// Vive controller
	public ControllerState controller;
	// Vive tracker
	public Transform tracker;

	// stroke mesh
	private Mesh mesh;
	private List<Vector3> vert;
	private List<int> tri;
	private List<Vector2> uv;
	private List<Vector3> norm;
	
	public Camera cam;
	private GameObject line;
	private string prefab;

	private Vector3 prevPt;
	public List<Vector3> points;
	public List<Quaternion> offsetsQuat;
	public List<Vector3> offsetsVec;

	private float lineWidth;
	private float lineWidthPrev;
	private float lineWidthPrevPrev;
	private List<float> lineWidthList;

	public Vector4 col;
	private Texture lineTexture;

	// for 3d mesh
	private int nbSides;
	private int nbVerticesSides;
	
	// speed <-> stroke thickness
	private float prevDist;
	private float prevprevDist;
	private float prevTime;
	private float prevprevTime;
	
	public bool isSketch;
	public bool isInteractive;

	// threshhold for adding new stroke point
	public float thresh;

	// maximum number of objects in scene
	public int maxNumObjects;

	// color array
	Vector4[] colorArray = {new Vector4(0, 191, 255, 255), 
							new Vector4(255, 245, 50, 255),
							new Vector4(255, 255, 255, 255),
							new Vector4(69, 69, 69, 255),
							new Vector4(175, 255, 71, 255),
							new Vector4(255, 0, 132, 255)};

	// for metaball generation
	private StaticMetaballSeed seed;
	
	void Awake()
	{
		vert = new List<Vector3>();
		tri = new List<int>();
		uv = new List<Vector2>();
		norm = new List<Vector3>();

		prefab = "SketchMeshThreePrefab";
		isSketch = false;
		isInteractive = true;
		points = new List<Vector3>();

		// default line color
		col = ColorConvert(colorArray[0]);
		
		// default line width
		lineWidth = 0.005f;
		lineWidthPrev = 0.01f;
		lineWidthPrevPrev = 0.01f;
		lineWidthList = new List<float>();

		// 3d polygon parameters
		nbSides = 24;
		nbVerticesSides = nbSides * 2 + 2;
		
		// default line texture
		lineTexture = null;
		
		prevDist = 0f;
		prevprevDist = 0f;
		prevTime = 0f;
		prevprevTime = 0f;

		thresh = 0.004f; // 0.02 / 0.008 mocap / 0.004f VR

		maxNumObjects = 250;
	}

	Vector4 ColorConvert(Vector4 rgb)
	{
		rgb.x /= 255;
		rgb.y /= 255;
		rgb.z /= 255;
		rgb.w /= 255;

		return new Vector4 (rgb.x, rgb.y, rgb.z, rgb.w);
	}

	void Update()
	{
		Vector3 newPt = new Vector3 (0, 0, 0);
		float dist;

		// check for stroke initiation trigger
		// add this when testing with controller: controller.isTrigger
		if ((global.isReleased == 0 && global.bendData > 0) || Input.GetMouseButton(0))
		{
			// create new mesh GameObject that is synced with PUN
			if (!isSketch)
			{
				Refresh();
				isSketch = true;
				prevPt = GetPosition();
				prevTime = Time.deltaTime;
				return;
			}

			// new point and distance
			newPt = GetPosition();
			dist = Vector3.Distance (prevPt, newPt);

			// sketch speed calculation
			float velocity = Mathf.Abs(dist + prevDist + prevprevDist) / (Time.deltaTime + prevTime + prevprevTime);

			// store previous data
			prevPt = newPt;
			prevprevDist = prevDist;
			prevDist = dist;
			prevprevTime = prevTime;
			prevTime = Time.deltaTime;

			// do not add new point if not moved as much
			if (dist >= thresh)
			{
				// change line width depending on stroke speed
				if (isInteractive)
				{
					InteractiveLineWidth(velocity);
				}

				// update sketch point
				points.Add(newPt);

				// update line width
				lineWidthList.Add(lineWidth);

				// update sketch
				if (points.Count > 1)
				{
					if (prefab == "SketchMeshThreePrefab")
					{
						// vector perpendicular to vector of plotted points and first-person forward vector
						offsetsQuat.Add(Quaternion.FromToRotation(Vector3.forward, points[points.Count - 2] - points [points.Count - 1]));
						UpdateMeshThree();
						RecalculateMesh();
					}
					else if (global.forceMode == 3)
					{
						if (line.transform.Find ("SourceRoot").childCount <= 200) {
							MakeMetaballs (dist);
						}
					}
				}
			}
		}
				
		// released. refresh point list. increment line list. avoid reaching vertex limit.
		// add this when testing with Vive controller: !controller.isTrigger
		if (isSketch && (mesh.vertexCount > 50000 || global.isReleased == 1 || (global.bendData <= 0 && !controller.isTrigger && !Input.GetMouseButton(0)))) 
		{
			// smooth mesh with optimal tangent
			mesh.RecalculateNormals(60);
			isSketch = false;

			if (global.forceMode == 4) {
				ReleaseEffect ();
			}
		}
	}

	Vector3 GetPosition()
	{
		Vector3 temp;
		float distZ = 1.5f; // 1.5

		// check wearable sensor trigger
		if (global.bendData > 0) 
		{
			return tracker.position;
		} 
		// check Vive controller trigger
		else if (controller.isTrigger) 
		{
			return controller.transform.position;
		}
		// for mouse testing
		else
		{
			temp = Input.mousePosition;
			temp.z = distZ;
			return cam.ScreenToWorldPoint(temp);
		}
	}

	void RecalculateMesh()
	{
		mesh.vertices = vert.ToArray();
		mesh.triangles = tri.ToArray();
		mesh.uv = uv.ToArray();
		mesh.normals = norm.ToArray();
		mesh.RecalculateNormals(); // built-in and optimize once finished drawing
	}

	void InteractiveLineWidth(float vel)
	{
		float maxWidth = 0.15f; // 0.01 / 0.36 mocap
		float maxWidthMin = 0.00007f; // 0.02 (min width you can adjust with button press) / 0.00007f VR
		float minWidth = 0.00005f;

		// max thickness considering pressure data
		// ALWAYS send sensor value in the range of 0-30
		float maxWidthPress = Mathf.Clamp(map(global.bendData, 0, 30, maxWidthMin, maxWidth), maxWidthMin, maxWidth);
		//float maxWidthPress = 0.1f; // for mouse control

		// change stroke width depending on stroke speed (thick -> thin)
		var sensitivity = 3f; // 15f / 3f VR
		float lineWidthTemp = Mathf.Clamp(((minWidth - maxWidthPress) / sensitivity * vel + maxWidthPress), minWidth, maxWidthPress);

		// smooth
		//float diff = Mathf.Abs (lineWidthPrev - lineWidthTemp);
		//float scaler = map (diff, 0f, 0.35f, 1, 5);
		lineWidth = (lineWidthTemp + lineWidthPrev + lineWidthPrevPrev) / 3f;
		// smooth further
		if (Mathf.Abs (lineWidth - lineWidthPrev) < 0.001) lineWidth = lineWidthPrev;

		lineWidthPrevPrev = lineWidthPrev;
		lineWidthPrev = lineWidth;

		// (deltaWidth / (deltaDist) * dist + maxWidth
		//lineWidth = (0.01f - 0.1f) / (0.2f - 0.02f) * dist + 0.1f;
	}

	void MakeMetaballs(float d)
	{
		/*
		Vector3 tempPt = points [points.Count - 1];
		Vector3 tempDiff = points [points.Count - 1] - points [points.Count - 2];
		int numPt = d / 0.1f;
		Debug.Log (numPt);
		Vector3[] midPts = new Vector3[numPt];
		midPts [numPt-1] = tempPt;

		for (int i = 0; i < numPt; i++) 
		{
			Vector3 midPt = prevPt + ((1f / 1f) * (i + 1) * tempDiff);
			// update sketch point
			points.Add (midPt);
			// update line width
			lineWidthList.Add (Random.Range(0.1f, 0.1f * 2f));
			// update mesh
			offsetsQuat.Add (Quaternion.FromToRotation (Vector3.forward, points [points.Count - 2] - points [points.Count - 1]));
			UpdateMeshThree ();
			RecalculateMesh ();
		}*/


		int ballNum = 10;
		float dist = 0.1f;
		Vector3[] randomDist = new Vector3[ballNum];
		Vector3 temp;
		Vector3 pt = points [points.Count - 1];

		// create metaballs
		for (int i = 0; i < ballNum; i++) 
		{
			randomDist [i] = new Vector3 (Random.Range (-dist, dist), Random.Range (-dist/2, dist/2), Random.Range (0, dist));
			temp = new Vector3 (pt.x + randomDist [i].x, pt.y + randomDist[i].y, pt.z + randomDist[i].z);
			GameObject child = new GameObject ("MetaballNode");
			child.transform.parent = line.transform.Find ("SourceRoot").transform;
			child.transform.position = temp;
			child.transform.localScale = Vector3.one;
			child.transform.localRotation = Quaternion.identity;
			MetaballNode newNode = child.AddComponent<MetaballNode> ();
			newNode.baseRadius = Random.Range (0.4f, 0.6f);
		}

		// rebuild mesh
		seed.CreateMesh();
	}

	void ReleaseEffect()
	{
		int ballNum = 60;
		float dist = 0.15f;
		Vector3[] randomDist = new Vector3[ballNum];
		Vector3 temp;
		Vector3 pt = points [points.Count - 1];

		// create metaballs
		for (int i = 0; i < ballNum; i++) 
		{
			randomDist [i] = new Vector3 (Random.Range (-dist, dist), Random.Range (-dist, dist), Random.Range (-dist, dist));
			temp = new Vector3 (pt.x + randomDist [i].x, pt.y + randomDist[i].y, pt.z + randomDist[i].z);
			GameObject child = new GameObject ("MetaballNode");
			child.transform.parent = line.transform.Find ("SourceRoot").transform;
			child.transform.position = temp;
			child.transform.localScale = Vector3.one;
			child.transform.localRotation = Quaternion.identity;
			MetaballNode newNode = child.AddComponent<MetaballNode> ();
			newNode.baseRadius = Random.Range (0.2f, 0.35f);
		}

		// rebuild mesh
		seed.CreateMesh();
	}

	void Refresh()
	{
		// refresh for a new mesh object
		vert.Clear();
		tri.Clear();
		uv.Clear();
		norm.Clear();
		
		// new sketch mesh object
		line = (GameObject)Instantiate(Resources.Load(prefab), Vector3.zero, Quaternion.identity); 

		if (prefab == "SketchMeshThreePrefab") 
		{
			// assign mesh
			mesh = new Mesh ();
			line.GetComponent<MeshFilter> ().mesh = mesh;
			// assign material, texture, color
			line.GetComponent<MeshRenderer> ().material.color = col;
			line.GetComponent<MeshRenderer> ().material.mainTexture = lineTexture;
		} 
		else if (prefab == "Metaball") 
		{
			// assign mesh
			mesh = new Mesh ();
			line.transform.Find("StaticMesh").GetComponent<MeshFilter> ().mesh = mesh;
			// assign material, texture, color
			line.transform.Find("StaticMesh").GetComponent<MeshRenderer> ().material.color = col;
			line.transform.Find("StaticMesh").GetComponent<MeshRenderer> ().material.mainTexture = lineTexture;
			// metaball
			seed = line.GetComponent<StaticMetaballSeed> ();
			// scale in overall to fit to VR scene
			line.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		}

		points = new List<Vector3>();
		lineWidthList = new List<float>();

		offsetsQuat = new List<Quaternion>();
		offsetsQuat.Add(Quaternion.identity);

		offsetsVec = new List<Vector3>();
		offsetsVec.Add(new Vector3(0, 0, 0));
	}

	void UpdateMeshThree()
	{	
		int index = vert.Count;
		
		#region Vertices+Normales+UVs
		// side
		float _2pi = Mathf.PI * 2f;
		int vertCounter = 0;
		int sideCounter = 0;
		while (vertCounter < nbVerticesSides)
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
			float r1 = (float)(sideCounter++) / nbSides * _2pi;
			float cos = Mathf.Cos (r1);
			float sin = Mathf.Sin (r1);
			
			Vector3 temp = points[points.Count-1];
			Vector3 tempPrev = points[points.Count-2];
			vert.Add(temp + offsetsQuat[offsetsQuat.Count - 1] * new Vector3(cos, sin, 0) * lineWidthList[lineWidthList.Count - 1]);
			vert.Add(tempPrev + offsetsQuat[offsetsQuat.Count - 2] * new Vector3(cos, sin, 0) * lineWidthList[lineWidthList.Count - 2]);
			
			norm.Add(new Vector3 (Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
			norm.Add(norm[index + vertCounter]);
			
			float t = (float)sideCounter / nbSides;
			uv.Add(new Vector2 (t, 0f));
			uv.Add(new Vector2 (t, 1f));
			
			vertCounter += 2;
		}
		#endregion
		
		#region Triangles
		int nbFace = nbSides;
		int nbTriangles = nbFace * 2;
		
		// side
		sideCounter = 0;
		while (sideCounter < nbSides)
		{
			int current = sideCounter * 2;
			int next = sideCounter * 2 + 2;
			
			/*
			tri.Add(index + next);
			tri.Add(index + current);
			tri.Add(index + next + 1);
			tri.Add(index + current + 1);
			tri.Add(index + next + 1);
			tri.Add(index + current);
			*/
			
			tri.Add(index + current);
			tri.Add(index + next);
			tri.Add(index + next + 1);
			tri.Add(index + current);
			tri.Add(index + next + 1);
			tri.Add(index + current + 1);
			
			sideCounter++;
		}
		#endregion
	}

	float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}

	// event trigger when color button is selected on menu UI
	public void OnColor(int index)
	{
		if (index == 1) {
			col = ColorConvert (colorArray[0]);
		} else if (index == 2) {
			col = ColorConvert (colorArray[1]);
		} else if (index == 3) {
			col = ColorConvert (colorArray[2]);
		} else if (index == 4) {
			col = ColorConvert (colorArray[3]);
		} else if (index == 5) {
			col = ColorConvert (colorArray[4]);
		} else if (index == 6) {
			col = ColorConvert (colorArray[5]);
		}
		//Debug.Log (index);
	}
	
	// event listerner for changing stroke type
	public void OnStrokeChange(string stroke)
	{
		if (stroke == "SketchMeshThreePrefab") 
		{
			prefab = "SketchMeshThreePrefab";
		} 
		else if (stroke == "Metaball")
		{
			prefab = "Metaball";
		}
	}
}
