using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

	[SerializeField]
	private GameObject m_BrowserButton;

	[SerializeField]
	private GameObject m_MenuPanel;

	[SerializeField]
	private Camera m_MainCamera;

	private bool m_Moving = false;
	private Vector3 m_Target = new Vector3( 0.0f, 0.1f, 0.0f );

	void Update() {
		Vector3 menu, camera;
		menu = m_MenuPanel.transform.rotation.eulerAngles;
		camera = m_MainCamera.transform.rotation.eulerAngles;

		menu.y = Mathf.Clamp(camera.y, 0.0f, 360.0f);
		m_MenuPanel.transform.rotation = Quaternion.Euler(menu);
	}

	public bool IsMoving
	{
		get { return m_Moving; }
	}

	public Vector3 GetPosition
	{
		get { return this.transform.position; }
	}

	public void BrowserButtonActive( bool _arg ) {
		m_BrowserButton.SetActive( _arg );
	}

	private Vector3 Position
	{
		set { this.transform.position = value; }
	}

	public void Return()
	{
		if ( m_Moving )
		{
			StopCoroutine( "CameraLerp" );
			Position = m_Target;
		}
		StartCoroutine( CameraLerp( new Vector3( 0.0f, 1.0f, 0.0f ), 2.0f ) );
	}

	public void MoveCamera( Vector3 _EndPos, float _t )
	{
		if ( m_Moving )
		{
			StopCoroutine( "CameraLerp" );
			Position = m_Target;
		}
		StartCoroutine( CameraLerp( _EndPos, _t ) );
	}

	private IEnumerator CameraLerp( Vector3 _EndPos, float _t )
	{
		m_Target = _EndPos;
		m_Moving = true;
		float diff = Vector3.Distance( _EndPos, GetPosition ) * 0.01f;
		while ( GetPosition != _EndPos )
		{
			Position = Vector3.Slerp( GetPosition, _EndPos, _t * Time.deltaTime );
			if ( Vector3.Distance( _EndPos, GetPosition ) < diff )
			{
				Position = _EndPos;
			}
			yield return null;
		}
		m_Moving = false;
		yield return null;
	}
}
