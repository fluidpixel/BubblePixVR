using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class VideoPlayer : MonoBehaviour {

	#region DllImports

	[DllImport( "NativeMediaPlayer" )]
	private static extern void SetTimeFromUnity( float _t );

	[DllImport( "NativeMediaPlayer" )]
	private static extern void SetTextureFromUnity( System.IntPtr _texPtr );

	[DllImport( "NativeMediaPlayer" )]
	private static extern int InitNativeVideo( char[] _fName );

	[DllImport( "NativeMediaPlayer" )]
	private static extern void DestroyPlayer();

	[DllImport( "NativeMediaPlayer" )]
	private static extern int Width();

	[DllImport( "NativeMediaPlayer" )]
	private static extern int Height();

	[DllImport( "NativeMediaPlayer" )]
	private static extern int Duration();

	[DllImport( "NativeMediaPlayer" )]
	private static extern int FrameRate();

	[DllImport( "NativeMediaPlayer" )]
	private static extern int DecodeFrame();

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
	 * 
	 */

	void Start() {
		//char[] str = ("/storage/emulated/0/DCIM/Camera/VID_20150528_144533.mp4").ToCharArray();
		//int res = InitNativeVideo( str );
		//if ( res != 0 ) {
		//	Debug.Log("Error with initialising native video plugin. Err Code: " + res.ToString());
		//}

		Texture2D tex = new Texture2D(255, 255, TextureFormat.ARGB32, false);
		tex.filterMode = FilterMode.Point;
		tex.Apply();

		m_Mesh.material.mainTexture = tex;

		SetTextureFromUnity(tex.GetNativeTexturePtr());
		//DecodeFrame();
		m_Playing = true;
		StartCoroutine(CallPluginAtEOF());
	}

	private IEnumerator CallPluginAtEOF() {
		while ( m_Playing ) {
			yield return new WaitForEndOfFrame();

			SetTimeFromUnity(Time.timeSinceLevelLoad);

			GL.IssuePluginEvent(1);
		}
	}

}