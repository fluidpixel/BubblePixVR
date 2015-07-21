using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// Handles the meshes related to the viewing of panoramas,
// along with the methods that move and enable parts of the viewer.

public class PanoramaViewer : MonoBehaviour {

#region Variable Declarations

	[SerializeField]
	private GameObject m_Cylinder;

	[SerializeField]
	private GameObject m_Sphere;

	[SerializeField]
	private Transform m_MeshAnchor;

	[SerializeField]
	private GameObject m_Capsule;

	[SerializeField]
	private MeshRenderer m_BackButton;

	[SerializeField]
	private MeshRenderer m_TestPanel;

	[SerializeField]
	private GameObject m_PanoButtons;

	[SerializeField]
	private VideoPlayer m_VideoPlayer;

	private bool m_IsCylinder = true;
	private bool isMoving = false;
	//private bool videoMode = false;
	private MeshRenderer m_ActiveMesh;
	private AppController m_Controller;
	private Vector3 m_Target;
	private ThumbTile m_ActiveThumb;
	private int m_ImageCount;

#endregion

#region Accessor/Mutator Methods

	public MeshRenderer TestPanel {
		get { return m_TestPanel; }
	}

	public ThumbTile ActiveThumb {
		get { return m_ActiveThumb; }
		set { m_ActiveThumb = value; }
	}

	public void SetCapActive( bool _arg ) {
		m_Capsule.SetActive( _arg );
	}

#endregion

#region Monobehaviour Overrides

	void Awake() {
		m_Controller = GameObject.Find( "SceneObjects" ).GetComponent<AppController>() as AppController;
	}

	void Start() 
	{
		m_ActiveMesh = m_Cylinder.GetComponent( "MeshRenderer" ) as MeshRenderer;
	}

	void Update() {
		if ( m_Controller.State == AppController.AppState.Viewer ) {
			if ( m_Controller.VRMode ) {
				m_PanoButtons.SetActive( false );
			}
			else {
				m_PanoButtons.SetActive( true );
			}

			if ( m_Controller.TC.SwipeDirection[0] == TouchController.Swipe.Positive  ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, Mathf.Min( m_Controller.TC.SwipeSpeed.x * 0.04f, 4.0f ), 0.0f ) );
			}
			else if ( m_Controller.TC.SwipeDirection[0] == TouchController.Swipe.Negative ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, -Mathf.Min( m_Controller.TC.SwipeSpeed.x * 0.04f, 4.0f ), 0.0f ) );
			}
			else if ( Input.GetKey( KeyCode.RightArrow ) ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, 1.0f, 0.0f ) );
			}
			else if ( Input.GetKey( KeyCode.LeftArrow ) ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, -1.0f, 0.0f ) );
			}

		}
		else {
			m_PanoButtons.SetActive( false );
		}

		
	}

#endregion

	public void SwapMesh() 
	{
		if ( m_IsCylinder )
		{
			m_Cylinder.SetActive( false );
			( m_Sphere.GetComponent( "MeshRenderer" ) as MeshRenderer ).renderer.material = m_ActiveMesh.renderer.material;
			m_ActiveMesh = m_Sphere.GetComponent( "MeshRenderer" ) as MeshRenderer;
			m_Sphere.SetActive( true );
		}
		else
		{
			m_Sphere.SetActive( false );
			( m_Cylinder.GetComponent( "MeshRenderer" ) as MeshRenderer ).renderer.material = m_ActiveMesh.renderer.material;
			m_ActiveMesh = m_Cylinder.GetComponent( "MeshRenderer" ) as MeshRenderer;
			m_Cylinder.SetActive( true );
		}
		m_IsCylinder = !m_IsCylinder;
	}

	public void ViewPanorama( Texture2D _tex ) 
	{		
		//if ( isMoving ) {
		//	StopCoroutine( "MoveMesh" );
		//	m_MeshAnchor.localPosition = m_Target;
		//}
		//
		//if ( m_IsCylinder )
		//{
		//	m_Cylinder.SetActive( true );
		//}
		//else
		//{
		//	m_Sphere.SetActive( true );
		//}
		//m_ActiveMesh.renderer.material.mainTexture = _tex;
		//StartCoroutine( MoveMesh( true ) );
		m_BackButton.gameObject.SetActive( false );
	}

	public void ViewVideo() {
		//videoMode = true;
	}

	public void ExitPanorama() 
	{
		//if ( isMoving ) { 
		//	StopCoroutine("MoveMesh");
		//	m_MeshAnchor.localPosition = m_Target;
		//}

		//StartCoroutine( MoveMesh( false ) );

		m_BackButton.gameObject.SetActive( false );
	}

	public void ButtonClicked() {
		m_Controller.PanoToBrowser();
	}

	private IEnumerator MoveMesh( bool _up ) {
		isMoving = true;
		if ( _up ) {
			m_Target = new Vector3( 0.0f, 0.0f, 0.0f );
		}
		else {
			m_Target = new Vector3( 0.0f, -10.0f, 0.0f );
			m_BackButton.gameObject.SetActive( false );
		}

		float diff = Vector3.Distance( m_Target, m_MeshAnchor.localPosition ) * 0.1f;
		
		while ( m_MeshAnchor.localPosition != m_Target ) {
			m_MeshAnchor.localPosition = Vector3.Slerp( m_MeshAnchor.localPosition, m_Target, 3.0f * Time.deltaTime );
			if ( Vector3.Distance( m_Target, m_MeshAnchor.localPosition ) < diff ) {
				m_MeshAnchor.localPosition = m_Target;
			}
			yield return null;
		}

		if ( !_up ) {
			if ( m_IsCylinder ) {
				m_Cylinder.SetActive( false );
			}
			else {
				m_Sphere.SetActive( false );
			}
		}
		else {
			m_BackButton.gameObject.SetActive( true );
		}
		
		isMoving = false;
		yield return null;
	}
}