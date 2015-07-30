using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

//Handles the importing of files external to the application, by interacting with the
//Java-Unity Interface class or the IOS-Unity interface class.

public class FileHandler : MonoBehaviour {
	public class Thumbnail {
		private Texture2D m_Thumb;
		private string m_ImageLoc;
		private int m_Width;
		private int m_Height;
		private string m_Country;
		private DateTime m_Date;

		public int GetTexPtr {
			get { return m_Thumb.GetNativeTextureID(); }
		}

		public Texture2D Thumb {
			get { return m_Thumb; }
		}
		public string ImageLoc {
			get { return m_ImageLoc; }
		}

		public int Width {
			get { return m_Width; }
		}

		public int Height {
			get { return m_Height; }
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
			m_Width = _Thumb.width;
			m_Height = _Thumb.height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime( 1970, 1, 1 );
			}
			else {
				m_Date = DateFormat.DateTime( _date );
			}
		}
		public Thumbnail( byte[] _Thumb, string _ImageLoc, int _width, int _height, string _date, string _country ) {
			m_Thumb = new Texture2D( _width, _height );
			m_Thumb.LoadImage( _Thumb );
			m_ImageLoc = _ImageLoc;
			m_Width = _width;
			m_Height = _height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime(1970, 1, 1);
			}
			else { 
				m_Date = DateFormat.DateTime( _date );
			}
		}
		public Thumbnail( Texture2D _tex, string _ImageLoc, int _width, int _height, string _date, string _country ) {
			m_Thumb = _tex;
			m_Width = _width;
			m_Height = _height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime( 1970, 1, 1 );
			}
			else {
				m_Date = DateFormat.DateTime( _date );
			}
		}
	}

	#if UNITY_ANDROID && !UNITY_EDITOR

	[SerializeField]
	private JavaUnityInterface m_JUInterface;

	#elif UNITY_IOS && !UNITY_EDITOR

	[SerializeField]
	private iOSUnityInterface m_IUInterface;
	#endif

	private List<Thumbnail> m_Thumbs;

	public int NumThumbs {
		get { return m_Thumbs.Count; }
	}

	public Thumbnail[] GetThumbs() {
		UpdateImages();
		return m_Thumbs.ToArray();
	}

#if !UNITY_EDITOR
	private string[] m_Textures;

	public int TexCount {
		get { return m_Textures.Length; }
	}
#endif

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
			if ( m_JUInterface.DecodeImage( file ) )
				m_Thumbs.Add( new Thumbnail( m_JUInterface.Image, file, m_JUInterface.Width, m_JUInterface.Height, m_JUInterface.Date, m_JUInterface.Country ) );
		}
#elif UNITY_IOS
		
		m_Textures = m_IUInterface.GetImages();
		foreach (string file in m_Textures ) {
			m_Thumbs.Add( new Thumbnail( m_IUInterface.GetPanoramaData( file ), file, m_IUInterface.GetWidth( file ), m_IUInterface.GetHeight( file ), m_IUInterface.GetDate( file ), m_IUInterface.GetCountry( file ) ) );
		}
#endif
	}

	private void UpdateImages() {

#if UNITY_ANDROID && !UNITY_EDITOR
		string[] tex = m_JUInterface.GetImagePaths();

		if ( tex.Length != m_Textures.Length ) {
			m_Textures = tex;
			m_Thumbs.Clear();
			foreach ( string file in m_Textures ) {
				if ( m_JUInterface.DecodeImage( file ) ) {
					m_Thumbs.Add( new Thumbnail( m_JUInterface.Image, file, m_JUInterface.Width, m_JUInterface.Height, m_JUInterface.Date, m_JUInterface.Country ) );
				}
			}
		}
#elif UNITY_IOS && !UNITY_EDITOR
		string tex[] = m_IUInterface.GetImages();

		if (tex.Length != m_Textures.Length ) {
			m_Textures = tex;
			m_Thumbs.Clear();
			foreach (string file in m_Textures ) {
				m_Thumbs.Add( new Thumbnail( m_IUInterface.GetPanoramaData( file ), file, m_IUInterface.GetWidth( file ), m_IUInterface.GetHeight( file ), m_IUInterface.GetDate( file ), m_IUInterface.GetCountry( file ) ) );
			}
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
