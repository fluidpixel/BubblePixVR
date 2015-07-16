using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

/* Prefab script that defines each thumbnail and
 * it's individual behaviour within the browser.
 */

public class ThumbTile : MonoBehaviour {

#region Variable Declarations

	[SerializeField]
	private MeshRenderer m_Mesh;

	[SerializeField]
	private MeshRenderer m_InfoPanel;

	[SerializeField]
	private MeshRenderer m_LargeInfoPanel;

	[SerializeField]
	private MeshAnimator m_Animator;

	[SerializeField]
	private Text m_Text;

	[SerializeField]
	private Text m_LargeText;

	private AppController m_AppController;
	private FileHandler.Thumbnail m_Thumb;
	private bool m_Selected = false;
	private bool m_Selectable;
	private bool m_Clicked;
	private int m_CarouselPos;
	private bool m_isViewing = false;

#endregion

#region Properties
	public FileHandler.Thumbnail Image {
		get { return m_Thumb; }
	}

	public MeshAnimator Animator {
		get { return m_Animator; }
	}

	public Transform MeshTransform {
		get { return m_Mesh.transform; }
	}

	public int CarouselPos {
		get { return m_CarouselPos; }
		set { m_CarouselPos = value; }
	}

	public string ImageString {
		get { return m_Thumb.ImageLoc; }
	}

#endregion

#region MonoBehaviour Overrides

	void Start() {
		m_AppController = GetComponentInParent<AppController>() as AppController;
	}

	void Update() {
		if ( m_AppController.TC.Swiping ) {
			m_Selected = false;
			m_Clicked = false;
		}

		if ( m_AppController.VRMode ) {
			m_InfoPanel.gameObject.SetActive( true );
			m_LargeInfoPanel.gameObject.SetActive( false );
		}
		else {
			m_InfoPanel.gameObject.SetActive( false );
			m_LargeInfoPanel.gameObject.SetActive( true );
		}

		Vector3 pos = m_Mesh.gameObject.transform.position;

		float xdist = ( Mathf.Abs( pos.x ) - 2.0f ) * 0.1f;
		float ydist = Mathf.Abs( pos.y - 0.93f ) - 1.0f;

		Vector4 color = m_Mesh.material.color;
		Vector4 infoColor = m_InfoPanel.material.color;
		Vector4 textColor = m_Text.color;

		xdist = Mathf.Clamp(xdist, 0.0f, 1.0f);
		ydist = Mathf.Clamp(ydist, 0.0f, 1.0f);

		float fade = Mathf.Max(xdist, ydist);

		if ( m_Selected || m_Clicked ) {
			color.w = 1.0f;
			textColor.w = 1.0f;
			infoColor.w = 0.6f;
		}
		else {
			color.w = 1.0f - fade;
			textColor.w = 0.0f;
			infoColor.w = 0.0f;
		}

		m_Mesh.material.color = color;
		m_InfoPanel.material.color = infoColor;
		m_LargeInfoPanel.material.color = infoColor;
		m_Text.color = textColor;
		m_LargeText.color = textColor;

		if ( pos.x < 2.5f && pos.x > -2.5f && pos.y < 2.0f && pos.y > -0.3f ) {
			m_Selectable = true;
		}
		else {
			m_Selectable = false;
			m_Clicked = false;
			//m_Selected = false;
		}
	}

#endregion

#region Mutator Methods

	public void SetThumb( FileHandler.Thumbnail _thumb ) {
		m_Thumb = _thumb;
		m_Mesh.material.mainTexture = m_Thumb.Thumb;
		m_Text.text = InfoString( m_Thumb );
		m_LargeText.text = InfoString( m_Thumb );
	}

	public void SetPos( Vector3 _pos ) {
		this.gameObject.transform.localPosition = _pos;
	}

#endregion

#region User-Interface Methods

	public void Selected() {
		if ( m_Selectable && m_AppController.VRMode && !m_isViewing )
			m_Selected = true;
			m_AppController.PointerColor = 1;
	}

	public void DeSelected() {
		m_Selected = false;
		m_AppController.PointerColor = 0;
	}

	public void DeClicked() {
		m_Selected = false;
		m_Clicked = false;
	}

	public void Clicked() {
		if ( !m_isViewing ) {
			if ( !m_AppController.VRMode ) {
				if ( m_Clicked ) {
					m_Clicked = false;
					DeSelected();
					m_isViewing = true;
					m_AppController.BrowserToPano( this );
				}
				else {
					( GetComponentInParent<AppController>() as AppController ).TB.DeselectThumbs();
					m_Clicked = true;
				}
			}
			else if ( m_Selected ) {
				DeSelected();
				m_isViewing = true;
				m_AppController.BrowserToPano( this );
			}
			else if ( !m_Selectable ) {
				m_AppController.TB.SweepToCentre(this.gameObject);
			}
		}
	}

	public void SetComponentsActive( bool _arg ) {
		m_LargeInfoPanel.gameObject.SetActive( _arg );
		m_InfoPanel.gameObject.SetActive( _arg );
		m_isViewing = !_arg;
	}


#endregion

	private string InfoString( FileHandler.Thumbnail _thumb ) {
		return "<b>Name: </b>" + Path.GetFileNameWithoutExtension( _thumb.ImageLoc ) + 
				" \n <b>Resolution: </b>" + _thumb.Width + "x" + _thumb.Height + 
				" \n <b>Location: </b>" + _thumb.Country +
				" \n <b>Date Taken: </b>" + _thumb.DateString;
	}
}