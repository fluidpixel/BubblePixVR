using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Gallery {
	
	/* Interface to native implementation */
	
	[DllImport ("__Internal")]
	private static extern int _iOS_Gallery__GetPanoramaCount();
	
	[DllImport ("__Internal")]
	private static extern string _iOS_Gallery__GetLocalID(int index);
	
	[DllImport ("__Internal")]
	private static extern int _iOS_Gallery__GetPanoramaWidth (string localID);
	
	[DllImport ("__Internal")]
	private static extern int _iOS_Gallery__GetPanoramaHeight (string localID);
	
	[DllImport ("__Internal")]
	private static extern string _iOS_Gallery__GetPanoramaDateTaken (string localID);
	
	[DllImport ("__Internal")]
	private static extern string _iOS_Gallery__GetPanoramaCountry (string localID);
	
	[DllImport ("__Internal")]
	private static extern void _iOS_Gallery__PanoramaToTexture (string localID, int gl_tex_id, int texWidth, int texHeight);

	/* Public interface for use inside C# / JS code */
	
	// Starts lookup for some bonjour registered service inside specified domain
	public static int PanoramaCount() {
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor)
			return _iOS_Gallery__GetPanoramaCount();
		else
			return 0;
	}
	
	// Stops lookup current lookup
	public static string GetLocalID(int index)
	{
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor)
			return _iOS_Gallery__GetLocalID (index);
		else
			return "";
	}

	// Returns list of looked up service hosts
	public static Texture2D GetPanoramaData(string localID) {
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor) {
			
			int width = _iOS_Gallery__GetPanoramaWidth (localID);
			int height = _iOS_Gallery__GetPanoramaHeight (localID);
			Texture2D tex = new Texture2D (width, height);
			
			_iOS_Gallery__PanoramaToTexture (localID, tex.GetNativeTextureID(), width, height );
			
			tex.Apply();
			
			return tex;
		}
		else {
			return Texture2D.blackTexture;
		}
	}
}