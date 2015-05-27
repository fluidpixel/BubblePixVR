using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

//Handles the importing of files external to the application, by interacting with the
//Java-Unity Interface class.

public class FileHandler : MonoBehaviour {
	public class Thumbnail {
		private Texture2D m_Thumb;
		private string m_ImageLoc;
		private int m_StartWidth;
		private int m_StartHeight;
		private string m_Country;
		private DateTime m_Date;

		public Texture2D Thumb {
			get { return m_Thumb; }
		}
		public string ImageLoc {
			get { return m_ImageLoc; }
		}

		public int Width {
			get { return m_StartWidth; }
		}

		public int Height {
			get { return m_StartHeight; }
		}

		public string Country {
			get { return m_Country; }
		}

		public DateTime Date {
			get { return m_Date; }
		}

		public string DateString {
			get {
				if ( m_Date == new DateTime( 1970, 1, 1 ) ) {
					return "";
				}
				else {
					return m_Date.ToString("d", CultureInfo.CurrentCulture);
				}
			}
		}

		public Thumbnail( Texture2D _Thumb, string _ImageLoc, string _country, string _date ) {
			m_Thumb = _Thumb;
			m_ImageLoc = _ImageLoc;
			m_StartWidth = _Thumb.width;
			m_StartHeight = _Thumb.height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime( 1970, 1, 1 );
			}
			else {
				m_Date = DateFormat.DateTime( _date );
			}
		}
		public Thumbnail( byte[] _Thumb, string _ImageLoc, int _width, int _height, int _startWidth, int _startHeight, string _date, string _country ) {
			m_Thumb = new Texture2D( _width, _height );
			m_Thumb.LoadImage( _Thumb );
			m_ImageLoc = _ImageLoc;
			m_StartWidth = _startWidth;
			m_StartHeight = _startHeight;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime(1970, 1, 1);
			}
			else { 
				m_Date = DateFormat.DateTime( _date );
			}
		}
	}

	[SerializeField]
	private JavaUnityInterface m_JUInterface;

#if UNITY_ANDROID
	private string[] m_Textures;
#endif
	private List<Thumbnail> m_Thumbs;

	public int NumThumbs {
		get { return m_Thumbs.Count; }
	}

	public Thumbnail[] GetThumbs() {
#if UNITY_EDITOR
		//Do nothing.
#elif UNITY_ANDROID
		UpdateImages();
#endif
		return m_Thumbs.ToArray();
	}

	public int TexCount {
		get { return m_Textures.Length; }
	}

	void Start() {
		m_Thumbs = new List<Thumbnail>();
#if UNITY_EDITOR
		for (int o = 0; o < 2; o++ ) {
			for ( int i = 0; i < 6; ++i ) {
				Thumbnail temp = new Thumbnail( Resources.Load( "Textures/Photosphere00" + i ) as Texture2D, "Textures/Photosphere00" + i, Country(), Date() );

				m_Thumbs.Add( temp );
			}
		}

#elif UNITY_ANDROID

		m_Textures = m_JUInterface.GetImagePaths();

		foreach ( string file in m_Textures ) {
			if ( m_JUInterface.DecodeImage( file, true, false ) )
				m_Thumbs.Add( new Thumbnail( m_JUInterface.Image, file, m_JUInterface.Width, m_JUInterface.Height, m_JUInterface.StartWidth, m_JUInterface.StartHeight, m_JUInterface.Date, m_JUInterface.Country ) );
		}
#endif
	}

	private void UpdateImages() {
		string[] tex = m_JUInterface.GetImagePaths();

		if ( tex.Length != m_Textures.Length ) {
			m_Textures = tex;
			m_Thumbs.Clear();
			foreach ( string file in m_Textures ) {
				if ( m_JUInterface.DecodeImage( file, true, true ) ) {
					m_Thumbs.Add( new Thumbnail( m_JUInterface.Image, file, m_JUInterface.Width, m_JUInterface.Height, m_JUInterface.StartWidth, m_JUInterface.StartHeight, m_JUInterface.Date, m_JUInterface.Country ) );
				}
			}
		}
	}

	public Texture2D TexFromThumb( Thumbnail _thumb ) {
#if UNITY_EDITOR

		return Resources.Load( _thumb.ImageLoc ) as Texture2D;

#elif UNITY_ANDROID

		if ( m_JUInterface.DecodeImage( _thumb.ImageLoc, false, false ) ) { 
			Texture2D tex = new Texture2D( m_JUInterface.Width, m_JUInterface.Height );
			tex.LoadImage( m_JUInterface.Image );
			return tex;
		}
		else {
			return null;
		}

#endif
	}

	private string Country() {
		string[] nations = { "", "", "United States of America", "Japan", "" };

		return nations[UnityEngine.Random.Range( 0, nations.Length - 1 )];
	}

	private string Date() {
		string[] dates = { "", "2010:6:6 272762", "2011:7:5 82727", "2012:10:12 28373", "2011:1:3 293812", "2011:3:1 827636" };

		return dates[UnityEngine.Random.Range( 0, dates.Length - 1)];
	}
}
