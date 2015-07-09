using UnityEngine;
using System.Collections;

public class MeshAnimator : MonoBehaviour {
	
	[SerializeField]
	private Mesh m_MeshOriginal;

	[SerializeField]
	private MeshFilter m_MeshFilter;

	private Vector3[] m_Vertices;
	private Mesh m_Mesh;
	private int z = 1;

	// Use this for initialization
	void Start () {
		m_Mesh = Instantiate( m_MeshOriginal ) as Mesh;
		m_MeshFilter.mesh = m_Mesh;
		m_Vertices = m_Mesh.vertices;
	}
	
	// Update is called once per frame
	void Update () {
		//The new vertices
		Vector3[] vertices = new Vector3[m_Vertices.Length];
		//Random values to move the axis by and create the jingle
		float fScaleX = Random.value / 10;
		float fScaleY = Random.value / 10;
		float fScaleZ = Random.value / 10;
		//loop through each vertex
		for ( int i = 0; i < vertices.Length; i++ ) {
			//Get the next vertex
			Vector3 vertex = m_Vertices[i];
			//Make a change to each of the coordinates of this vertex

			vertex.y += Mathf.Abs( vertex.x * (0.001f * z + vertex.y) );
			
			//Add the new vertex to our new array
			vertices[i] = vertex;
		}
		z++;
		//Set the new array as the mesh vertex array
		m_Mesh.vertices = vertices;
		//We can ask the engine to recalculate the normals  
		//of the mesh, the normals are the surfaces that are rendered.                          meshObjectsMesh.RecalculateNormals();
		m_Mesh.RecalculateNormals();
		//We have to call RecalculateBounds to make sure
		//that the triangles from the vertex relationships
		//are recalculated correctly
		m_Mesh.RecalculateBounds();
	}
}
