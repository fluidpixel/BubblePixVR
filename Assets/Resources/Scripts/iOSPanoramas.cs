using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Gallery {
	
	/* Interface to native implementation */
	
	[DllImport ("__Internal")]
	private static extern int _GetPanoramaCount();
	
	[DllImport ("__Internal")]
	private static extern string _GetLocalID(int index);
	
	[DllImport ("__Internal")]
	private static extern int _GetPanoramaWidth (string localID);
	
	[DllImport ("__Internal")]
	private static extern int _GetPanoramaHeight (string localID);
	
	[DllImport ("__Internal")]
	private static extern string _GetPanoramaDateTaken (string localID);
	
	[DllImport ("__Internal")]
	private static extern string _GetPanoramaCountry (string localID);
	
	[DllImport ("__Internal")]
	private static extern void _PanoramaToTexture (string localID, int gl_tex_id, int texWidth, int texHeight);
	
	[DllImport ("__Internal")]
	private static extern void _GalleryRefresh();
	
	/* Public interface for use inside C# / JS code */
	
	// Starts lookup for some bonjour registered service inside specified domain
	public static int PanoramaCount() {
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor)
			return _GetPanoramaCount();
		else
			return 0;
	}
	
	// Stops lookup current lookup
	public static string GetLocalID(int index)
	{
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor)
			return _GetLocalID (index);
		else
			return "";
	}
	
	// Returns current lookup status
	public static void GalleryRefresh()
	{
		if (Application.platform != RuntimePlatform.OSXEditor)
			_GalleryRefresh();
	}
	
	// Returns list of looked up service hosts
	public static Texture2D GetPanoramaData(string localID) {
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor) {
			
			int width = _GetPanoramaWidth (localID);
			int height = _GetPanoramaHeight (localID);
			Texture2D tex = new Texture2D (width, height);
			
			_PanoramaToTexture (localID, tex.GetNativeTextureID(), width, height );
			
			tex.Apply();
			
			return tex;
		}
		else {
			return Texture2D.blackTexture;
		}
	}
}