using UnityEngine;
using System.Collections;

public class MeshAnimator : MonoBehaviour {
	

	[SerializeField]
	private MeshFilter m_MeshFilter;

	private Vector3[] m_Vertices;
	private Mesh m_Mesh;
	private float z = 1;
	private int width, height;
	private bool toPlane = false;
	private bool paused = false;

	// Use this for initialization
	void Start () {
		width = 39;
		height = 19;

		m_Mesh = ProceduralMesh.GeneratePlane( height, width );

		m_MeshFilter.mesh = m_Mesh;
		m_Vertices = m_Mesh.vertices;
		StartCoroutine(PlaneToCircle());
	}
	
	// Update is called once per frame
	void Update () {
		if ( Input.GetKey( KeyCode.RightArrow ) ) {
			this.gameObject.transform.Rotate( new Vector3( 0.0f, -0.3f, 0.0f ) );
		}
		else if ( Input.GetKey( KeyCode.LeftArrow ) ) {
			this.gameObject.transform.Rotate( new Vector3( 0.0f, 0.3f, 0.0f ) );
		}

		if ( Input.GetKeyDown( KeyCode.UpArrow ) ) {
			z += 0.1f;
			m_MeshFilter.mesh = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, 360.0f, z );
			
		}
		else if ( Input.GetKeyDown( KeyCode.DownArrow ) ) {
			z -= 0.1f;
			m_MeshFilter.mesh = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, 360.0f, z );
			
		}

		if ( Input.GetKeyDown( KeyCode.Space ) ) {
			
			m_MeshFilter.mesh = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, 360.0f, z );
			
			StopAllCoroutines();
			if ( z == 1.0f ) {
				StartCoroutine(CircleToPlane());
			}
			else if (z == 2.0f) {
				StartCoroutine(PlaneToCircle());
			}
		}
		if ( Input.GetKeyDown( KeyCode.Alpha1 ) ) {
			paused = !paused;
		}
		
	}

	private IEnumerator LerpMesh() {
		Mesh target;

		if ( toPlane ) {
			target = ProceduralMesh.GeneratePlane( height, width );
			Debug.Log("To Plane");
		}
		else {
			target = ProceduralMesh.GenerateCurvedCylinder( height, width );
			Debug.Log( "To Curve" );
		}
		
		Vector3[] vertices	= new Vector3[width * height];
		Vector2[] texCoords = new Vector2[width * height];
		Vector3[] normals	= new Vector3[width * height];

		m_Mesh.triangles = target.triangles;

		//Debug.Log( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ).ToString() );
		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) > 0.01f ) {
			if ( !paused ) {
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i]		= Vector3.Slerp(m_Mesh.vertices[i], target.vertices[i], 0.5f * Time.deltaTime);
					texCoords[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], 0.5f * Time.deltaTime );
					normals[i] = Vector3.Slerp( m_Mesh.normals[i], target.normals[i], 0.5f * Time.deltaTime );

					if ( i == m_Mesh.vertexCount * 0.5 ) {
						yield return null;
					}
				}
				m_Mesh.vertices = vertices;
				m_Mesh.uv		= texCoords;
				m_Mesh.normals	= normals;
				m_Mesh.RecalculateBounds();
			}
			yield return null;
			//Debug.Log(Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ).ToString());
		}
		m_Mesh.vertices	= target.vertices;
		m_Mesh.uv		= target.uv;
		m_Mesh.normals	= target.normals;
		m_Mesh.RecalculateBounds();
		Debug.Log("Complete");
	}

	private IEnumerator CircleToPlane() {
		Mesh target;
		Vector3[] vertices = new Vector3[width * height];
		Vector2[] texCoords = new Vector2[width * height];
		Vector3[] normals = new Vector3[width * height];

		target = ProceduralMesh.GeneratePlane(height, width);

		//Stage one is to flatten it enough to resemble a plane
		while ( z < 2.0f ) {
			m_Mesh = ProceduralMesh.GenerateCurvedCylinderSegment(height, width, 360.0f, z);
			m_MeshFilter.mesh = m_Mesh;
			z += 0.01f;
			yield return null;
		}
		z = 2.0f;
		m_Mesh.triangles = target.triangles;

		//Then approximate the rest with a lerp
		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) > 0.01f ) {
			if ( !paused ) {
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], Time.deltaTime );
					texCoords[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], Time.deltaTime );
					normals[i] = Vector3.Lerp( m_Mesh.normals[i], target.normals[i], Time.deltaTime );
		
					if ( i == m_Mesh.vertexCount * 0.5 ) {
						yield return null;
					}
				}
				m_Mesh.vertices = vertices;
				m_Mesh.uv = texCoords;
				m_Mesh.normals = normals;
				m_Mesh.RecalculateBounds();
			}
			yield return null;
			//Debug.Log(Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ).ToString());
		}
		m_Mesh.vertices = target.vertices;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
		m_Mesh.RecalculateBounds();
		Debug.Log( "Complete" );
	}

	private IEnumerator PlaneToCircle() {
		Mesh target;
		Vector3[] vertices = new Vector3[width * height];
		Vector2[] texCoords = new Vector2[width * height];
		Vector3[] normals = new Vector3[width * height];

		target = ProceduralMesh.GenerateCurvedCylinderSegment(height, width, 360.0f, 2);
		Debug.Log(Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ).ToString());
		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) > 0.01f ) {
			if ( !paused ) {
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], (Time.deltaTime / Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) ) * 0.5f );
					//texCoords[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], Time.deltaTime );
					//normals[i] = Vector3.Lerp( m_Mesh.normals[i], target.normals[i], Time.deltaTime );

					if ( i == m_Mesh.vertexCount * 0.5 ) {
						yield return null;
					}
				}
				m_Mesh.vertices = vertices;
				//m_Mesh.uv = texCoords;
				//m_Mesh.normals = normals;
				//m_Mesh.RecalculateBounds();
			}
			yield return null;
		}

		m_Mesh.vertices = target.vertices;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
		//m_Mesh.RecalculateBounds();
		z = 2.0f;

		while ( z > 1.0f ) {
			m_Mesh = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, 360.0f, z );
			m_MeshFilter.mesh = m_Mesh;
			z -= 0.01f;
			yield return null;
		}
		z = 1.0f;
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
