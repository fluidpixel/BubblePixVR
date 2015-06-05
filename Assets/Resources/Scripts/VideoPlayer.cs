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

	private long m_CurrentFrame;
	private long m_NumFrames;

	private bool m_Loaded;
	private bool m_FrameChanged = false;
	private bool m_Playing = false;
	private bool m_Paused = false;

	private bool Test = false;

	private Texture2D m_Frame;

	public bool FrameChanged {
		get { return m_FrameChanged; }
	}

	public void TextureTest() {
		Test = true;
	}

	void OnPreRender() {
		if ( Test ) {
			m_Frame = new Texture2D( m_JVInterface.Width, m_JVInterface.Height );
			m_PanoViewer.TestPanel.material.mainTexture = m_Frame;
			m_JVInterface.PrepareVideo( "/storage/emulated/0/DCIM/Camera/VID_20150528_144533.mp4", m_PanoViewer.TestPanel.material.mainTexture.GetNativeTextureID() );
			PlayVideo();
		}
	}

	public void PlayVideo() {
		m_Playing = true;
		m_PanoViewer.gameObject.SetActive( true );
		m_JVInterface.PlayVideo();
	}
}