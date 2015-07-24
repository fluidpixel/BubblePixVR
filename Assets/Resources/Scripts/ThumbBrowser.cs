using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/* Controls the browser component of the application.
 * Specifically, handles:
 * - Sorting of thumbnails.
 * - Button-based methods (clicked/selected/deselected)
 * - Scrolling and switching between sorting methods.
 */

public class ThumbBrowser : MonoBehaviour {

#region Variable Declarations

	#region Variables - Editor

	[SerializeField]
	private AppController m_AppController;

	[SerializeField]
	private ThumbTile m_TilePrefab;

	[SerializeField]
	private UIButton m_LeftScroll;

	[SerializeField]
	private UIButton m_RightScroll;

	[SerializeField]
	private UIButton m_TopScroll;

	[SerializeField]
	private UIButton m_BotScroll;

	[SerializeField]
	private UIButton m_SortButton;

	[SerializeField]
	private UIButton m_VRModeButton;

	[SerializeField]
	private UIButton m_GlobeButton;

	[SerializeField]
	private UIButton m_CalendarButton;

	[SerializeField]
	private UIButton m_SettingsButton;

	[SerializeField]
	private Carousel m_ThumbAnchor;

	[SerializeField]
	private TextPanel m_TextPanelPrefab;

	[SerializeField]
	private ColumnAnchor m_ColumnAnchorPrefab;

	#endregion

	#region Variables - Private

	private enum SortingType {
		None = 0,
		Country = 1,
		Date = 2
	};

	private bool				moveLeft					= false;
	private bool				moveRight					= false;
	private bool				moveUp						= false;
	private bool				moveDown					= false;
	private bool				imageMode					= true;
	private bool				m_Ascending					= true;
	private bool				m_Sweeping					= false;
	private int					m_ActiveColumn;
	private float				xSpacing					= 4.0f;
	private float				ySpacing					= 1.0f;
	private float				m_MaxVelocity				= 10.0f;
	private float				m_MaxAcceleration			= 4.0f;
	private float				m_xAcceleration				= 0.0f;
	private float				m_AccInc					= 1.5f;
	private float				m_xVelocity					= 0.0f;
	private float				m_MinVelocity				= 0.1f;
	private float				m_GlobeButtonPosx			= -1.8f;
	private float				m_CalendarButtonPosx		= -3.34f;
	private float				m_SortOrderPosx				= 4.9f;
	private float				m_LeftScrollPosx			= -1.5f;
	private float				m_RightScrollPosx			= 1.5f;
	private float				m_SettingsPosx				= 7.19f;
	private float				m_VRBtnPosx					= -4.88f;
	private List<ThumbTile>		m_Thumbs					= new List<ThumbTile>();
	private List<string>		m_Countries					= new List<string>();
	private List<DateTime>		m_Dates						= new List<DateTime>();
	private List<TextPanel>		m_TextPanels				= new List<TextPanel>();
	private List<ColumnAnchor>	m_ColumnAnchors				= new List<ColumnAnchor>();
	private SortingType			m_Sorting					= SortingType.None;

	public bool Sweeping {
		get { return m_Sweeping; }
	}

	#endregion

#endregion

#region MonoBehaviour Overrides

	void Awake() {
		//m_PhotoTex = Resources.Load( "Textures/Picture" ) as Texture2D;
		//m_VideoTex = Resources.Load( "Textures/Video" ) as Texture2D;
		//m_3DTex = Resources.Load( "Textures/3dmodeButton" ) as Texture2D;
		//m_2DTex = Resources.Load( "Textures/2dmodeButton" ) as Texture2D;
		//m_VRModeButton.material.SetTexture( "_BorderTex", m_3DTex );
	}

	void Update() {

#if UNITY_EDITOR
		if ( !m_AppController.VRMode ) {
			moveLeft = Input.GetKey( KeyCode.LeftArrow );
			moveRight = Input.GetKey( KeyCode.RightArrow );
			moveUp = Input.GetKey( KeyCode.UpArrow );
			moveDown = Input.GetKey( KeyCode.DownArrow );
		}
#endif
		if ( m_Thumbs.Count > 1 ) {

			ApplyAcceleration();
			IntegrateXVelocity( ShouldMoveX() );

			if ( m_Sorting != SortingType.None ) {
				GetActiveColumn();
				if ( m_ActiveColumn != -1 ) {

					if ( moveLeft || moveRight ) {
						m_ColumnAnchors[m_ActiveColumn].Velocity = m_ColumnAnchors[m_ActiveColumn].Acceleration = 0.0f;
					}
					if ( m_ColumnAnchors[m_ActiveColumn].Tiles > 2 ) {
						ApplyColumnAcceleration();
						IntegrateColumnVelocity( ShouldMoveY() );
					}
					ColumnToRest( ShouldMoveY() );
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

		m_VRModeButton.SetClicked(m_AppController.VRMode);
	}

#endregion

#region Transition Methods

	public void ViewBrowser() {
		this.gameObject.SetActive( true );
		PopThumbs();
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

		ChangeSorting( SortingType.None );
		m_Countries.Clear();
		m_Dates.Clear();
		m_TextPanels.Clear();
		m_Thumbs.Clear();
		PopThumbs();
	}
	public void To2DView() {
		StopAllCoroutines();
		//move sorting buttons up/move thumb browser back
		StartCoroutine( MoveBrowser( true ) );
		StartCoroutine( MoveSortButtons( 0 ) );
	}
	public void To3DView() {
		StopAllCoroutines();
		StartCoroutine( MoveBrowser( false ) );

		if ( m_Sorting != SortingType.None ) {
			StartCoroutine( MoveSortButtons( 1 ) );
		}
		else {
			StartCoroutine( MoveSortButtons( 2 ) );
		}
	}
	public void HideCarousel( int _thumbNum ) {
		foreach ( ThumbTile thumb in m_Thumbs ) {
			if ( thumb.CarouselPos != _thumbNum )
				thumb.gameObject.SetActive( false );
		}
	}
	public void ShowCarousel() {
		foreach ( ThumbTile thumb in m_Thumbs ) {
			thumb.gameObject.SetActive( true );
		}
	}
	private IEnumerator MoveSortButtons( int _position ) {
		float target;
		Vector3 globeTarget, calendarTarget, sortTarget, leftScrollTarget, rightScrollTarget, settingsTarget, VRBtnTarget;

		if ( _position == 0 ) { //Move inwards
			target = 2.21f;
			globeTarget			= new Vector3( m_GlobeButtonPosx + target, m_GlobeButton.transform.localPosition.y, m_GlobeButton.transform.localPosition.z );
			calendarTarget		= new Vector3( m_CalendarButtonPosx + target, m_CalendarButton.transform.localPosition.y, m_CalendarButton.transform.localPosition.z );
			sortTarget			= new Vector3( m_SortOrderPosx - target, m_SortButton.transform.localPosition.y, m_SortButton.transform.localPosition.z );
			leftScrollTarget	= new Vector3( m_LeftScrollPosx + target, m_LeftScroll.transform.localPosition.y, m_LeftScroll.transform.localPosition.z );
			rightScrollTarget	= new Vector3( m_RightScrollPosx - target, m_RightScroll.transform.localPosition.y, m_RightScroll.transform.localPosition.z );
			settingsTarget		= new Vector3( m_SettingsPosx - target, m_SettingsButton.transform.localPosition.y, m_SettingsButton.transform.localPosition.z );
			VRBtnTarget			= new Vector3( m_VRBtnPosx + target, m_VRModeButton.transform.localPosition.y, m_VRModeButton.transform.localPosition.z );

			if ( m_LeftScroll.gameObject.activeSelf ) {
				m_LeftScroll.SetAlpha( 0.46875f );
				m_RightScroll.SetAlpha( 0.46875f );

				StartCoroutine( m_LeftScroll.AlphaFade( 0.0f, 0.02f ) );
				StartCoroutine( m_RightScroll.AlphaFade( 0.0f, 0.02f ) );
			}
			if ( m_TopScroll.gameObject.activeSelf ) {
				m_TopScroll.SetAlpha( 0.46875f );
				m_BotScroll.SetAlpha( 0.46875f );

				StartCoroutine( m_TopScroll.AlphaFade( 0.0f, 0.02f ) );
				StartCoroutine( m_BotScroll.AlphaFade( 0.0f, 0.02f ) );
			}
		}
		else if ( _position == 1 ){ //Move outwards
			globeTarget			= new Vector3( m_GlobeButtonPosx, m_GlobeButton.transform.localPosition.y, m_GlobeButton.transform.localPosition.z );
			calendarTarget		= new Vector3( m_CalendarButtonPosx, m_CalendarButton.transform.localPosition.y, m_CalendarButton.transform.localPosition.z );
			sortTarget			= new Vector3( m_SortOrderPosx, m_SortButton.transform.localPosition.y, m_SortButton.transform.localPosition.z );
			leftScrollTarget	= new Vector3( m_LeftScrollPosx, m_LeftScroll.transform.localPosition.y, m_LeftScroll.transform.localPosition.z );
			rightScrollTarget	= new Vector3( m_RightScrollPosx, m_RightScroll.transform.localPosition.y, m_RightScroll.transform.localPosition.z );
			settingsTarget		= new Vector3( m_SettingsPosx, m_SettingsButton.transform.localPosition.y, m_SettingsButton.transform.localPosition.z );
			VRBtnTarget			= new Vector3( m_VRBtnPosx, m_VRModeButton.transform.localPosition.y, m_VRModeButton.transform.localPosition.z );

			if ( !m_LeftScroll.gameObject.activeSelf ) {
				EnableHorizontalTriggers( true );

				m_LeftScroll.SetAlpha( 0.0f );
				m_RightScroll.SetAlpha( 0.0f );

				StartCoroutine( m_LeftScroll.AlphaFade( 0.46875f, 0.02f ) );
				StartCoroutine( m_RightScroll.AlphaFade( 0.46875f, 0.02f ) );
			}

			if ( !m_TopScroll.gameObject.activeSelf ) {
				EnableVerticalTriggers( true );

				m_TopScroll.SetAlpha( 0.0f );
				m_BotScroll.SetAlpha( 0.0f );

				StartCoroutine( m_TopScroll.AlphaFade( 0.46875f, 0.01f ) );
				StartCoroutine( m_BotScroll.AlphaFade( 0.46875f, 0.01f ) );
			}
			
		}
		else { //Half move for when vertical scrolling is not needed.
			target = 1.2f;
			globeTarget			= new Vector3( m_GlobeButtonPosx + target, m_GlobeButton.transform.localPosition.y, m_GlobeButton.transform.localPosition.z );
			calendarTarget		= new Vector3( m_CalendarButtonPosx + target, m_CalendarButton.transform.localPosition.y, m_CalendarButton.transform.localPosition.z );
			sortTarget			= new Vector3( m_SortOrderPosx - target, m_SortButton.transform.localPosition.y, m_SortButton.transform.localPosition.z );
			leftScrollTarget	= new Vector3( m_LeftScrollPosx + target - 0.13f, m_LeftScroll.transform.localPosition.y, m_LeftScroll.transform.localPosition.z );
			rightScrollTarget	= new Vector3( m_RightScrollPosx - target + 0.13f, m_RightScroll.transform.localPosition.y, m_RightScroll.transform.localPosition.z );
			settingsTarget		= new Vector3( m_SettingsPosx - target, m_SettingsButton.transform.localPosition.y, m_SettingsButton.transform.localPosition.z );
			VRBtnTarget			= new Vector3( m_VRBtnPosx + target, m_VRModeButton.transform.localPosition.y, m_VRModeButton.transform.localPosition.z );

			if ( !m_LeftScroll.gameObject.activeSelf ) {
				EnableHorizontalTriggers( true );

				m_LeftScroll.SetAlpha( 0.0f );
				m_RightScroll.SetAlpha( 0.0f );

				StartCoroutine( m_LeftScroll.AlphaFade( 0.46875f, 0.02f ) );
				StartCoroutine( m_RightScroll.AlphaFade( 0.46875f, 0.02f ) );
			}
		}

		float time = 2.0f * Time.deltaTime;

		while ( Vector3.Distance( m_SortButton.transform.localPosition, sortTarget ) > 0.05f ) {
			m_GlobeButton.gameObject.transform.localPosition	= Vector3.Lerp( m_GlobeButton.transform.localPosition, globeTarget, time );
			m_CalendarButton.transform.localPosition			= Vector3.Lerp( m_CalendarButton.transform.localPosition, calendarTarget, time );
			m_SortButton.transform.localPosition				= Vector3.Lerp( m_SortButton.transform.localPosition, sortTarget, time );
			m_RightScroll.transform.localPosition				= Vector3.Lerp( m_RightScroll.transform.localPosition, rightScrollTarget, time );
			m_LeftScroll.transform.localPosition				= Vector3.Lerp( m_LeftScroll.transform.localPosition, leftScrollTarget, time );
			m_SettingsButton.transform.localPosition			= Vector3.Lerp( m_SettingsButton.transform.localPosition, settingsTarget, time );
			m_VRModeButton.transform.localPosition				= Vector3.Lerp( m_VRModeButton.transform.localPosition, VRBtnTarget, time );
			yield return null;
		}
		m_GlobeButton.transform.localPosition		= globeTarget;
		m_CalendarButton.transform.localPosition	= calendarTarget;
		m_SortButton.transform.localPosition		= sortTarget;
		m_LeftScroll.transform.localPosition		= leftScrollTarget;
		m_RightScroll.transform.localPosition		= rightScrollTarget;
		m_SettingsButton.transform.localPosition	= settingsTarget;
		m_VRModeButton.transform.localPosition		= VRBtnTarget;
	}

#endregion

#region De-Selected Methods

	public void LeftTrigNoHover() {
		moveLeft = false;
		m_LeftScroll.SetClicked( false );
	}

	public void RightTrigNoHover() {
		moveRight = false;
		m_RightScroll.SetClicked( false );
	}

	public void TopTrigNoHover() {
		moveUp = false;
		m_TopScroll.SetClicked( false );
	}

	public void BotTrigNoHover() {
		moveDown = false;
		m_BotScroll.SetClicked( false );
	}

#endregion

#region Clicked Methods

	public void PVToggleClicked() {
		imageMode = !imageMode;
	}
	public void SortButtonClicked() {
		if ( m_Sorting != SortingType.None ) {
			m_Ascending = !m_Ascending;

			if ( m_Ascending ) {
				m_SortButton.Material.SetTextureScale( "_BorderTex", new Vector2( 1, 1 ) );
			}
			else {
				m_SortButton.Material.SetTextureScale( "_BorderTex", new Vector2( 1, -1 ) );
			}

			if ( m_Sorting == SortingType.Date ) {
				SortColumns( false );
			}
			else if ( m_Sorting == SortingType.Country ) {
				SortColumns( true );
			}
		}
	}
	public void GlobeButtonClicked() {
		if ( m_Sorting == SortingType.Country ) {
			RefreshBrowser();
			ChangeSorting( SortingType.None );
		}
		else {
			SortColumns( true );
			ChangeSorting( SortingType.Country );
			m_CalendarButton.SetClicked( false );
		}
	}
	public void CalendarButtonClicked() {
		if ( m_Sorting == SortingType.Date ) {
			RefreshBrowser();
			ChangeSorting( SortingType.None );
		}
		else {
			SortColumns( false );
			ChangeSorting( SortingType.Date );
			m_GlobeButton.SetClicked( false );
		}
	}
	public void VRModeButtonClicked() {
		m_AppController.VrMode();
	}
	public void LeftTrigClicked() {
		moveRight = !moveRight;
	}
	public void RightTrigClicked() {
		moveLeft = !moveLeft;
	}
	public void TopTrigClicked() {
		moveUp = !moveUp;
	}
	public void BotTrigClicked() {
		moveDown = !moveDown;
	}
	public void SettingsButtonClicked() {

	}

#endregion

#region Sorting Methods

	private void SortDates() {
		Vector3 pos = m_ThumbAnchor.LocalPos;
		pos.x = 0.0f;
		m_ThumbAnchor.LocalPos = pos;
		if ( m_Ascending ) {
			m_Dates.Sort( ( a, b ) => a.CompareTo( b ) );
		}
		else {
			m_Dates.Sort( ( a, b ) => b.CompareTo( a ) );
		}
	}
	private void SortCountries() {
		Vector3 pos = m_ThumbAnchor.LocalPos;
		pos.x = 0.0f;
		m_ThumbAnchor.LocalPos = pos;
		if ( m_Ascending ) {
			m_Countries.Sort( ( a, b ) => a.CompareTo( b ) );
		}
		else {
			m_Countries.Sort( ( a, b ) => b.CompareTo( a ) );
		}
	}
	//Provides the initial, unsorted display of the thubnails.
	public void PopThumbs() {
		FileHandler.Thumbnail[] thumbs = m_AppController.FH.GetThumbs();
		float x = 0;
		float y = ySpacing - 0.3f;
		m_ThumbAnchor.LocalPos = new Vector3( 0.0f, -0.85f, -2.95f );
		for ( int i = 0; i < thumbs.Length; ++i ) {
			ThumbTile temp = Instantiate( m_TilePrefab ) as ThumbTile;
			temp.transform.parent = m_ThumbAnchor.transform;
			Vector3 tPos = Vector3.zero;
			tPos.x = x;
			tPos.y = y;
			AddCountry( thumbs[i].Country );
			AddDate( thumbs[i].Date );
			temp.SetThumb( thumbs[i] );
			temp.CarouselPos = i;
			temp.SetPos( tPos );
			m_Thumbs.Add( temp );
			if ( (i + 1) % 3 == 0 ) {
				x += xSpacing;
				y = ySpacing - 0.3f;
			}
			else {
				y -= ySpacing;
			}
		}
	}
	//Rearranges the columns with sorting rules applied.
	private void SortColumns( bool _byCountry ) {
		m_xVelocity = m_xAcceleration = 0.0f; //Stop things moving.
		if ( m_TextPanels.Count > 0 ) { //Get rid of any existing panels
			foreach ( TextPanel tp in m_TextPanels ) {
				DestroyObject( tp.gameObject );
			}
			m_TextPanels.Clear();
		}

		if ( _byCountry )
			SortCountries();
		else 
			SortDates();

		float x = xSpacing;
		float y = ySpacing * 2 - 0.1f;
		float yPadding = 0.3f;
		int row = 0;
		int column = 0;

		List<ThumbTile> temp = new List<ThumbTile>();
		List<ColumnAnchor> tempAnchs = new List<ColumnAnchor>();

		if ( _byCountry ) {
			foreach ( string str in m_Countries ) {
				TextPanel tempTx = Instantiate( m_TextPanelPrefab ) as TextPanel; //First a title panel is made to denote the country
				Vector3 pos = new Vector3( x * column, y, -0.1f );

				tempTx.transform.parent = m_ThumbAnchor.transform;
				tempTx.SetPos( pos );

				tempTx.SetText( str == "" ? "Unknown" : str );
				m_TextPanels.Add( tempTx );

				ColumnAnchor anchor = Instantiate( m_ColumnAnchorPrefab ) as ColumnAnchor;
				anchor.transform.parent = m_ThumbAnchor.transform;
				anchor.LocalPos = new Vector3( x * column, -( row * ySpacing ) + ySpacing, 0.0f );
				if ( _byCountry ) {
					for ( int i = 0; i < m_Thumbs.Count; ++i ) {
						if ( m_Thumbs[i].Image.Country == str ) {
							ThumbTile tempTile = Instantiate( m_TilePrefab ) as ThumbTile;
							tempTile.transform.parent = anchor.transform;
							tempTile.SetThumb( m_Thumbs[i].Image );
							Vector3 tPos = new Vector3( 0.0f, -( row * ySpacing ) - yPadding, 0.0f );

							tempTile.SetPos( tPos );
							tempTile.CarouselPos = i;
							temp.Add( tempTile );

							row++;
						}
					}
				}
				anchor.Tiles = row;
				anchor.BottomPoint = -( row * ySpacing ) + ySpacing;
				tempAnchs.Add( anchor );
				column++;
				row = 0;
			}
		}
		else {
			foreach ( DateTime str in m_Dates ) {
				TextPanel tempTx = Instantiate( m_TextPanelPrefab ) as TextPanel; //First a title panel is made to denote the country
				Vector3 pos = new Vector3( x * column, y, -0.1f );

				tempTx.transform.parent = m_ThumbAnchor.transform;
				tempTx.SetPos( pos );

				tempTx.SetText( str == new DateTime( 1970, 1, 1 ) ? "Unknown" : DateFormat.WrittenDate( str ) );
				m_TextPanels.Add( tempTx );

				ColumnAnchor anchor = Instantiate( m_ColumnAnchorPrefab ) as ColumnAnchor;
				anchor.transform.parent = m_ThumbAnchor.transform;
				anchor.LocalPos = new Vector3( x * column, -( row * ySpacing ) + ySpacing, 0.0f );

				for ( int i = 0; i < m_Thumbs.Count; ++i ) {
					if ( m_Thumbs[i].Image.Date == str ) {
						ThumbTile tempTile = Instantiate( m_TilePrefab ) as ThumbTile;
						tempTile.transform.parent = anchor.transform;
						tempTile.SetThumb( m_Thumbs[i].Image );
						Vector3 tPos = new Vector3( 0.0f, -( row * ySpacing ) - yPadding, 0.0f );

						tempTile.SetPos( tPos );
						tempTile.CarouselPos = i;
						temp.Add( tempTile );

						row++;
					}
				}
				anchor.Tiles = row;
				anchor.BottomPoint = -( row * ySpacing ) + ySpacing;
				tempAnchs.Add( anchor );
				column++;
				row = 0;
			}
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

		//Call a corourtine to move some stuff
		
	}
	private void ChangeSorting( SortingType _arg ) {
		StopAllCoroutines();
		m_Sorting = _arg;
		if ( m_Sorting == SortingType.None ) {
			
			EnableVerticalTriggers( false );
			if ( m_AppController.VRMode ) { 
				StartCoroutine( MoveSortButtons( 2 ) );
			}
			else {
				StartCoroutine( MoveSortButtons( 0 ) );
			}
		}
		else if ( m_Sorting == SortingType.Date ) {
			if ( m_AppController.VRMode ) {
				StartCoroutine( MoveSortButtons( 1 ) );
			}
		}
		else {
			if ( m_AppController.VRMode ) {
				StartCoroutine( MoveSortButtons( 1 ) );
			}
		}
	}

#endregion

#region Carousel Motion Methods

	public void SweepToCentre( GameObject _tileToMove ) {
		StopAllCoroutines();
		Vector3 targetPos = m_ThumbAnchor.Transform.position - new Vector3( _tileToMove.transform.position.x, 0.0f, 0.0f );
		StartCoroutine( SweepOverTime( _tileToMove, targetPos ) );
	}
	private IEnumerator SweepOverTime( GameObject _tileToMove, Vector3 _targetPos ) {
		m_Sweeping = true;
		float time = 0.0f;
		while ( Mathf.Abs( _tileToMove.transform.position.x ) > 0.1f ) { //Current position of tile is not 0
			
			m_ThumbAnchor.Transform.position = Vector3.Lerp( m_ThumbAnchor.Transform.position, _targetPos, time);
			time += Time.deltaTime / Mathf.Abs(_tileToMove.transform.position.x);
			yield return null;
		}
		m_Sweeping = false;
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
	private bool ShouldMoveY() { //Does the user want the carousel to move?
		if ( m_AppController.TC.SwipeDirection[1] != TouchController.Swipe.None || ( m_Sorting != SortingType.None && ( moveDown || moveUp ) ) ) {
			return true;
		}
		else {
			return false;
		}
	}
	private bool ShouldMoveX() {
		if ( m_AppController.TC.SwipeDirection[0] != TouchController.Swipe.None || ( moveLeft || moveRight ) ) {
			return true;
		}
		else {
			return false;
		}
	}
	private void ApplyAcceleration() { //Will apply an acceleration to the carousel in the desired direction.
		//Forces the carousel to immediately change direction when it's asked to move in the opposite direciton to it's current.
		if ( ( moveRight || m_AppController.TC.SwipeDirection[0] == TouchController.Swipe.Positive ) && m_xAcceleration < m_MaxAcceleration ) {
			m_xAcceleration += m_AccInc;
			if ( m_xVelocity < 0.0f ) {
				m_xVelocity = 0.5f;
			}
		}
		else if ( ( moveLeft || m_AppController.TC.SwipeDirection[0] == TouchController.Swipe.Negative ) && m_xAcceleration > -m_MaxAcceleration ) {
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
		if ( ( moveUp || m_AppController.TC.SwipeDirection[1] == TouchController.Swipe.Positive ) && m_ColumnAnchors[m_ActiveColumn].Acceleration < m_MaxAcceleration ) {
			m_ColumnAnchors[m_ActiveColumn].Acceleration -= m_AccInc;
			if ( m_ColumnAnchors[m_ActiveColumn].Velocity < 0.0f ) {
				m_ColumnAnchors[m_ActiveColumn].Velocity = -0.5f;
			}
		}
		else if ( ( moveDown || m_AppController.TC.SwipeDirection[1] == TouchController.Swipe.Negative ) && m_ColumnAnchors[m_ActiveColumn].Acceleration > -m_MaxAcceleration ) {
			m_ColumnAnchors[m_ActiveColumn].Acceleration += m_AccInc;
			if ( m_ColumnAnchors[m_ActiveColumn].Velocity > 0.0f ) {
				m_ColumnAnchors[m_ActiveColumn].Velocity = 0.5f;
			}
		}
		else {
			m_ColumnAnchors[m_ActiveColumn].Acceleration = 0.0f;
		}
	}
	private void IntegrateXVelocity( bool _moving ) {
		Vector3 pos = m_ThumbAnchor.Transform.position;

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
			else {
				if ( m_AppController.TC.SwipeSpeed.x > m_AppController.TC.SwipeSpeed.y ) {
					if ( m_AppController.TC.SwipeDirection[0] == TouchController.Swipe.Positive ) {
						m_xVelocity = Mathf.Min( m_AppController.TC.SwipeSpeed.x * 0.08f, m_MaxVelocity );
					}
					else if ( m_AppController.TC.SwipeDirection[0] == TouchController.Swipe.Negative ) {
						m_xVelocity = Mathf.Max( -m_AppController.TC.SwipeSpeed.x * 0.08f, -m_MaxVelocity );
					}
				}
			}
		}

		//If it's really slow, just stop.
		if ( Mathf.Abs( m_xVelocity ) <= m_MinVelocity && !_moving ) {
			m_xVelocity = 0.0f;
		}
		//Assign the new position of the carousel (d = V * t)
		pos.x = m_ThumbAnchor.Transform.position.x + m_xVelocity * Time.deltaTime;
		m_ThumbAnchor.Transform.position = pos;
	}
	private void IntegrateColumnVelocity( bool _moving ) { //Mostly the same as IntegrateXVelocity, just for the active column's vertical motion.
		float yVelocity;
		//Calculate the velocity of the column and apply damping if the user isn't trying to move it.
		yVelocity = ( m_ColumnAnchors[m_ActiveColumn].Velocity + m_ColumnAnchors[m_ActiveColumn].Acceleration * Time.deltaTime ) * ( _moving ? 1.0f : 0.96f );

		//Keep it from scrolling faster than a certain rate.
		if ( yVelocity > m_MaxVelocity ) {
			yVelocity = m_MaxVelocity;
		}
		else if ( yVelocity < -m_MaxVelocity ) {
			yVelocity = -m_MaxVelocity;
		}

		//Inverts the velocity (with damping) if it has scrolled to the top or bottom.
		if ( ( m_ColumnAnchors[m_ActiveColumn].LocalPos.y < 0.6f && yVelocity < 0.0f ) ||
		( m_ColumnAnchors[m_ActiveColumn].LocalPos.y > ( ySpacing * m_ColumnAnchors[m_ActiveColumn].Tiles - ( ySpacing + 0.6f ) ) && yVelocity > 0.0f ) ) {
			yVelocity = -yVelocity * 0.6f;
		}

		else {
			if ( m_AppController.TC.SwipeSpeed.y > m_AppController.TC.SwipeSpeed.x ) {
				if ( m_AppController.TC.SwipeDirection[1] == TouchController.Swipe.Positive ) {
					yVelocity = Mathf.Min( m_AppController.TC.SwipeSpeed.y * 0.03f, m_MaxVelocity );
				}
				else if ( m_AppController.TC.SwipeDirection[1] == TouchController.Swipe.Negative ) {
					yVelocity = Mathf.Max( -m_AppController.TC.SwipeSpeed.y * 0.03f, -m_MaxVelocity );
				}
			}
		}

		//If its really slow, stop it.
		if ( Mathf.Abs( yVelocity ) <= m_MinVelocity && !_moving ) {
			yVelocity = 0.0f;
		}

		//Assign the new position of the column.
		float yPos = m_ColumnAnchors[m_ActiveColumn].LocalPos.y + yVelocity * Time.deltaTime;
		m_ColumnAnchors[m_ActiveColumn].LocalY = yPos;
		m_ColumnAnchors[m_ActiveColumn].Velocity = yVelocity;
	}
	private void ColumnToRest( bool _moving ) {
		if ( !_moving ) {
			Vector3 pos;
			for ( int i = 0; i < m_ColumnAnchors.Count; i++ ) {
				if ( i != m_ActiveColumn ) {
					pos = m_ColumnAnchors[i].gameObject.transform.localPosition;

					float round = Mathf.Round( pos.y );

					if ( round < -1.0f ) {
						round = -1.0f;
					}
					else if ( round > ( ySpacing * m_ColumnAnchors[i].Tiles ) ) {
						round = ySpacing * m_ColumnAnchors[i].Tiles;
					}

					if ( pos.y != round ) {
						pos.y = round;
						if ( pos.y > 0 )
							m_ColumnAnchors[i].gameObject.transform.localPosition = pos;
					}
				}
			}
		}
	}

	#endregion

#region Misc Methods

	public void DeselectThumbs() {
		foreach ( ThumbTile TT in m_Thumbs ) {
			TT.DeClicked();
		}
	}
	private IEnumerator MoveBrowser( bool _away ) {
		Vector3 target;
		if ( _away ) {
			target = new Vector3( 0.0f, 7.25f, 7.0f );
		}
		else {
			target = new Vector3( 0.0f, 7.25f, 7.2f );//= new Vector3( 0.0f, 7.35f, 6.78f );
		}

		float diff = Vector3.Distance( target, this.gameObject.transform.localPosition ) * 0.5f;

		while ( Vector3.Distance( target, this.gameObject.transform.position ) > diff ) {

			this.gameObject.transform.localPosition = Vector3.Slerp( this.gameObject.transform.localPosition, target, 2.0f * Time.deltaTime );
			yield return null;
		}
		this.gameObject.transform.localPosition = target;

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
			m_Countries.Add( StringCheck( _country ) );
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
	private string StringCheck( string _arg ) {
		if ( _arg != null )
			return _arg;
		else
			return "";
	}
	private void EnableVerticalTriggers( bool _arg ) {
		m_TopScroll.gameObject.SetActive( _arg );
		m_BotScroll.gameObject.SetActive( _arg );
	}
	private void EnableHorizontalTriggers( bool _arg ) {
		m_RightScroll.gameObject.SetActive( _arg );
		m_LeftScroll.gameObject.SetActive( _arg );
	}
	
	private void SetAlpha( MeshRenderer _argMesh, float _value ) {
		Color color = _argMesh.material.color;
		color.a = _value;
		_argMesh.material.color = color;
	}

#endregion

}