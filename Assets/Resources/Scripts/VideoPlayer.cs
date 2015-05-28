using UnityEngine;
using System.Collections;

public class VideoPlayer : MonoBehaviour {

	[SerializeField]
	private JavaVideoInterface m_JVInterface;

	[SerializeField]
	private int m_TargetFrameRate = 30;

	[SerializeField]
	private TextMesh m_Text;

	//private float m_Time;

	private string m_FileName;

	private long m_VideoLength;
	private long m_CurrentFrame;
	private long m_NumFrames;

	private int m_Width;
	private int m_Height;
	
	private bool m_Loaded;
	private bool m_FrameChanged = false;
	private bool m_Playing = false;
	private bool m_Paused = false;

	private Texture2D m_Frame;

	public bool FrameChanged {
		get { return m_FrameChanged; }
	}

	public void LoadVideo( string _path ) {
		m_Loaded = m_JVInterface.LoadVideo( _path );
		
		if ( m_Loaded ) {
			m_Width = m_JVInterface.GetVideoInfo.Width;
			m_Height = m_JVInterface.GetVideoInfo.Height;
			m_VideoLength = m_JVInterface.GetVideoInfo.Length;
			m_NumFrames = m_TargetFrameRate * m_VideoLength;
			m_FileName = m_JVInterface.GetVideoInfo.Name;
			m_Frame = new Texture2D( m_Width, m_Height );

			m_Text.text = m_Width.ToString() + " " + m_Height.ToString() + " " + m_VideoLength.ToString() + " " + m_FileName;

			PlayVideo();
		}
	}

	public Texture2D GetFrame() {
		m_FrameChanged = false;
		return m_Frame;
	}

	public void PlayVideo() {
		m_Playing = true;
		StartCoroutine(GetFrames());
	}

	public void PauseVideo() {
		m_Paused = !m_Paused;
	}

	public void StopVideo() {
		m_Playing = false;
	}

	private IEnumerator GetFrames() {
		//m_Time = 0.0f;
		m_CurrentFrame = 0L;
		float frameStep = 1.0f / m_TargetFrameRate;
		float deltaTime = 0.0f;

		m_Frame.LoadImage( m_JVInterface.GetFrameAtTime( 0L ) );
		m_FrameChanged = true;
		while ( m_Playing && m_CurrentFrame != m_NumFrames ) {
			deltaTime += Time.deltaTime;
			if ( deltaTime >= frameStep && !m_FrameChanged ) {
				AdvanceFrame();
				m_Frame.LoadImage( m_JVInterface.GetFrameAtTime( (long)(m_CurrentFrame * frameStep) * 1000L ) ); //The approx time of the current frame in msec.
				m_FrameChanged = true;
				yield return null;
			}

		}
	}

	private void AdvanceFrame() {
		//To be adapted to allow frames to decrease or increase, and at different rates. But for now, just increment frames.
		m_CurrentFrame++;
	}
}
