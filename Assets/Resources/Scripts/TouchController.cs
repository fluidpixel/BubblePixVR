using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TouchController : MonoBehaviour {

	private bool couldBeSwipe = false;
	private Vector2 startPos = Vector2.zero;
	private float swipeStartTime = 0;

	private float comfortZone = 100.0f;
	private float minSwipeDist = 40.0f;
	private float maxSwipeTime = 1.0f;
	//private float lastSwipeTime = 0.0f;
	private float swipeSpeed = 0.0f;

	public float SwipeSpeed {
		get { return swipeSpeed; }
	}

	[SerializeField]
	private Text m_Text;

	public enum SwipeTypes {
		None = 0,
		Left = 1,
		Right = 2
	};

	private SwipeTypes m_SwipeDirection = SwipeTypes.None;

	public SwipeTypes SwipeDirection {
		get { return m_SwipeDirection; }
	}

	void Start () {
		//StartCoroutine(CheckHorizontalSwipes());
	}

	void Update() {
		m_SwipeDirection = SwipeTypes.None;
		if ( Input.touchCount > 0 ) {
			Touch touch = Input.touches[0];
			switch ( touch.phase ) { //Screen has been touched, this could be a swipe.
				case TouchPhase.Began:
					m_SwipeDirection = SwipeTypes.None;
					//lastSwipeTime = 0.0f;
					couldBeSwipe = true;
					startPos = touch.position;
					swipeStartTime = Time.time;
					break;

				case TouchPhase.Moved:
					if ( Mathf.Abs( touch.position.y - startPos.y ) > comfortZone ) { //Input is all over the place, probably not a swipe...
						couldBeSwipe = false;
					}
					break;

				case TouchPhase.Ended:
					if ( couldBeSwipe ) {
						float swipeTime = Time.time - swipeStartTime;
						float swipeDist = (new Vector3(0, touch.position.x, 0) - new Vector3(0, startPos.x, 0)).magnitude;

						if ( ( swipeTime < maxSwipeTime ) && ( swipeDist > minSwipeDist ) ) { //If this is true, its a swipe.
							float swipeValue = Mathf.Sign(touch.position.x - startPos.x);

							//If the value is positive, it was a swipe right, otherwise its going left.
							if ( swipeValue > 0 ) {
								m_SwipeDirection = SwipeTypes.Right;
							}
							else if ( swipeValue < 0 ) {
								m_SwipeDirection = SwipeTypes.Left;
							}
							
							swipeSpeed = swipeDist / swipeTime;

							//lastSwipeTime = Time.time;
						}
					}
					else {
						m_SwipeDirection = SwipeTypes.None;
						swipeSpeed = 0.0f;
					}
					break;
			}
		}
		m_Text.text = swipeSpeed.ToString();
	}
}