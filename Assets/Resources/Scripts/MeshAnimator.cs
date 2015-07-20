using UnityEngine;
using System.Collections;

public class MeshAnimator : MonoBehaviour {

	[SerializeField]
	private MeshFilter m_Filter;

	[SerializeField]
	private MeshRenderer m_Renderer;

	private Mesh m_Mesh;
	private float m_BorderAlpha = 0.0f;
	private int width = 29;
	private int height = 14;
	private bool toPlane = false;
	private bool paused = false;

	// Use this for initialization
	void Start() {
		m_Mesh = ProceduralMesh.GeneratePlane( height, width );
		//m_Mesh = ProceduralMesh.GenerateCurvedCylinderSegment(height, width, 1, 0.75f);
		m_Filter.mesh = m_Mesh;
	}

	// Update is called once per frame
	void Update() {
		m_Renderer.material.SetFloat( "_BorderAlpha", m_BorderAlpha );
		//paused = !Input.GetKey( KeyCode.P );

		//if (Input.GetKeyDown(KeyCode.RightArrow))
		//	StartCoroutine( PlaneToCircle() );

		//if (Input.GetKeyDown(KeyCode.LeftArrow))
		//	StartCoroutine( CircleToPlane() );
	}

	public void ToCylinder( Vector3 _targetPos ) {
		StartCoroutine( MoveAndScale( _targetPos, new Vector3( 4.0f, 4.0f, 4.0f ), new Vector3( 0.0f, 0.0f, 0.0f ) ) );
		StartCoroutine( PlaneToCircle() );
	}

	public void ToPlane() {
		StartCoroutine( FixRotation() );
	}

	private IEnumerator LerpMesh() {
		Mesh target;

		if ( toPlane ) {
			target = ProceduralMesh.GeneratePlane( height, width );
			Debug.Log( "To Plane" );
		}
		else {
			target = ProceduralMesh.GenerateCurvedCylinder( height, width );
			Debug.Log( "To Curve" );
		}

		Vector3[] vertices = new Vector3[width * height];
		Vector2[] texCoords = new Vector2[width * height];
		Vector3[] normals = new Vector3[width * height];

		m_Mesh.triangles = target.triangles;

		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) > 0.01f ) {
			if ( !paused ) {
				
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Slerp( m_Mesh.vertices[i], target.vertices[i], 0.5f * Time.deltaTime );
					texCoords[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], 0.5f * Time.deltaTime );
					normals[i] = Vector3.Slerp( m_Mesh.normals[i], target.normals[i], 0.5f * Time.deltaTime );

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
		}
		m_Mesh.vertices = target.vertices;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
		m_Mesh.RecalculateBounds();
		Debug.Log( "Complete" );
	}

	private IEnumerator CircleToPlane() {

		Mesh target;
		Vector3[] vertices = new Vector3[width * height];
		Vector2[] uvs = new Vector2[width * height];
		target = ProceduralMesh.GeneratePlane( height, width );
		float time = 0.0f;
		

		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) != 0.0f ) {
			m_BorderAlpha = Mathf.Clamp( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ), 0.0f, 1.0f );
			if ( !paused ) {
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], time );
					uvs[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], time );
				}
				m_Mesh.vertices = vertices;
				m_Mesh.uv = uvs;
				time = ( Time.deltaTime / Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) );
			}
			yield return null;
		}
		m_BorderAlpha = 0.0f;
		m_Mesh.vertices = target.vertices;
		m_Mesh.triangles = target.triangles;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
		m_Mesh.bounds = target.bounds;
	}

	private IEnumerator PlaneToCircle() {

		Mesh target;
		Vector3[] vertices = new Vector3[width * height];
		Vector2[] uvs = new Vector2[width * height];
		target = ProceduralMesh.GenerateCurvedCylinderSegment( height, width, 1.0f, 0.75f );
		m_Mesh.bounds = target.bounds;
		m_Mesh.triangles = target.triangles;
		float time = 0.0f;

		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) != 0.0f ) {
			if ( !paused ) {
				m_BorderAlpha = 1.0f - Mathf.Clamp( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ), 0.0f, 1.0f );
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], time );
					uvs[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], time );
				}
				m_Mesh.vertices = vertices;
				m_Mesh.uv = uvs;
				time = ( Time.deltaTime / Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) );
			}
			yield return null;
		}
		m_BorderAlpha = 1.0f;
		m_Mesh.vertices = target.vertices;
		m_Mesh.triangles = target.triangles;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
	}

	private IEnumerator FixRotation() {
		Vector3 target = new Vector3( 0.0f, 0.0f, 0.0f );
		Vector3 current = gameObject.transform.localRotation.eulerAngles;

		float time = 0.0f;

		while ( gameObject.transform.localRotation.eulerAngles != target ) {
			time = Time.deltaTime / Vector3.Distance(gameObject.transform.localRotation.eulerAngles, target);
			gameObject.transform.localRotation = Quaternion.Euler( Vector3.Lerp( current, target, time ) );
		
			yield return null;
		}
		StartCoroutine( CircleToPlane() );
		StartCoroutine( MoveAndScale( Vector3.zero, new Vector3( 2.0f, 2.0f, 1.0f ), Vector3.zero ) );
	}

	private IEnumerator MoveAndScale( Vector3 _targetPos, Vector3 _targetScale, Vector3 _targetRot ) {
		float distance = 1.0f;
		float scale = 1.0f;
		float time = 0.0f;
		float rotation = 1.0f;

		Vector3 startPos = gameObject.transform.localPosition;
		Vector3 startScale = gameObject.transform.localScale;
		Vector3 startRot = gameObject.transform.localRotation.eulerAngles;
		yield return null;
		while ( distance > 0.01f || scale > 0.01f || rotation > 0.01f ) {
			if ( !paused ) {
				
				time += Time.deltaTime * 0.5f;
				distance = Vector3.Distance( gameObject.transform.localPosition, _targetPos );
				scale = Vector3.Distance( gameObject.transform.localScale, _targetScale );
				rotation = Vector3.Distance( gameObject.transform.localRotation.eulerAngles, _targetRot );

				gameObject.transform.localPosition = Vector3.Lerp( startPos, _targetPos, time );
				gameObject.transform.localScale = Vector3.Lerp( startScale, _targetScale, time );
				gameObject.transform.localRotation = Quaternion.Euler( Vector3.Lerp( startRot, _targetRot, time ) );
			}
			yield return null;
		}
		gameObject.transform.localPosition = _targetPos;
		gameObject.transform.localScale = _targetScale;
		gameObject.transform.localRotation = Quaternion.Euler( _targetRot );
	}

}