using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ThumbBrowser : MonoBehaviour {

	private enum SortingType {
		None = 0,
		Country = 1,
		Date = 2
	};

	[SerializeField]
	private AppController m_AppController;

	[SerializeField]
	private ThumbTile m_TilePrefab;

	[SerializeField]
	private MeshRenderer m_LeftScroll;

	[SerializeField]
	private MeshRenderer m_RightScroll;

	[SerializeField]
	private MeshRenderer m_VideoToggle;

	[SerializeField]
	private MeshRenderer m_SortButton;

	[SerializeField]
	private MeshRenderer m_CalendarIcon;

	[SerializeField]
	private MeshRenderer m_GlobeIcon;

	[SerializeField]
	private GameObject m_ThumbAnchor;

	[SerializeField]
	private TextPanel m_TextPanelPrefab;

	[SerializeField]
	private ColumnAnchor m_ColumnAnchorPrefab;

	private List<ThumbTile> m_Thumbs = new List<ThumbTile>();
	private List<string> m_Countries = new List<string>();
	private List<DateTime> m_Dates = new List<DateTime>();
	private List<TextPanel> m_TextPanels = new List<TextPanel>();
	private List<ColumnAnchor> m_ColumnAnchors = new List<ColumnAnchor>();

	private int m_ActiveColumn;
	private float xSpacing = 4.0f;
	private float ySpacing = 1.0f;
	private SortingType m_Sorting = SortingType.None;
	private bool moveLeft = false;
	private bool moveRight = false;
	private bool moveUp = false;
	private bool moveDown = false;
	private bool imageMode = true;
	private bool m_Ascending = true;
	private Color m_ScrollColor = new Color( 0.44f, 0.48f, 0.62f, 0.27f );
	private Color m_ScrollColorHover = new Color( 0.44f, 0.48f, 0.62f, 0.54f );
	private Color m_ActiveButtonColor = new Color( 0.32f, 0.7f, 0.4f, 0.27f );
	private Color m_ActiveButtonColorHover = new Color( 0.32f, 0.7f, 0.4f, 0.54f );
	private float m_MaxVelocity = 6.0f;
	private float m_MaxAcceleration = 4.0f;
	private float m_xAcceleration = 0.0f;
	private float m_AccInc = 1.5f;
	private float m_xVelocity = 0.0f;
	private float m_MinVelocity = 0.1f;
	private Texture2D m_PhotoTex, m_VideoTex;

	void Start() {
		m_VideoToggle.material.color = m_ScrollColor;
		m_PhotoTex = Resources.Load( "Textures/Picture" ) as Texture2D;
		m_VideoTex = Resources.Load( "Textures/Video" ) as Texture2D;
		m_VideoToggle.material.SetTexture( "_BorderTex", m_PhotoTex );
	}

	void Update() {
		//Broken swipe input.
		if ( m_AppController.TC.SwipeDirection == TouchController.SwipeTypes.Right ) {
			m_xVelocity += m_AppController.TC.SwipeSpeed / 300.0f;
		}
		else if ( m_AppController.TC.SwipeDirection == TouchController.SwipeTypes.Left ) {
			m_xVelocity -= m_AppController.TC.SwipeSpeed / 300.0f;
		}

		#if UNITY_EDITOR

		moveLeft	= Input.GetKey( KeyCode.LeftArrow );
		moveRight	= Input.GetKey( KeyCode.RightArrow );
		moveUp		= Input.GetKey( KeyCode.UpArrow );
		moveDown	= Input.GetKey( KeyCode.DownArrow );

		#endif

		if ( m_Thumbs.Count > 1 ) {

			ApplyAcceleration();

			IntegrateXVelocity( ShouldMove() );

			if ( m_Sorting != SortingType.None ) { 
				GetActiveColumn();
				if ( m_ActiveColumn != -1 ) {

					if ( moveLeft || moveRight ) {
						m_ColumnAnchors[m_ActiveColumn].Velocity = m_ColumnAnchors[m_ActiveColumn].Acceleration = 0.0f;
					}

					ApplyColumnAcceleration();
					IntegrateColumnVelocity( ShouldMove() );
				}
			}
		}

		if ( Input.GetKeyDown( KeyCode.Space ) ) {
			SortButtonClicked();
		}
		if ( Input.GetKeyDown( KeyCode.Alpha1 ) ) {
			GlobeButtonClicked();
		}
		if ( Input.GetKeyDown( KeyCode.Alpha2 ) ) {
			CalendarButtonClicked();
		}
	}

	public void ViewBrowser() {
		this.gameObject.SetActive( true );
		StartCoroutine( MoveBrowser( false ) );
		PopThumbs();
	}

	public void ReturnToBrowser() {
		this.gameObject.SetActive( true );
	}

	public void RefreshBrowser() {
		foreach ( ThumbTile thumb in m_Thumbs ) {
			DestroyObject( thumb.gameObject );
		}
		if ( m_TextPanels.Count > 0 ) {
			foreach ( TextPanel tp in m_TextPanels ) {
				DestroyObject( tp.gameObject );
			}
		}
		m_Countries.Clear();
		m_Dates.Clear();
		m_TextPanels.Clear();
		m_Thumbs.Clear();
		PopThumbs();
	}

	public void ViewImage() {
		this.gameObject.SetActive( false );
	}

	//Many hover/nohover/clicked methods for buttons.
	public void LeftTrigHover() {
		m_LeftScroll.material.color = m_ScrollColorHover;
		moveLeft = true;
	}
	public void RightTrigHover() {
		m_RightScroll.material.color = m_ScrollColorHover;
		moveRight = true;
	}
	public void LeftTrigNoHover() {
		m_LeftScroll.material.color = m_ScrollColor;
		moveLeft = false;
	}
	public void RightTrigNoHover() {
		m_RightScroll.material.color = m_ScrollColor;
		moveRight = false;
	}
	public void PVToggleHover() {
		m_VideoToggle.material.color = m_ScrollColorHover;
	}
	public void PVToggleNoHover() {
		m_VideoToggle.material.color = m_ScrollColor;
	}
	public void PVToggleClicked() {
		if ( imageMode ) {
			m_VideoToggle.material.SetTexture( "_BorderTex", m_VideoTex );
		}
		else {
			m_VideoToggle.material.SetTexture( "_BorderTex", m_PhotoTex );
		}
		imageMode = !imageMode;
	}
	public void SortButtonHover() {
		m_SortButton.material.color = m_ScrollColorHover;
	}
	public void SortButtonNoHover() {
		m_SortButton.material.color = m_ScrollColor;
	}
	public void SortButtonClicked() {
		m_Ascending = !m_Ascending;

		if ( m_Ascending ) { 
			m_SortButton.material.SetTextureScale( "_BorderTex", new Vector2( 1, 1 ) );
		}
		else {
			m_SortButton.material.SetTextureScale( "_BorderTex", new Vector2( 1, -1 ) );
		}
		if ( m_Sorting == SortingType.Date ) {
			SortByDate();
		}
		else if ( m_Sorting == SortingType.Country ) {
			SortByCountry();
		}
	}
	public void GlobeButtonHover() {
		if ( m_Sorting == SortingType.Country ) {
			m_GlobeIcon.material.color = m_ActiveButtonColorHover;
		}
		else {
			m_GlobeIcon.material.color = m_ScrollColorHover;
		}
	}
	public void GlobeButtonNoHover() {
		if ( m_Sorting == SortingType.Country ) {
			m_GlobeIcon.material.color = m_ActiveButtonColor;
		}
		else {
			m_GlobeIcon.material.color = m_ScrollColor;
		}
	}
	public void GlobeButtonClicked() {
		if ( m_Sorting == SortingType.Country ) {
			RefreshBrowser();
			ChangeSorting( SortingType.None );
		}
		else {
			SortByCountryColumn();
			ChangeSorting( SortingType.Country );
		}
	}
	public void CalendarButtonHover() {
		if ( m_Sorting == SortingType.Date ) {
			m_CalendarIcon.material.color = m_ActiveButtonColorHover;
		}
		else {
			m_CalendarIcon.material.color = m_ScrollColorHover;
		}
	}
	public void CalendarButtonNoHover() {
		if ( m_Sorting == SortingType.Date ) {
			m_CalendarIcon.material.color = m_ActiveButtonColor;
		}
		else {
			m_CalendarIcon.material.color = m_ScrollColor;
		}
	}
	public void CalendarButtonClicked() {
		if ( m_Sorting == SortingType.Date ) {
			RefreshBrowser();
			ChangeSorting( SortingType.None );
		}
		else {
			SortByDate();
			ChangeSorting( SortingType.Date );
		}
	}

	//Private methods, mostly stuff that gets reused.
	private IEnumerator MoveBrowser( bool _away ) {
		Vector3 target;
		if ( _away ) {
			target = Vector3.zero;
		}
		else {
			target = new Vector3( 0.0f, 1.78f, -8.2f );
		}

		float diff = Vector3.Distance( target, this.gameObject.transform.position ) * 0.01f;

		while ( Vector3.Distance( target, this.gameObject.transform.position ) > diff ) {

			this.gameObject.transform.position = Vector3.Slerp( this.gameObject.transform.position, target, 2.0f * Time.deltaTime );
			yield return null;
		}
		this.gameObject.transform.position = target;

		yield return null;
	}

	private void AddCountry( string _country ) {
		bool exists = false;

		foreach ( string co in m_Countries ) {
			if ( co == _country ) {
				exists = true;
				break;
			}
		}
		if ( !exists )
			m_Countries.Add( _country );
	}

	private void AddDate( DateTime _date ) {
		bool exists = false;

		foreach ( DateTime date in m_Dates ) {
			if ( date == _date ) {
				exists = true;
				break;
			}
		}
		if ( !exists ) {
			m_Dates.Add( _date );
		}
	}

	private void SortDates() {
		Vector3 pos = m_ThumbAnchor.transform.localPosition;
		pos.x = 0.0f;
		m_ThumbAnchor.transform.localPosition = pos;
		if ( m_Ascending ) {
			m_Dates.Sort( ( a, b ) => a.CompareTo( b ) );
		}
		else {
			m_Dates.Sort( ( a, b ) => b.CompareTo( a ) );
		}
	}

	private void SortCountries() {
		Vector3 pos = m_ThumbAnchor.transform.localPosition;
		pos.x = 0.0f;
		m_ThumbAnchor.transform.localPosition = pos;
		if ( m_Ascending ) {
			m_Countries.Sort( ( a, b ) => a.CompareTo( b ) );
		}
		else {
			m_Countries.Sort( ( a, b ) => b.CompareTo( a ) );
		}
	}

	private void PopThumbs() {
		FileHandler.Thumbnail[] thumbs = m_AppController.FH.GetThumbs();
		float x = 0;
		float y = ySpacing;
		for ( int i = 0; i < thumbs.Length; ++i ) {
			ThumbTile temp = Instantiate( m_TilePrefab ) as ThumbTile;
			temp.transform.parent = m_ThumbAnchor.gameObject.transform;
			Vector3 tPos = Vector3.zero;
			tPos.x = x;
			tPos.y = y;
			AddCountry( thumbs[i].Country );
			AddDate( thumbs[i].Date );
			temp.SetThumb( thumbs[i] );
			temp.SetPos( tPos );
			m_Thumbs.Add( temp );
			if ( y == -ySpacing ) {
				x += xSpacing;
				y = ySpacing;
			}
			else {
				y -= ySpacing;
			}
		}
	}

	private void SortByCountryColumn() {
		m_xVelocity = m_xAcceleration = 0.0f; //Stop things moving.
		if ( m_TextPanels.Count > 0 ) { //Get rid of any existing panels
			foreach ( TextPanel tp in m_TextPanels ) {
				DestroyObject( tp.gameObject );
			}
			m_TextPanels.Clear();
		}

		SortCountries();
		float x = xSpacing;
		float y = ySpacing * 2 - 0.1f;
		int row = 0;
		int column = 0;

		List<ThumbTile> temp = new List<ThumbTile>();
		List<ColumnAnchor> tempAnchs = new List<ColumnAnchor>();

		foreach ( string str in m_Countries ) {
			TextPanel tempTx = Instantiate( m_TextPanelPrefab ) as TextPanel; //First a title panel is made to denote the country
			Vector3 pos = new Vector3( x * column, y, 0.0f );

			tempTx.transform.parent = m_ThumbAnchor.transform;
			tempTx.SetPos( pos );

			tempTx.SetText( str == "" ? "Unknown" : str );
			m_TextPanels.Add( tempTx );

			ColumnAnchor anchor = Instantiate(m_ColumnAnchorPrefab) as ColumnAnchor;
			anchor.transform.parent = m_ThumbAnchor.transform;
			anchor.LocalPos = new Vector3( x * column, -(row * ySpacing) + ySpacing, 0.0f );
			
			for ( int i = 0; i < m_Thumbs.Count; ++i ) {
				if ( m_Thumbs[i].Image.Country == str ) {
					ThumbTile tempTile = Instantiate( m_TilePrefab ) as ThumbTile;
					tempTile.transform.parent = anchor.transform;
					tempTile.SetThumb( m_Thumbs[i].Image );
					Vector3 tPos = new Vector3( 0.0f, -(row * ySpacing), 0.0f );

					tempTile.SetPos(tPos);
					temp.Add( tempTile );
					
					row++;
				}
			}
			anchor.BottomPoint = -( row * ySpacing ) + ySpacing;
			tempAnchs.Add( anchor );
			column++;
			row = 0;
		}

		foreach ( ThumbTile thumb in m_Thumbs ) {
			DestroyObject( thumb.gameObject );
		}
		m_Thumbs.Clear();
		m_Thumbs = temp;

		if ( m_ColumnAnchors.Count > 0 ) {
			foreach ( ColumnAnchor an in m_ColumnAnchors ) {
				DestroyObject( an.gameObject );
			}
		}
		m_ColumnAnchors.Clear();
		m_ColumnAnchors = tempAnchs;
	}

	private void SortByCountry() {
		m_xVelocity = m_xAcceleration = 0.0f; //Stop things moving.
		if ( m_TextPanels.Count > 0 ) { //Get rid of any existing panels
			foreach ( TextPanel tp in m_TextPanels ) {
				DestroyObject( tp.gameObject );
			}
			m_TextPanels.Clear();
		}

		SortCountries();
		float x = xSpacing;
		float y = 0.0f;
		int row = 0;
		int column = 0;

		List<ThumbTile> temp = new List<ThumbTile>();

		foreach ( string str in m_Countries ) {
			TextPanel tempTx = Instantiate( m_TextPanelPrefab ) as TextPanel; //First a title panel is made to denote the country
			y = 2.7f;
			Vector3 pos = new Vector3( x * column, y, 0.0f );

			tempTx.transform.parent = m_ThumbAnchor.gameObject.transform;
			tempTx.SetPos( pos );

			tempTx.SetText( str == "" ? "Unknown" : str );
			m_TextPanels.Add( tempTx );

			for ( int i = 0; i < m_Thumbs.Count; ++i ) {
				if ( row == 0 ) {
					y = ySpacing;
				}
				else if ( row == 1 ) {
					y = 0.0f;
				}
				else {
					y = -ySpacing;
				}

				if ( m_Thumbs[i].Image.Country == str ) {
					ThumbTile tempTile = Instantiate( m_TilePrefab ) as ThumbTile;
					tempTile.transform.parent = m_ThumbAnchor.gameObject.transform;
					tempTile.SetThumb( m_Thumbs[i].Image );
					Vector3 tPos = new Vector3( x * column, y, 0.0f );

					tempTile.SetPos( tPos );
					temp.Add( tempTile );
					if ( row > 1 ) {
						row = 0;
						column++;
					}
					else {
						row++;
					}
				}
			}
			if ( row != 0 ) {
				column++;
			}
			row = 0;
		}

		foreach ( ThumbTile thumb in m_Thumbs ) {
			DestroyObject( thumb.gameObject );
		}
		m_Thumbs.Clear();
		m_Thumbs = temp;
	}

	private void SortByDate() {
		m_xVelocity = m_xAcceleration = 0.0f; //Stop things moving.
		if ( m_TextPanels.Count > 0 ) {
			foreach ( TextPanel tp in m_TextPanels ) {
				DestroyObject( tp.gameObject );
			}
			m_TextPanels.Clear();
		}

		ChangeSorting( SortingType.Date );
		SortDates();
		float x = xSpacing;
		float y = 0.0f;
		int row = 0;
		int column = 0;

		List<ThumbTile> temp = new List<ThumbTile>();

		foreach ( DateTime dt in m_Dates ) {
			TextPanel tempTx = Instantiate( m_TextPanelPrefab ) as TextPanel; //First a title panel is made to denote the date
			y = 2.7f;
			Vector3 pos = new Vector3( x * column, y, 0.0f );

			tempTx.transform.parent = m_ThumbAnchor.gameObject.transform;
			tempTx.SetPos( pos );

			if ( dt == new DateTime( 1970, 1, 1 ) ) {
				tempTx.SetText( "Unknown" );
			}
			else {
				tempTx.SetText( DateFormat.WrittenDate( dt ) );
			}
			m_TextPanels.Add( tempTx );

			for ( int i = 0; i < m_Thumbs.Count; ++i ) {
				if ( row == 0 ) {
					y = ySpacing;
				}
				else if ( row == 1 ) {
					y = 0.0f;
				}
				else {
					y = -ySpacing;
				}

				if ( m_Thumbs[i].Image.Date == dt ) {
					ThumbTile tempTile = Instantiate( m_TilePrefab ) as ThumbTile;
					tempTile.transform.parent = m_ThumbAnchor.gameObject.transform;
					tempTile.SetThumb( m_Thumbs[i].Image );
					Vector3 tPos = new Vector3( x * column, y, 0.0f );

					tempTile.SetPos( tPos );
					temp.Add( tempTile );
					if ( row > 1 ) {
						row = 0;
						column++;
					}
					else {
						row++;
					}
				}
			}
			if ( row != 0 ) {
				column++;
			}
			row = 0;
		}

		foreach ( ThumbTile thumb in m_Thumbs ) {
			DestroyObject( thumb.gameObject );
		}
		m_Thumbs.Clear();
		m_Thumbs = temp;
	}

	private void ChangeSorting( SortingType _arg ) {
		m_Sorting = _arg;
		if ( m_Sorting == SortingType.None ) {
			m_GlobeIcon.material.color = m_ScrollColor;
			m_CalendarIcon.material.color = m_ScrollColor;
		}
		else if ( m_Sorting == SortingType.Date ) {
			m_GlobeIcon.material.color = m_ScrollColor;
			m_CalendarIcon.material.color = m_ActiveButtonColor;
		}
		else {
			m_GlobeIcon.material.color = m_ActiveButtonColor;
			m_CalendarIcon.material.color = m_ScrollColor;
		}
	}

	private void GetActiveColumn() {
		m_ActiveColumn = -1;
		for ( int i = 0; i < m_ColumnAnchors.Count; i++ ) {
			if ( m_ColumnAnchors[i].Pos.x > -2.0f && m_ColumnAnchors[i].Pos.x < 2.0f ) {
				m_ActiveColumn = i;
				break;
			}
		}
	}

	private bool ShouldMove() { //Does the user want the carousel to move?
		if ( moveLeft || moveRight || m_AppController.TC.SwipeDirection != TouchController.SwipeTypes.None || ( m_Sorting != SortingType.None && ( moveDown || moveUp ) ) ) {
			return true;
		}
		else {
			return false;
		}
	}

	private void ApplyAcceleration() { //Will apply an acceleration to the carousel/active column in the desired direction.
		//Forces the carousel to immediately change direction when it's asked to move in the opposite direciton to it's current.
		if ( ( moveLeft || m_AppController.TC.SwipeDirection == TouchController.SwipeTypes.Right ) && m_xAcceleration < m_MaxAcceleration ) {
			m_xAcceleration += m_AccInc;
			if ( m_xVelocity < 0.0f ) {
				m_xVelocity = 0.5f;
			}
		}
		else if ( ( moveRight || m_AppController.TC.SwipeDirection == TouchController.SwipeTypes.Left ) && m_xAcceleration > -m_MaxAcceleration ) {
			m_xAcceleration -= m_AccInc;
			if ( m_xVelocity > 0.0f ) {
				m_xVelocity = -0.5f;
			}
		}
		else {
			m_xAcceleration = 0.0f;
		}
	}

	private void ApplyColumnAcceleration() {
		if ( moveUp && m_ColumnAnchors[m_ActiveColumn].Acceleration < m_MaxAcceleration ) {
			m_ColumnAnchors[m_ActiveColumn].Acceleration += m_AccInc;
			if ( m_ColumnAnchors[m_ActiveColumn].Velocity < 0.0f ) {
				m_ColumnAnchors[m_ActiveColumn].Velocity = 0.5f;
			}
		}
		else if ( moveDown && m_ColumnAnchors[m_ActiveColumn].Acceleration > -m_MaxAcceleration ) {
			m_ColumnAnchors[m_ActiveColumn].Acceleration -= m_AccInc;
			if ( m_ColumnAnchors[m_ActiveColumn].Velocity > 0.0f ) {
				m_ColumnAnchors[m_ActiveColumn].Velocity = -0.5f;
			}
		}
		else {
			m_ColumnAnchors[m_ActiveColumn].Acceleration = 0.0f;
		}
	}

	private void IntegrateXVelocity( bool _moving ) {
		Vector3 pos = m_ThumbAnchor.transform.position;
		
		//Works out the velocity of the carousel from its acceleration and last frame's velocity (V = pV + a * t).
		//This only applies a damping force when the user stops scrolling.
		m_xVelocity = ( m_xVelocity + m_xAcceleration * Time.deltaTime ) * ( _moving ? 1.0f : 0.97f );
		
		//Keep it from scrolling faster than a certain rate.
		if ( m_xVelocity > m_MaxVelocity ) {
			m_xVelocity = m_MaxVelocity;
		}
		else if ( m_xVelocity < -m_MaxVelocity ) {
			m_xVelocity = -m_MaxVelocity;
		}

		//Handles reaching the end of the list, by inverting the velocity (and damping), making it appear to bounce.
		if ( m_Thumbs[0] != null ) {
			if ( ( m_Thumbs[0].transform.position.x > 0.0f && m_xVelocity > 0.0f ) ||
			( m_Thumbs[m_Thumbs.Count - 1].transform.position.x < 0.0f && m_xVelocity < 0.0f ) ) {//Has it scrolled to the leftmost/rightmost thumb, and is it still trying to go further?
				m_xVelocity = -m_xVelocity * 0.5f; //if so, bounce.
			}
		}

		//If it's really slow, just stop.
		if ( Mathf.Abs( m_xVelocity ) <= m_MinVelocity && !_moving ) {
			m_xVelocity = 0.0f;
		}
		//Assign the new position of the carousel (d = V * t)
		pos.x = m_ThumbAnchor.transform.position.x + m_xVelocity * Time.deltaTime;
		m_ThumbAnchor.transform.position = pos;		
	}

	private void IntegrateColumnVelocity( bool _moving ) { //Same as IntegrateXVelocity, just for the active column's vertical motion.
		float yVelocity;
		//Calculate the velocity of the column and apply damping if the user isn't trying to move it.
		yVelocity = ( m_ColumnAnchors[m_ActiveColumn].Velocity + m_ColumnAnchors[m_ActiveColumn].Acceleration * Time.deltaTime ) * ( _moving ? 1.0f : 0.97f );

		//Keep it from scrolling faster than a certain rate.
		if ( yVelocity > m_MaxVelocity ) {
			yVelocity = m_MaxVelocity;
		}
		else if ( yVelocity < -m_MaxVelocity ) {
			yVelocity = -m_MaxVelocity;
		}

		//Inverts the velocity (with damping) if it has scrolled to the top or bottom.
		//if ( (m_ColumnAnchors[m_ActiveColumn].LocalPos.y + m_ColumnAnchors[m_ActiveColumn].BottomPoint > 0.0f && yVelocity > 0.0f ) ||
		//( m_ColumnAnchors[m_ActiveColumn].LocalPos.y > ySpacing * 2.0f && yVelocity < 0.0f ) ) { //Has it traveled as high or as low as it can, and is it still trying to go further?
		//	yVelocity = -yVelocity * 0.5f; //if so, bounce
		//}

		//If its really slow, stop it.
		if ( Mathf.Abs( yVelocity ) <= m_MinVelocity && !_moving ) {
			yVelocity = 0.0f;
		}

		//Assign the new position of the column.
		float yPos = m_ColumnAnchors[m_ActiveColumn].LocalPos.y + yVelocity * Time.deltaTime;
		m_ColumnAnchors[m_ActiveColumn].LocalY = yPos;
		m_ColumnAnchors[m_ActiveColumn].Velocity = yVelocity;
	}
}