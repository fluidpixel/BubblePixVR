using UnityEngine;
using System.Collections;

public class MeshAnimator : MonoBehaviour {
	
	[SerializeField]
	private Mesh m_MeshOriginal;

	[SerializeField]
	private MeshFilter m_MeshFilter;

	private Mesh m_CurvedMesh;
	private Vector3[] m_Vertices;
	private Mesh m_Mesh;
	private int z = 1;

	// Use this for initialization
	void Start () {
		m_Mesh = ProceduralMesh.GeneratePlane( 20, 40 );
		
		 //Instantiate( m_MeshOriginal ) as Mesh;
		m_CurvedMesh = ProceduralMesh.GenerateCurvedCylinder( 19, 39 );
		
		m_MeshFilter.mesh = m_Mesh;
		m_Vertices = m_Mesh.vertices;

		//StartCoroutine(MorphMesh());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private IEnumerator MorphMesh() {
		Vector3[] vertices	= new Vector3[m_Mesh.vertexCount];
		Vector2[] texCoords = new Vector2[m_Mesh.uv.Length];
		Vector3[] normals	= new Vector3[m_Mesh.normals.Length];
		m_Mesh.triangles = m_CurvedMesh.triangles;
		while ( Vector3.Distance( m_Mesh.vertices[0], m_CurvedMesh.vertices[0] ) > 0.01f ) {
			for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
				vertices[i]		= Vector3.Slerp(m_Mesh.vertices[i], m_CurvedMesh.vertices[i], Time.deltaTime);
				texCoords[i]	= Vector2.Lerp(m_Mesh.uv[i], m_CurvedMesh.uv[i], Time.deltaTime);
				normals[i]		= Vector3.Slerp(m_Mesh.normals[i], m_CurvedMesh.normals[i], Time.deltaTime);

				if ( i == m_Mesh.vertexCount * 0.5 ) {
					yield return null;
				}
			}
			m_Mesh.vertices = vertices;
			m_Mesh.uv		= texCoords;
			m_Mesh.normals	= normals;
			m_Mesh.RecalculateBounds();
			yield return null;
		}
		m_Mesh.vertices	= m_CurvedMesh.vertices;
		m_Mesh.uv		= m_CurvedMesh.uv;
		m_Mesh.normals	= m_CurvedMesh.normals;
		m_Mesh.RecalculateBounds();
	}

	private void JiggleMesh() {
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

			vertex.y += Mathf.Abs( vertex.x * ( 0.001f * z + vertex.y ) );

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
