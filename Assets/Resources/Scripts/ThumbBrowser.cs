using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/* Controls the browser component of the application.
 * Specifically, handles:
 * - Sorting of thumbnails.
 * - Button-based methods (clicked/selected/deselected)
 * - Scrolling and switching between sorting methods.
 * - Placed in regions to reduce the clutter/madness
 */

public class ThumbBrowser : MonoBehaviour {

#region Variable Declarations

	#region Variables - Editor

	[SerializeField]
	private AppController m_AppController;

	[SerializeField]
	private ThumbTile m_TilePrefab;

	[SerializeField]
	private MeshRenderer m_LeftScroll;

	[SerializeField]
	private MeshRenderer m_RightScroll;

	[SerializeField]
	private MeshRenderer m_TopScroll;

	[SerializeField]
	private MeshRenderer m_BotScroll;

	[SerializeField]
	private MeshRenderer m_VideoToggle;

	[SerializeField]
	private MeshRenderer m_SortButton;

	[SerializeField]
	private MeshRenderer m_CalendarIcon;

	[SerializeField]
	private MeshRenderer m_GlobeIcon;

	[SerializeField]
	private MeshRenderer m_VRModeButton;

	[SerializeField]
	private GameObject m_ThumbAnchor;

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
	private Color m_ScrollColor = new Color( 0.34f, 0.48f, 0.72f, 0.47f );
	private Color m_ScrollColorHover = new Color( 0.34f, 0.48f, 0.72f, 0.74f );
	private Color m_ActiveButtonColor = new Color( 0.32f, 0.7f, 0.4f, 0.47f );
	private Color m_ActiveButtonColorHover = new Color( 0.32f, 0.7f, 0.4f, 0.74f );
	private float m_MaxVelocity = 10.0f;
	private float m_MaxAcceleration = 4.0f;
	private float m_xAcceleration = 0.0f;
	private float m_AccInc = 1.5f;
	private float m_xVelocity = 0.0f;
	private float m_MinVelocity = 0.1f;
	private float m_GlobeButtonPosx = -1.8f;
	private float m_CalendarButtonPosx = -3.34f;
	private float m_SortOrderPosx = 4.9f;
	private float m_LeftScrollPosx = -1.5f;
	private float m_RightScrollPosx = 1.5f;
	private Texture2D m_PhotoTex, m_VideoTex, m_3DTex, m_2DTex;

	#endregion

#endregion

#region MonoBehaviour Overrides

	void Start() {
		m_PhotoTex = Resources.Load( "Textures/Picture" ) as Texture2D;
		m_VideoTex = Resources.Load( "Textures/Video" ) as Texture2D;
		m_3DTex = Resources.Load( "Textures/3dmodeButton" ) as Texture2D;
		m_2DTex = Resources.Load( "Textures/2dmodeButton" ) as Texture2D;
		m_VideoToggle.material.SetTexture( "_BorderTex", m_PhotoTex );
		m_VRModeButton.material.SetTexture( "_BorderTex", m_3DTex );
		//Set all the things to a color.
		m_LeftScroll.material.color = 
		m_RightScroll.material.color = 
		m_TopScroll.material.color = 
		m_BotScroll.material.color = 
		m_SortButton.material.color = 
		m_GlobeIcon.material.color = 
		m_CalendarIcon.material.color =
		m_VideoToggle.material.color = 
		m_VRModeButton.material.color = m_ScrollColor;
	}

	void Update() {
#if UNITY_EDITOR

		moveLeft = Input.GetKey( KeyCode.LeftArrow );
		moveRight = Input.GetKey( KeyCode.RightArrow );
		moveUp = Input.GetKey( KeyCode.UpArrow );
		moveDown = Input.GetKey( KeyCode.DownArrow );

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
	private IEnumerator MoveSortButtons( int _position ) {
		float target;
		Vector3 globeTarget, calendarTarget, sortTarget, leftScrollTarget, rightScrollTarget;

		if ( _position == 0 ) { //Move inwards
			target = 2.15f;
			globeTarget			= new Vector3( m_GlobeButtonPosx + target, m_GlobeIcon.transform.localPosition.y, m_GlobeIcon.transform.localPosition.z );
			calendarTarget		= new Vector3( m_CalendarButtonPosx + target, m_CalendarIcon.transform.localPosition.y, m_CalendarIcon.transform.localPosition.z );
			sortTarget			= new Vector3( m_SortOrderPosx - target, m_SortButton.transform.localPosition.y, m_SortButton.transform.localPosition.z );
			leftScrollTarget	= new Vector3( m_LeftScrollPosx + target, m_LeftScroll.transform.localPosition.y, m_LeftScroll.transform.localPosition.z );
			rightScrollTarget	= new Vector3( m_RightScrollPosx - target, m_RightScroll.transform.localPosition.y, m_RightScroll.transform.localPosition.z );

			if ( m_LeftScroll.gameObject.activeSelf ) {
				SetAlpha( m_LeftScroll, 0.46875f );
				SetAlpha( m_RightScroll, 0.46875f );

				StartCoroutine( AlphaFade( m_LeftScroll, 0.0f, 0.02f ) );
				StartCoroutine( AlphaFade( m_RightScroll, 0.0f, 0.02f ) );
			}
			if ( m_TopScroll.gameObject.activeSelf ) {
				SetAlpha( m_TopScroll, 0.46875f );
				SetAlpha( m_BotScroll, 0.46875f );

				StartCoroutine( AlphaFade( m_TopScroll, 0.0f, 0.02f ) );
				StartCoroutine( AlphaFade( m_BotScroll, 0.0f, 0.02f ) );
			}
		}
		else if ( _position == 1 ){ //Move outwards
			globeTarget			= new Vector3( m_GlobeButtonPosx, m_GlobeIcon.transform.localPosition.y, m_GlobeIcon.transform.localPosition.z );
			calendarTarget		= new Vector3( m_CalendarButtonPosx, m_CalendarIcon.transform.localPosition.y, m_CalendarIcon.transform.localPosition.z );
			sortTarget			= new Vector3( m_SortOrderPosx, m_SortButton.transform.localPosition.y, m_SortButton.transform.localPosition.z );
			leftScrollTarget	= new Vector3( m_LeftScrollPosx, m_LeftScroll.transform.localPosition.y, m_LeftScroll.transform.localPosition.z );
			rightScrollTarget	= new Vector3( m_RightScrollPosx, m_RightScroll.transform.localPosition.y, m_RightScroll.transform.localPosition.z );

			if ( !m_LeftScroll.gameObject.activeSelf ) {
				EnableHorizontalTriggers( true );

				SetAlpha( m_LeftScroll, 0.0f );
				SetAlpha( m_RightScroll, 0.0f );

				StartCoroutine( AlphaFade( m_LeftScroll, 0.46875f, 0.02f ) );
				StartCoroutine( AlphaFade( m_RightScroll, 0.46875f, 0.02f ) );
			}

			if ( !m_TopScroll.gameObject.activeSelf ) {
				EnableVerticalTriggers( true );

				SetAlpha( m_TopScroll, 0.0f );
				SetAlpha( m_BotScroll, 0.0f );

				StartCoroutine( AlphaFade( m_TopScroll, 0.46875f, 0.01f ) );
				StartCoroutine( AlphaFade( m_BotScroll, 0.46875f, 0.01f ) );
			}
			
		}
		else { //Half move for when vertical scrolling is not needed.
			target = 1.2f;
			globeTarget			= new Vector3( m_GlobeButtonPosx + target, m_GlobeIcon.transform.localPosition.y, m_GlobeIcon.transform.localPosition.z );
			calendarTarget		= new Vector3( m_CalendarButtonPosx + target, m_CalendarIcon.transform.localPosition.y, m_CalendarIcon.transform.localPosition.z );
			sortTarget			= new Vector3( m_SortOrderPosx - target, m_SortButton.transform.localPosition.y, m_SortButton.transform.localPosition.z );
			leftScrollTarget	= new Vector3( m_LeftScrollPosx + target - 0.13f, m_LeftScroll.transform.localPosition.y, m_LeftScroll.transform.localPosition.z );
			rightScrollTarget	= new Vector3( m_RightScrollPosx - target + 0.13f, m_RightScroll.transform.localPosition.y, m_RightScroll.transform.localPosition.z );

			if ( !m_LeftScroll.gameObject.activeSelf ) {
				EnableHorizontalTriggers( true );

				SetAlpha( m_LeftScroll, 0.0f );
				SetAlpha( m_RightScroll, 0.0f );

				StartCoroutine( AlphaFade( m_LeftScroll, 0.46875f, 0.02f ) );
				StartCoroutine( AlphaFade( m_RightScroll, 0.46875f, 0.02f ) );
			}
		}

		float time = 2.0f * Time.deltaTime;

		while ( Vector3.Distance( m_SortButton.transform.localPosition, sortTarget ) > 0.05f ) {
			m_GlobeIcon.gameObject.transform.localPosition	= Vector3.Lerp( m_GlobeIcon.transform.localPosition, globeTarget, time );
			m_CalendarIcon.transform.localPosition			= Vector3.Lerp( m_CalendarIcon.transform.localPosition, calendarTarget, time );
			m_SortButton.transform.localPosition			= Vector3.Lerp( m_SortButton.transform.localPosition, sortTarget, time );
			m_RightScroll.transform.localPosition			= Vector3.Lerp( m_RightScroll.transform.localPosition, rightScrollTarget, time );
			m_LeftScroll.transform.localPosition			= Vector3.Lerp( m_LeftScroll.transform.localPosition, leftScrollTarget, time );
			yield return null;
		}
		m_GlobeIcon.transform.localPosition		= globeTarget;
		m_CalendarIcon.transform.localPosition	= calendarTarget;
		m_SortButton.transform.localPosition	= sortTarget;
		m_LeftScroll.transform.localPosition	= leftScrollTarget;
		m_RightScroll.transform.localPosition	= rightScrollTarget;

	}

#endregion

#region Hover Methods

	public void LeftTrigHover() {
		m_LeftScroll.material.color = m_ScrollColorHover;
	}
	public void RightTrigHover() {
		m_RightScroll.material.color = m_ScrollColorHover;
	}
	public void TopTrigHover() {
		m_TopScroll.material.color = m_ScrollColorHover;
	}
	public void BotTrigHover() {
		m_BotScroll.material.color = m_ScrollColorHover;
	}
	public void PVToggleHover() {
		m_VideoToggle.material.color = m_ScrollColorHover;
	}
	public void SortButtonHover() {
		m_SortButton.material.color = m_ScrollColorHover;
	}
	public void GlobeButtonHover() {
		if ( m_Sorting == SortingType.Country ) {
			m_GlobeIcon.material.color = m_ActiveButtonColorHover;
		}
		else {
			m_GlobeIcon.material.color = m_ScrollColorHover;
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
	public void VRModeButtonHover() {
		m_VRModeButton.material.color = m_ScrollColorHover;
	}

#endregion

#region No-Hover Methods

	public void LeftTrigNoHover() {
		m_LeftScroll.material.color = m_ScrollColor;
		moveRight = false;
	}
	public void RightTrigNoHover() {
		m_RightScroll.material.color = m_ScrollColor;
		moveLeft = false;
	}
	public void TopTrigNoHover() {
		m_TopScroll.material.color = m_ScrollColor;
		moveUp = false;
	}
	public void BotTrigNoHover() {
		m_BotScroll.material.color = m_ScrollColor;
		moveDown = false;
	}
	public void PVToggleNoHover() {
		m_VideoToggle.material.color = m_ScrollColor;
	}
	public void SortButtonNoHover() {
		m_SortButton.material.color = m_ScrollColor;
	}
	public void GlobeButtonNoHover() {
		if ( m_Sorting == SortingType.Country ) {
			m_GlobeIcon.material.color = m_ActiveButtonColor;
		}
		else {
			m_GlobeIcon.material.color = m_ScrollColor;
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
	public void VRModeButtonNoHover() {
		m_VRModeButton.material.color = m_ScrollColor;
	}

#endregion

#region Clicked Methods

	public void PVToggleClicked() {
		if ( imageMode ) {
			m_VideoToggle.material.SetTexture( "_BorderTex", m_VideoTex );
		}
		else {
			m_VideoToggle.material.SetTexture( "_BorderTex", m_PhotoTex );
		}
		imageMode = !imageMode;
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
			SortColumns( false );
		}
		else if ( m_Sorting == SortingType.Country ) {
			SortColumns( true );
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
		}
	}
	public void VRModeButtonClicked() {
		m_AppController.VrMode();

		if ( m_AppController.VRMode ) {
			m_VRModeButton.material.SetTexture( "_BorderTex", m_2DTex );
		}
		else {
			m_VRModeButton.material.SetTexture( "_BorderTex", m_3DTex );
		}
	}
	public void LeftTrigClicked() {
		moveRight = !moveRight;
		if (moveRight)
			m_LeftScroll.material.color = m_ActiveButtonColorHover;
		else
			m_LeftScroll.material.color = m_ScrollColorHover;
	}
	public void RightTrigClicked() {
		moveLeft = !moveLeft;
		if ( moveLeft )
			m_RightScroll.material.color = m_ActiveButtonColorHover;
		else
			m_RightScroll.material.color = m_ScrollColorHover;
	}
	public void TopTrigClicked() {
		moveUp = !moveUp;
		if (moveUp)
			m_TopScroll.material.color = m_ActiveButtonColorHover;
		else
			m_TopScroll.material.color = m_ScrollColorHover;
	}
	public void BotTrigClicked() {
		moveDown = !moveDown;
		if ( moveDown )
			m_BotScroll.material.color = m_ActiveButtonColorHover;
		else
			m_BotScroll.material.color = m_ScrollColorHover;
	}

#endregion

#region Sorting Methods

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
	//Provides the initial, unsorted display of the thubnails.
	public void PopThumbs() {
		FileHandler.Thumbnail[] thumbs = m_AppController.FH.GetThumbs();
		float x = 0;
		float y = ySpacing - 0.3f;
		m_ThumbAnchor.gameObject.transform.localPosition = new Vector3( 0.0f, -0.85f, -2.95f );
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
			m_GlobeIcon.material.color = m_ScrollColor;
			m_CalendarIcon.material.color = m_ScrollColor;
			EnableVerticalTriggers( false );
			if ( m_AppController.VRMode ) { 
				StartCoroutine( MoveSortButtons( 2 ) );
			}
			else {
				StartCoroutine( MoveSortButtons( 0 ) );
			}
		}
		else if ( m_Sorting == SortingType.Date ) {
			m_GlobeIcon.material.color = m_ScrollColor;
			m_CalendarIcon.material.color = m_ActiveButtonColor;
			if ( m_AppController.VRMode ) {
				StartCoroutine( MoveSortButtons( 1 ) );
			}
		}
		else {
			m_GlobeIcon.material.color = m_ActiveButtonColor;
			m_CalendarIcon.material.color = m_ScrollColor;
			if ( m_AppController.VRMode ) {
				StartCoroutine( MoveSortButtons( 1 ) );
			}
		}
	}

#endregion

#region Carousel Motion Methods

	public void SweepToCentre( GameObject _tileToMove ) {
		StopAllCoroutines();
		Vector3 targetPos = m_ThumbAnchor.transform.position - new Vector3( _tileToMove.transform.position.x, 0.0f, 0.0f );
		StartCoroutine( SweepOverTime( _tileToMove, targetPos ) );
	}
	private IEnumerator SweepOverTime( GameObject _tileToMove, Vector3 _targetPos ) {
		
		while ( Mathf.Abs( _tileToMove.transform.position.x ) > 0.1f ) { //Current position of tile is not 0
			
			m_ThumbAnchor.transform.position = Vector3.Lerp( m_ThumbAnchor.transform.position, _targetPos, Time.deltaTime);

			yield return null;
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
		pos.x = m_ThumbAnchor.transform.position.x + m_xVelocity * Time.deltaTime;
		m_ThumbAnchor.transform.position = pos;
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
	private IEnumerator AlphaFade( MeshRenderer _argMesh, float _target, float _rate ) {
		Color color = _argMesh.material.color;
		bool subtract = _target < color.a ? true : false;

		while ( Mathf.Abs(_target - color.a ) > 0.01f ) {
			color = _argMesh.material.color;
			
			if ( subtract )
				color.a -= _rate;
			else
				color.a += _rate;
			
			_argMesh.material.color = color;
			
			yield return null;
		}
		color.a = _target;
		_argMesh.material.color = color;

		if ( _target == 0.0f ) {
			_argMesh.gameObject.SetActive( false );
		}

	}
	private void SetAlpha( MeshRenderer _argMesh, float _value ) {
		Color color = _argMesh.material.color;
		color.a = _value;
		_argMesh.material.color = color;
	}

#endregion

}