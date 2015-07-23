using UnityEngine;
using System.Collections;
using System.Globalization;

//Centralised controller class that allows all other classes access to each other, especially
//for access to things like the File Handler and Touch Controller. Also maintains track of the 
//state of the app and manages the transtions between each.

public class AppController : MonoBehaviour {

#region Variable Declarations

	public enum AppState {
		Browser = 0,
		Viewer = 1
	};

	[SerializeField]
	private PanoramaViewer m_PanoViewer;

	[SerializeField]
	private ThumbBrowser m_ThumbBrowser;

	[SerializeField]
	private CameraController m_CameraController;

	[SerializeField]
	private FileHandler m_FileHandler;

	[SerializeField]
	private TouchController m_TouchController;

	[SerializeField]
	private Cardboard m_Cardboard;

	[SerializeField]
	private CardboardHead m_Head;

	[SerializeField]
	private ProximityDetector m_ProximityDetector;

	[SerializeField]
	private Pointer m_Pointer;

	private AppState m_State = AppState.Browser;
	private bool focusLost = false;
	private float faceTime = 0.0f;
	bool facePhone = false;

#endregion

#region Properties

	public FileHandler FH {
		get { return m_FileHandler; }
	}
	public CameraController CC {
		get { return m_CameraController; }
	}
	public PanoramaViewer PV {
		get { return m_PanoViewer; }
	}
	public ThumbBrowser TB {
		get { return m_ThumbBrowser; }
	}
	public TouchController TC {
		get { return m_TouchController; }
	}
	public bool VRMode {
		get { return m_Cardboard.VRModeEnabled; }
	}
	public AppState State {
		get { return m_State; }
	}
	public int PointerColor {
		set { m_Pointer.SetColor(value); }
	}

#endregion

#region Monobehavior Overrides

	void Start() {
		MenuToBrowser();
		VrMode();
	}

	void Update() {
		if ( Input.GetKeyDown( KeyCode.Escape ) ) {
			if ( m_State == AppState.Browser ) {
				Application.Quit();
			}
			else if ( m_State == AppState.Viewer ) {
				#if UNITY_EDITOR
				PanoToBrowser();
				#endif
			}
		}
		if ( Input.GetKeyDown( KeyCode.Alpha3 ) ) {
			VrMode();
		}

		if (m_ProximityDetector.Distance != -1.0f)
			CheckIfViewer();
	}

	void OnApplicationFocus( bool _focus ) {
		if ( _focus ) {
			if ( focusLost ) {
				if ( m_State != AppState.Browser ) {
					PanoToBrowser();
				}
				else {
					m_ThumbBrowser.RefreshBrowser();
				}
			}
			else {
				focusLost = true;
			}
		}
	}

#endregion

#region Transition Methods

	public void MenuToBrowser() {
		m_State = AppState.Browser;

		m_PanoViewer.gameObject.SetActive( true );
		m_ThumbBrowser.gameObject.SetActive( true );
		m_ThumbBrowser.PopThumbs();		
	}

	public void BrowserToPano( ThumbTile _tile ) {
		m_State = AppState.Viewer;

		m_ThumbBrowser.HideCarousel(_tile.CarouselPos);
		_tile.SetComponentsActive( false );
		m_ThumbBrowser.gameObject.SetActive( false );
		m_PanoViewer.ActiveThumb = _tile;
		//Vector3 targetPos = _tile.MeshTransform.InverseTransformPoint( m_CameraController.gameObject.transform.position );
		_tile.Animator.ToCylinder( m_CameraController.gameObject.transform.position );
		m_CameraController.BrowserButtonActive( true );
		m_Pointer.UnsetText();
	}

	public void PanoToBrowser() {
		m_State = AppState.Browser;
		m_PanoViewer.ActiveThumb.StopViewing();
		m_PanoViewer.ActiveThumb.Animator.ToPlane();
		m_CameraController.BrowserButtonActive( false );
		StartCoroutine( WaitForComplete( m_PanoViewer.ActiveThumb ) );
	}

	public void VrMode() {
		if ( m_Cardboard.VRModeEnabled ) {
			m_Head.trackPosition = m_Head.trackRotation = false;
			m_Head.transform.rotation = new Quaternion();
			m_ThumbBrowser.To2DView();
			m_Pointer.UnsetText();
		}
		else {
			m_Head.trackPosition = m_Head.trackRotation = true;
			m_Head.transform.rotation = new Quaternion();
			m_ThumbBrowser.To3DView();
			m_Pointer.UnsetText();
		}
		m_Cardboard.VRModeEnabled = !m_Cardboard.VRModeEnabled;
	}

#endregion

	private void CheckIfViewer() {
		if ( m_ProximityDetector.Distance < 1.0f ) {
			facePhone = true;
		}
		else {
			facePhone = false;
		}

		if ( facePhone != VRMode ) {
			faceTime += Time.deltaTime;
		}
		else {
			faceTime = 0.0f;
		}

		if ( faceTime >= 2.0f ) {
			VrMode();
			faceTime = 0.0f;
		}
	}

	private IEnumerator WaitForComplete( ThumbTile _tile ) {
		while ( !_tile.Animator.IsPlane ) {
			yield return null;
		}
		m_ThumbBrowser.gameObject.SetActive( true );
		m_ThumbBrowser.ShowCarousel();
		m_PanoViewer.ActiveThumb.SetComponentsActive( true );
	}
}