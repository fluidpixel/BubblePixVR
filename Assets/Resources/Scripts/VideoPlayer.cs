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

	private JavaVideoInterface.Video m_Video;

	private Texture2D m_Frame;

	public bool FrameChanged {
		get { return m_FrameChanged; }
	}

	public void LoadVideo( string _path ) {
		m_Loaded = m_JVInterface.LoadVideo( _path );

		if ( m_Loaded ) {
			m_Video = m_JVInterface.GetVideoInfo;
			m_Frame = new Texture2D( m_Video.Width, m_Video.Height );
			m_NumFrames = ( m_Video.Length * 1000 ) * m_Video.FrameRate;
			PlayVideo();
		}
	}

	public Texture2D GetFrame() {
		m_FrameChanged = false;
		return m_Frame;
	}

	public void PlayVideo() {
		m_Playing = true;
		m_PanoViewer.gameObject.SetActive( true );
		//StartCoroutine( FramesFromBytes() );
		StartCoroutine(GetFrames());
	}

	public void PauseVideo() {
		m_Paused = !m_Paused;
	}

	public void StopVideo() {
		m_Playing = false;
	}

	//Fast, but expensive.
	private IEnumerator FramesFromBytes() {
		float frameTime = 1 / m_Video.FrameRate;
		float deltaTime = 0.0f;
		int frameCount = 0;

		m_Frame.LoadImage( m_Video.ByteVideo[0] );
		m_FrameChanged = true;
		while ( m_Playing && m_CurrentFrame != m_NumFrames ) {
			if ( deltaTime >= frameTime ) {
				deltaTime = 0.0f;

				m_Frame.LoadImage( m_Video.ByteVideo[frameCount] );
				m_FrameChanged = true;
				frameCount++;
			}
			deltaTime += Time.deltaTime;
			yield return null;
		}
	}

	//Low memory, but terribly slow.
	private IEnumerator GetFrames() {
		m_CurrentFrame = 0L;
		float frameStep = 1.0f / m_TargetFrameRate;
		float deltaTime = 0.0f;

		m_Frame.LoadImage( m_JVInterface.GetFrameAtTime( 1L ) );
		m_FrameChanged = true;
		while ( m_Playing && m_CurrentFrame != m_NumFrames ) {
			deltaTime += Time.deltaTime;
			if ( deltaTime >= frameStep && !m_FrameChanged ) {
				deltaTime = 0.0f;
				AdvanceFrame();

				long time = (long)( m_CurrentFrame * frameStep * 1000 );

				m_Frame.LoadImage( m_JVInterface.GetFrameAtTime( time ) ); //The approx time of the current frame in msec.
				m_FrameChanged = true;
				yield return null;
			}
			yield return null;
		}
	}

	private void AdvanceFrame() {
		//To be adapted to allow frames to decrease or increase, and at different rates. But for now, just increment frames.
		m_CurrentFrame++;
	}
}
