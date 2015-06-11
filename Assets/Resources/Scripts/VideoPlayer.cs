using UnityEngine;
using System.Collections;

public class VideoPlayer : MonoBehaviour {

	[SerializeField]
	private JavaVideoInterface m_JVInterface;

	[SerializeField]
	private int m_TargetFrameRate = 10;

	[SerializeField]
	private PanoramaViewer m_PanoViewer;

	[SerializeField]
	private TextMesh m_Text;

	private bool m_Playing = false;

	private bool Test = false;
	private System.IntPtr ptr;
	private Texture2D m_Frame = null;

	void Start() {
		m_Text.text = m_JVInterface.GetVideoPaths();
	}

	public void TextureTest() {
		m_Text.text = m_JVInterface.TestJNI();
		//Test = true;
	}

	void OnPreRender() { //Stuck the JNI call to video player in here so it's called in the render thread.
		if ( Test ) {
			Test = false;
			m_JVInterface.InitPlayer("/storage/emulated/0/DCIM/Camera/VID_20150528_144533.mp4");
		}
		if ( m_JVInterface.IsReady && m_Frame == null ) {
			m_Frame = new Texture2D( m_JVInterface.Width, m_JVInterface.Height );
			m_PanoViewer.TestPanel.material.mainTexture = m_Frame;
			m_JVInterface.PrepareVideo( m_PanoViewer.TestPanel.material.mainTexture.GetNativeTextureID() + 1 );
			ptr = m_Frame.GetNativeTexturePtr();
			PlayVideo();
		}
		//m_Text.text = m_JVInterface.IsPlaying.ToString() + " " + m_JVInterface.Width + "x" + m_JVInterface.Height; 
	}

	void OnPostRender() {
		if ( m_JVInterface.IsPlaying ) {
			m_Frame.UpdateExternalTexture( ptr );
			GL.InvalidateState();
		}
	}

	public void PlayVideo() {
		m_Playing = true;
		m_PanoViewer.gameObject.SetActive( true );
		//m_JVInterface.PlayVideo();
	}
}