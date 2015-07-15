using UnityEngine;
using System.Collections;

public class MeshAnimator : MonoBehaviour {
	
	[SerializeField]
	private MeshFilter m_Filter;

	[SerializeField]
	private MeshRenderer m_Renderer;

	private Mesh m_Mesh;
	private float curve = 2.0f;
	private float bulge = 0.70f;
	private int width = 29;
	private int height = 14;
	private bool toPlane = false;
	private bool paused = false;

	// Use this for initialization
	void Start () {
		m_Mesh = ProceduralMesh.GeneratePlane( height, width );
		m_Filter.mesh = m_Mesh;
	}
	
	// Update is called once per frame
	void Update () {
		m_Renderer.material.SetFloat("_BorderAlpha", 1.0f - (curve - 1.0f));

		if ( Input.GetKeyDown( KeyCode.Space ) ) {
			StartCoroutine(MoveAndScale(new Vector3(0.0f, 0.0f, -2.0f), new Vector3( 3.0f, 3.0f, 3.0f )));
			StartCoroutine(PlaneToCircle());
		}
	}

	public void ToCylinder( Vector3 _targetPos ) {
		StartCoroutine( PlaneToCircle() );
		StartCoroutine( MoveAndScale( _targetPos, new Vector3( 4.0f, 4.0f, 4.0f ) ) );
	}

	public void ToPlane( ) {
		StartCoroutine( CircleToPlane() );
		StartCoroutine( MoveAndScale( Vector3.zero, new Vector3( 2.0f, 2.0f, 1.0f ) ) );
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

		Mesh target, end;
		Vector3[] vertices = new Vector3[width * height];
		curve = 1.0f;
		bulge = 0.75f;
		end = ProceduralMesh.GeneratePlane( height, width );
		target = ProceduralMesh.GenerateCurvedCylinderSegment(height, width, curve, bulge);
		float time;

		while ( Vector3.Distance( m_Mesh.vertices[0], end.vertices[0] ) > 0.01f ) {
			if ( !paused ) {
				
				if (curve != 2.0f )
					target = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, curve, bulge );
				else if ( bulge != 1.0f )
					target = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, curve, bulge );
				else {
					target = ProceduralMesh.GeneratePlane( height, width );
				}
				time = ( Time.deltaTime / Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) );

				if ( curve < 2.0f )
					curve += 0.04f;
				else
					curve = 2.0f;

				if ( bulge < 1.0f )
					bulge += 0.015f;
				else
					bulge = 1.0f;

				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], time );
				}
				m_Mesh.vertices = vertices;

				yield return null;
			}
			else {
				yield return null;
			}
		}
		m_Mesh.vertices = end.vertices;
		m_Mesh.triangles = end.triangles;
		m_Mesh.uv = end.uv;
		m_Mesh.normals = end.normals;
		m_Mesh.RecalculateBounds();
	}

	private IEnumerator PlaneToCircle() {
		
		Mesh target, end;
		Vector3[] vertices = new Vector3[width * height];
		curve = 2.0f;
		bulge = 1.0f;
		target = end = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, 1.0f, 0.75f );
		m_Mesh.bounds = end.bounds;
		m_Mesh.triangles = target.triangles;
		float time;
	
		while ( Vector3.Distance( m_Mesh.vertices[0], end.vertices[0] ) > 0.01f ) {
			time = (Time.deltaTime / Vector3.Distance(m_Mesh.vertices[0], end.vertices[0]));
			
			if ( !paused ) {

				target = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, curve, bulge );

				if ( curve > 1.0f )
					curve -= 0.04f;
				else 
					curve = 1.0f;

				if ( bulge > 0.75f)
					bulge -= 0.015f;
				else
					bulge = 0.75f;

				yield return null;

				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], time);
				}
				m_Mesh.vertices = vertices;
			
			}
			yield return null;
		}
		m_Mesh.vertices = end.vertices;
		m_Mesh.triangles = end.triangles;
		m_Mesh.uv = end.uv;
		m_Mesh.normals = end.normals;
		//m_Mesh.RecalculateBounds();
	}

	private IEnumerator MoveAndScale( Vector3 _targetPos, Vector3 _targetScale ) {
		float distance = 1.0f;
		float scale = 1.0f;
		while ( distance > 0.01f || scale > 0.01f ) {
			distance = Vector3.Distance( gameObject.transform.localPosition, _targetPos );
			scale = Vector3.Distance( gameObject.transform.localScale, _targetScale );

			gameObject.transform.localPosition = Vector3.Lerp( gameObject.transform.localPosition, _targetPos, Time.deltaTime / distance );
			gameObject.transform.localScale = Vector3.Lerp( gameObject.transform.localScale, _targetScale, Time.deltaTime / distance );

			yield return null;
		}
		gameObject.transform.localPosition = _targetPos;
		gameObject.transform.localScale = _targetScale;
	}

}