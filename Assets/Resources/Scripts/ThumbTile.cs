using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class ThumbTile : MonoBehaviour {

	[SerializeField]
	private MeshRenderer m_Mesh;

	[SerializeField]
	private MeshRenderer m_InfoPanel;

	[SerializeField]
	private Text m_Text;

	private FileHandler.Thumbnail m_Thumb;
	private bool m_Selected = false;
	private bool selectable;

	public FileHandler.Thumbnail Image {
		get { return m_Thumb; }
	}

	void Update() {
		Vector3 pos = m_Mesh.gameObject.transform.position;

		float xdist = (Mathf.Abs( pos.x ) - 2.0f) * 0.5f;
		float ydist = Mathf.Abs( pos.y - 0.93f ) - 1.0f;

		Vector4 color = m_Mesh.material.color;
		Vector4 infoColor = m_InfoPanel.material.color;
		Vector4 textColor = m_Text.color;

		xdist = Mathf.Clamp(xdist, 0.0f, 1.0f);
		ydist = Mathf.Clamp(ydist, 0.0f, 1.0f);

		float fade = Mathf.Max(xdist, ydist);

		if ( m_Selected ) {
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
		m_Text.color = textColor;

		if ( pos.x < 2.5f && pos.x > -2.5f ) {
			selectable = true;
		}
		else {
			selectable = false;
		}
	}

	public void SetThumb( FileHandler.Thumbnail _thumb ) {
		m_Thumb = _thumb;
		m_Mesh.material.mainTexture = m_Thumb.Thumb;
		m_Text.text = InfoString( m_Thumb );
	}

	public string GetImageString() {
		return m_Thumb.ImageLoc;
	}

	public void SetPos( Vector3 _pos ) {
		this.gameObject.transform.localPosition = _pos;
		if ( _pos.y == 1.0f ) {
			m_InfoPanel.transform.position = m_InfoPanel.transform.position - new Vector3( 0.0f, 0.1f, 0.0f );//top
		}
		else if ( _pos.y == -1.0f ) {
			m_InfoPanel.transform.position = m_InfoPanel.transform.position + new Vector3( 0.0f, 0.06f, -0.1f );//bottom
		}
	}

	public void Selected() {
		if ( selectable )
			m_Selected = true;
	}

	public void DeSelected() {
		m_Selected = false;
	}

	public void Clicked() {
		if ( m_Selected ) { 
			DeSelected();
			AppController AC = GetComponentInParent<AppController>() as AppController;
			AC.BrowserToPano( this );
		}
	}

	private string InfoString( FileHandler.Thumbnail _thumb ) {
		return "<b>Name: </b>" + Path.GetFileNameWithoutExtension( _thumb.ImageLoc ) + 
				" \n <b>Resolution: </b>" + _thumb.Width + "x" + _thumb.Height + 
				" \n <b>Location: </b>" + _thumb.Country +
				" \n <b>Date Taken: </b>" + _thumb.DateString;
	}

}
