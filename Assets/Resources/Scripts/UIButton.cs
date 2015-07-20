using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour {
	
	[SerializeField]
	private bool m_IsToggle;

	[Tooltip( "String that is displayed on the pointer when hovered, leave blank if no text should show" )]
	[SerializeField]
	private string m_PointerString = null;

	private Color m_Color = new Color( 0.34f, 0.48f, 0.72f, 0.47f );
	private Color m_ColorHover = new Color( 0.34f, 0.48f, 0.72f, 0.74f );
	private Color m_ActiveColor = new Color( 0.32f, 0.7f, 0.4f, 0.47f );
	private Color m_ActiveColorHover = new Color( 0.32f, 0.7f, 0.4f, 0.74f );

	private Color[] m_Colors = { new Color( 0.34f, 0.48f, 0.72f, 0.47f ),	//Color
								 new Color( 0.34f, 0.48f, 0.72f, 0.74f ),	//Hover Color
								 new Color( 0.32f, 0.7f, 0.4f, 0.47f ),		//Active Color
								 new Color( 0.32f, 0.7f, 0.4f, 0.74f ) };	//Active Hover Color

	private bool m_IsActive;
	private MeshRenderer m_Mesh = null;
	private Pointer m_Pointer = null;

	// Use this for initialization
	void Start () {
		m_Mesh = GetComponent<MeshRenderer>() as MeshRenderer;
		m_Pointer = GameObject.Find("GazePointer").GetComponent<Pointer>() as Pointer;

		if (m_Pointer == null)
		Debug.Log("Could not find the pointer");

		if (m_Mesh == null)
		Debug.Log("Could not find the mesh");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnHover() {
		if ( m_IsActive )
			m_Mesh.material.color = m_Colors[3];
		else
			m_Mesh.material.color = m_Colors[1];

		if ( m_PointerString != null )
			m_Pointer.SetText( m_PointerString );
	}

	public void OnNoHover() {
		if ( m_IsActive )
			m_Mesh.material.color = m_Colors[2];
		else
			m_Mesh.material.color = m_Colors[0];

		if ( m_PointerString != null )
			m_Pointer.UnsetText();
	}

	public void OnClick() {
		if ( m_IsToggle ) {
			m_IsActive = !m_IsActive;

			if ( m_IsActive )
				m_Mesh.material.color = m_Colors[3];
			else
				m_Mesh.material.color = m_Colors[1];

		}
	}

	public void SetClicked( bool _arg ) {
		if ( _arg ) {
			m_Mesh.material.color = m_Colors[2];
			m_IsActive = true;
		}
		else {
			m_Mesh.material.color = m_Colors[0];
			m_IsActive = false;
		}
	}
}
