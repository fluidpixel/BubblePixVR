using UnityEngine;
using System.Collections;

/* Provides methods for altering the pointer colors,
 * along with notification text attached to it. 
 */

public class Pointer : MonoBehaviour {
	
	[SerializeField]
	private MeshRenderer m_Pointer;

	[SerializeField]
	private TextMesh m_Text;

	private Color m_ButtonColor = Color.green;
	private Color m_ToggleColor = Color.yellow;
	private Color m_DefaultColor = Color.white;

	public void SetText( string _arg ) {
		m_Text.text = _arg;
	}

	public void SetColor( int _arg ) {
		if ( _arg == 0 ) {
			m_Pointer.material.color = m_DefaultColor;
		}
		else if ( _arg == 1 ) {
			m_Pointer.material.color = m_ButtonColor;
		}
		else {
			m_Pointer.material.color = m_ToggleColor;
		}
	}

}
