using UnityEngine;
using System.Collections;

public class TextPanel : MonoBehaviour {

	[SerializeField]
	private TextMesh m_TextMesh;

	[SerializeField]
	private MeshRenderer m_Panel;

	public TextMesh Text {
		get { return m_TextMesh; }
	}

	public MeshRenderer Mesh {
		get { return m_Panel; }
	}

	void Update() {
		Vector3 pos = gameObject.transform.position;

		float dist = Mathf.Clamp( Mathf.Abs( pos.x ) * 0.3f, 0.0f, 1.0f );

		Vector4 color = m_TextMesh.renderer.material.color;
		Vector4 panelColor = m_Panel.renderer.material.color;

		color.w = 1.0f - dist;
		panelColor.w = 1.0f - dist;

		m_TextMesh.renderer.material.color = color;
		m_Panel.renderer.material.color = panelColor;
	}

	public void SetPos( Vector3 _pos ) {
		this.gameObject.transform.localPosition = _pos;
	}

	public void SetText( string _text ) {
		m_TextMesh.text = _text;
	}
}
