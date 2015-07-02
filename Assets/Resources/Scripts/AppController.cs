using UnityEngine;
using System.Collections;
using System.Globalization;

//Centralised controller class that allows all other classes access to each other, especially
//for access to things like the File Handler and Touch Controller. Also maintains track of the 
//state of the app and manages the transtions between each.

public class AppController : MonoBehaviour {

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

	private AppState m_State = AppState.Browser;
	private bool focusLost = false;
	private float faceTime = 0.0f;
	bool facePhone = false;

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

	//Camera Transitions
	public void MenuToBrowser() {
		m_State = AppState.Browser;

		m_PanoViewer.gameObject.SetActive( true );
		m_ThumbBrowser.ViewBrowser();
		m_PanoViewer.SetCapActive( true );
	}

	public void BrowserToPano( ThumbTile _tile ) {
		m_State = AppState.Viewer;

		m_ThumbBrowser.ViewImage();
		m_PanoViewer.ViewPanorama( m_FileHandler.TexFromThumb( _tile.Image ) );
		m_CameraController.BrowserButtonActive( true );
	}

	public void PanoToBrowser() {
		m_State = AppState.Browser;

		m_PanoViewer.ExitPanorama();
		m_ThumbBrowser.ReturnToBrowser();
		m_CameraController.BrowserButtonActive( false );
	}

	public void VrMode() {
		if ( m_Cardboard.VRModeEnabled ) {
			m_Head.trackPosition = m_Head.trackRotation = false;
			m_Head.transform.rotation = new Quaternion();
			m_ThumbBrowser.To2DView();
		}
		else {
			m_Head.trackPosition = m_Head.trackRotation = true;
			m_Head.transform.rotation = new Quaternion();
			m_ThumbBrowser.To3DView();
		}
		m_Cardboard.ToggleVRMode();
	}

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
}