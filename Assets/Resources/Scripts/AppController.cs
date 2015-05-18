﻿using UnityEngine;
using System.Collections;
using System.Globalization;

public class AppController : MonoBehaviour {

	enum AppState {
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

	private AppState m_State = AppState.Browser;
	private bool focusLost = false;

	void Start() {
		MenuToBrowser();
	}

	void Update() {
#if UNITY_EDITOR
		if ( Input.GetKeyDown( KeyCode.Escape ) ) {
			if ( m_State == AppState.Viewer ) {
				PanoToBrowser();
			}
		}
#endif
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

		m_CameraController.MoveCamera( new Vector3( 0.0f, 1.0f, -15.0f ), 2.0f );
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
}
