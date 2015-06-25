using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class VideoPlayer : MonoBehaviour {

	#region DllImports

	[DllImport ("NativeMediaPlayer")]
	private static extern void SetTimeFromUnity(float _t);

	[DllImport ("NativeMediaPlayer")]
	private static extern void SetTexFromUnity(System.IntPtr _texPtr, int _width, int _height);

	[DllImport ("NativeMediaPlayer")]
	private static extern int PlayVideo( string _fname );

	#endregion

	public struct VideoInfo {
		int Width;
		int Height;
		int Duration;
		int FrameRate;
	};

	[SerializeField]
	private MeshRenderer m_Mesh;

	private bool m_Playing = false;
	private VideoInfo m_VideoInfo;

	/* How to video:
	 * 1.	Init Video
	 * 2.	Set texture from unity
	 * 3.	DecodeFrame()
	 * 4.	Set playing to true
	 */

	void Start() {
		StartCoroutine(WaitATick());
	}

	void Update() {
		
	}

	void OnPreRender() {
		SetTimeFromUnity( Time.timeSinceLevelLoad );
	
		GL.IssuePluginEvent( 1 );		
	}

	private IEnumerator WaitATick() {
		yield return new WaitForSeconds( 3 );
		Texture2D tex = new Texture2D( 255, 255, TextureFormat.ARGB32, false );
		tex.filterMode = FilterMode.Point;
		tex.Apply();

		m_Mesh.material.mainTexture = tex;

		SetTexFromUnity( tex.GetNativeTexturePtr(), 255, 255 );

		string hello = "/storage/emulated/0/DCIM/Camera/VID_20150528_144533.mp4";

		PlayVideo( hello );
	}
}