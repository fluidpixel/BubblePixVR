using UnityEngine;
using System.Collections;

public class JavaVideoInterface : MonoBehaviour {
	
	private AndroidJavaObject m_VideoPlayer = null;

	public int Height {
		get {
			if ( m_VideoPlayer != null )
				return m_VideoPlayer.Call<int>( "Height" );
			else {
				return -1;
			}
		}
	}
	public int Width {
		get {
			if ( m_VideoPlayer != null )
				return m_VideoPlayer.Call<int>( "Width" );
			else {
				return -1;
			}
		}
	}
	public int Length {
		get { 
			if (m_VideoPlayer != null)
				return m_VideoPlayer.Call<int>( "Length" );
			else {
				return -1; 
			}
		}
	}

	void Start() {
		m_VideoPlayer = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.VideoPlayer" );
	}

	public void PrepareVideo( string _path, int _intPtr ) {
		if ( !m_VideoPlayer.Call<bool>( "PrepareVideo", _path, _intPtr ) ) {
			Debug.LogError( "Loading Video Failed" );
		}
	}

	public void PlayVideo() {
		m_VideoPlayer.Call( "StartVideo" );
	}
}
