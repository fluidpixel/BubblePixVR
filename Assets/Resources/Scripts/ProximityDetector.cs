using UnityEngine;
using System.Collections;

public class ProximityDetector : MonoBehaviour {
	
	private AndroidJavaObject m_ProximityDetector = null;
	private AndroidJavaObject m_ActivityContext = null;

	private float m_Distance;

	public float Distance {
		get { return m_Distance; }
	}

	// Use this for initialization
	void Start () {
		m_Distance = -1.0f;

#if UNITY_ANDROID && !UNITY_EDITOR
		m_Distance = 0.0f;
		Debug.Log("ASDASDASDA");
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