using UnityEngine;
using System.Collections;

public class JavaVideoInterface : MonoBehaviour {

	public struct VideoInfo {
		private int m_Width;
		private int m_Height;
		private long m_Length;
		private string m_Name;

		public VideoInfo( int _w, int _h, long _l, string _s ) {
			m_Width = _w;
			m_Height = _h;
			m_Length = _l;
			m_Name = _s;
		}

		public int Width {
			get { return m_Width; }
		}
		public int Height {
			get { return m_Height; }
		}
		public long Length {
			get { return m_Length; }
		}
		public string Name {
			get { return m_Name; }
		}
	}

	private AndroidJavaObject m_VideoPlayer = null;
	private VideoInfo m_VideoInfo;

	public VideoInfo GetVideoInfo {
		get { return m_VideoInfo; }
	}

	public bool LoadVideo( string _path ) {
		m_VideoPlayer = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.VideoHandler", _path );
		long length = m_VideoPlayer.Call<long>( "Length" );
		if ( length != -1 ) {
			m_VideoInfo = new VideoInfo( m_VideoPlayer.Call<int>("Width"), m_VideoPlayer.Call<int>("Height"), m_VideoPlayer.Call<long>("Length"), m_VideoPlayer.Call<string>("FileName") );
			return true;
		}
		else {
			return false;
		}
	}

	public void DestroyVideo() {
		m_VideoPlayer.Dispose();
		m_VideoPlayer = null;
	}

	public byte[] GetFrameAtTime( long _msec ) {
		byte[] frame = m_VideoPlayer.Call<byte[]>( "GetFrameAtTime", _msec );
		return frame;
	}
}
