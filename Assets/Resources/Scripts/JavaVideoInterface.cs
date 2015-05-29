using UnityEngine;
using System.Collections;

public class JavaVideoInterface : MonoBehaviour {

	public struct Video {
		private int m_Width;
		private int m_Height;
		private long m_Length;
		private string m_Name;
		private int m_FrameRate;
		private byte[][] m_ByteVideo;

		public Video( int _w, int _h, long _l, string _s, byte[][] _b, int _f ) {
			m_Width = _w;
			m_Height = _h;
			m_Length = _l;
			m_Name = _s;
			m_ByteVideo = _b;
			m_FrameRate = _f;
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
		public int FrameRate {
			get { return m_FrameRate; }
		}
		public byte[][] ByteVideo {
			get { return m_ByteVideo; }
		}
	}

	private AndroidJavaObject m_VideoPlayer = null;
	private Video m_VideoInfo;
	private int m_FrameRate = 7;

	public Video GetVideoInfo {
		get { return m_VideoInfo; }
	}

	public bool LoadVideo( string _path ) {
		m_VideoPlayer = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.VideoHandler", _path );
		long length = m_VideoPlayer.Call<long>( "Length" );
		if ( length != -1 ) {
			m_VideoInfo = new Video( m_VideoPlayer.Call<int>( "Width" ), m_VideoPlayer.Call<int>( "Height" ), m_VideoPlayer.Call<long>( "Length" ), m_VideoPlayer.Call<string>( "FileName" ), m_VideoPlayer.Call<byte[][]>( "GetFrames", m_FrameRate ), m_FrameRate );
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
