using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class JavaVideoInterface : MonoBehaviour {
	
	private AndroidJavaObject m_VideoPlayer = null;
	private AndroidJavaObject m_PathGrabber = null;
	private AndroidJavaObject m_ActivityContext = null;
	private AndroidJavaObject m_Tester = null;

	[DllImport ("NDKBridge")]
	static extern int intFromJNI();

	public int Height {
		get {
			if ( m_VideoPlayer != null )
				return m_VideoPlayer.Call<int>( "Height" );
			else
				return -1;
		}
	}
	public int Width {
		get {
			if ( m_VideoPlayer != null )
				return m_VideoPlayer.Call<int>( "Width" );
			else 
				return -1;
		}
	}
	public int Length {
		get { 
			if (m_VideoPlayer != null)
				return m_VideoPlayer.Call<int>( "Length" );
			else 
				return -1; 
		}
	}
	public bool IsPlaying {
		get { 
			if (m_VideoPlayer != null)
				return m_VideoPlayer.Call<bool>("IsPlaying"); 
			else
				return false;
		}
	}
	public bool IsReady {
		get { 
			if (m_VideoPlayer != null)
				return m_VideoPlayer.Call<bool>("IsReady");
			else
				return false;
		}
	}

	public string TestJNI() {
		string ret;
		m_Tester = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.VideoTester" );
		
		if (m_Tester != null)
			ret = m_Tester.Call<string>( "Test" );
		else
			ret = "broke";
		return ret;
	}

	public string GetVideoPaths() {
		m_PathGrabber = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.GalleryInterface", GetActivityContext() );
		string[] ret = m_PathGrabber.Call<string[]>( "GetGalleryVideoPaths" );
		return ret[0];
	}

	public void PrepareVideo( int _intPtr ) {	
		m_VideoPlayer.Call( "PrepareVideo", _intPtr );
	}

	public void PlayVideo() {
		m_VideoPlayer.Call( "StartVideo" );
	}

	public void InitPlayer(string _path) {
		if ( m_VideoPlayer != null ) {
			m_VideoPlayer.Dispose();
			m_VideoPlayer = null;
		}
		m_VideoPlayer = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.VideoPlayer", _path, GetActivityContext() );
	}

	private AndroidJavaObject GetActivityContext() {
		if ( m_ActivityContext == null ) {
			AndroidJavaClass jc = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
			m_ActivityContext = jc.GetStatic<AndroidJavaObject>( "currentActivity" );
		}
		return m_ActivityContext;
	}
}
