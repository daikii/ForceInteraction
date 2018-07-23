/*
 * PlaneGrid.cs
 *
 * dynamically creates meshed plane
 * (placed under "PlaneGrid" GameObject)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshCollider))]
[RequireComponent (typeof(MeshRenderer))]

public class PlaneGrid: MonoBehaviour
{
	// plane size
	public int lengthAc;
	public int widthAc;
	public int lengthScale;
	public int widthScale;
	public int offset;
	public int scale;
	public bool isStart = false;
	
	protected Mesh mesh;
	protected MeshCollider meshCollider;
	
	protected List<Vector3> vert;
	protected List<int> tri;
	protected List<Vector2> uv;
	protected List<Vector3> norm;
	
	void Awake()
	{
		// actual dimension in centimeters
		lengthAc = 400; // 440
		widthAc = 400; // 420
		offset = 40; // 20
		scale = 10; 
		
		// 1 block = 30cm
		lengthScale = lengthAc / scale + offset;
		widthScale = widthAc / scale + offset;
	}
	
	void Start()
	{
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		meshCollider = GetComponent<MeshCollider>();
		
		vert = new List<Vector3>();
		tri = new List<int>();
		uv = new List<Vector2>();
		norm = new List<Vector3>();
		
		// generate meshed grid plane
		GeneratePlane();
		
		isStart = true;
	}
	
	void DrawCell(Vector3 coord)
	{
		Vector3 offsetX = Vector3.right;
		Vector3 offsetZ = Vector3.back;
		
		int index = vert.Count;
		
		// vertices
		vert.Add(coord);
		vert.Add(coord + offsetX);
		vert.Add(coord + offsetZ);
		vert.Add(coord + offsetX + offsetZ);
		
		// triangles/polygons
		tri.Add(index + 0);
		tri.Add(index + 1);
		tri.Add(index + 2);
		tri.Add(index + 3);
		tri.Add(index + 2);
		tri.Add(index + 1);
		
		// uv
		uv.Add(new Vector2(0, 0));
		uv.Add(new Vector2(0, 1));
		uv.Add(new Vector2(1, 0));
		uv.Add(new Vector2(0, 0));
		
		// normals
		norm.Add(Vector3.forward);
		norm.Add(Vector3.forward);
		norm.Add(Vector3.forward);
		norm.Add(Vector3.forward);
	}

	void GeneratePlane()
	{
		vert.Clear();
		tri.Clear();
		uv.Clear();
		norm.Clear();
		
		// clear out triangles just in case
		mesh.triangles = tri.ToArray();
		
		for (int x = 0; x < lengthScale; x++)
		{
			for (int z = 0; z < widthScale; z++)
			{
				Vector3 coord = new Vector3(x, 0, z);
				DrawCell(coord);
			}
		}
		
		// update mesh
		mesh.vertices = vert.ToArray();
		mesh.triangles = tri.ToArray();
		mesh.uv = uv.ToArray();
		mesh.normals = norm.ToArray();
		
		mesh.RecalculateNormals();
		
		// update collision
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}
	
	void Update()
	{
	}
}