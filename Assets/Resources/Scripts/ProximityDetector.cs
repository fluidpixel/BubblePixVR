using UnityEngine;
using System.Collections;

public class ProximityDetector : MonoBehaviour {
	
	private AndroidJavaObject m_ProximityDetector = null;
	private AndroidJavaObject m_ActivityContext = null;

	private float m_Distance = -1.0f;

	public bool Near {
		get { return m_Distance > 1.0f ? false : true; }
	}

	// Use this for initialization
	void Start () {

#if UNITY_ANDROID && !UNITY_EDITOR
		m_ProximityDetector = new AndroidJavaObject("com.sherif.cardboard3d.bitmaphandler.ProximityChecker", GetActivityContext());
#endif
	}
	
	// Update is called once per frame
	void Update () {
		if (m_ProximityDetector != null)
			m_Distance = m_ProximityDetector.Call<float>("Distance");
	}

	private AndroidJavaObject GetActivityContext() {
		if ( m_ActivityContext == null ) {
			AndroidJavaClass jc = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
			m_ActivityContext = jc.GetStatic<AndroidJavaObject>( "currentActivity" );
		}
		return m_ActivityContext;
	}
}