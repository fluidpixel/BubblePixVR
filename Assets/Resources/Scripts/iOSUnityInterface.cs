using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;

#if UNITY_IOS && !UNITY_EDITOR


public class iOSUnityInterface : MonoBehaviour {
	// Class compiler for iOS attaches to iOS plugin
	
	/* Interface to native implementation */
	
	[DllImport ("__Internal")]
	private static extern int _iOS_Gallery__GetPanoramaCount();
	
	[DllImport ("__Internal")]
	private static extern System.IntPtr _iOS_Gallery__GetLocalID(int index);
	
	[DllImport ("__Internal")]
	private static extern int _iOS_Gallery__GetPanoramaWidth (string localID);
	
	[DllImport ("__Internal")]
	private static extern int _iOS_Gallery__GetPanoramaHeight (string localID);
	
	[DllImport ("__Internal")]
	private static extern System.IntPtr _iOS_Gallery__GetPanoramaDateTaken (string localID);
	
	[DllImport ("__Internal")]
	private static extern System.IntPtr _iOS_Gallery__GetPanoramaCountry (string localID);
	
	[DllImport ("__Internal")]
	private static extern System.IntPtr _iOS_Gallery__CreateTempPanoFile (string localID);
	
	[DllImport ("__Internal")]
	private static extern void _iOS_Gallery__ReleaseTempPanoFile (string localID);
	
	/* Public interface for use inside C# / JS code */
	
	public int GetWidth( string _id ) {
		
		return _iOS_Gallery__GetPanoramaWidth( _id );
		
	}
	public int GetHeight( string _id ) {
		return _iOS_Gallery__GetPanoramaHeight( _id );
		
	}
	public string GetCountry( string _id ) {
		return  Marshal.PtrToStringAuto (_iOS_Gallery__GetPanoramaCountry (_id));
		
	}
	public string GetDate( string _id ) {
		return Marshal.PtrToStringAuto (_iOS_Gallery__GetPanoramaDateTaken (_id));
	}
	
	public string[] GetImages() {
		int images = _iOS_Gallery__GetPanoramaCount();
		string[] ret = new string[images];
		for ( int i = 0; i < images; i++ ) {
			ret[i] = Marshal.PtrToStringAuto(_iOS_Gallery__GetLocalID(i));
		}
		return ret;
	}
	
	// Starts lookup for some bonjour registered service inside specified domain
	private static int PanoramaCount() {
		return _iOS_Gallery__GetPanoramaCount();
		
	}
	
	//Stops lookup current lookup
	private static string GetLocalID(int index)
	{
		return Marshal.PtrToStringAuto(_iOS_Gallery__GetLocalID (index));
	}
	
	// Returns list of looked up service hosts
	public static Texture2D GetPanoramaData(string localID) {
		
		Texture2D rv = new Texture2D(2, 2);
		
		string path = Marshal.PtrToStringAuto( _iOS_Gallery__CreateTempPanoFile ( localID ) );
		
		byte[] fileData = File.ReadAllBytes(path);
		
		rv.LoadImage(fileData);
		
		_iOS_Gallery__ReleaseTempPanoFile ( localID );
		
		return rv;
	}
	
}

#else

// if not iOS, dummy values are returned
public class iOSUnityInterface : MonoBehaviour {
	public int GetWidth( string _id ) {
		return 0;
	}
	public int GetHeight( string _id ) {
		return 0;
	}
	public string GetCountry( string _id ) {
		return "Unknown";
	}
	public string GetDate( string _id ) {
		return "1970:1:1 0";
	}
	public string[] GetImages() {
		string[] ret = new string[1];
		ret[0] = "Test";
		return ret;
	}
	public static Texture2D GetPanoramaData(string localID) {
		return Texture2D.whiteTexture;
	}
}

#endif
