package com.sherif.cardboard3d.bitmaphandler;

import android.graphics.Bitmap;
import android.media.MediaMetadataRetriever;
import android.util.Log;

/**
 * Created by Sherif Salem on 27/5/2015.
 * Does things to video files so that unity can use them (Work In Progress)
 */

public class VideoHandler {

	//Create a constructor that forces the use of a string location.
	//This location is then used to find information about the video file and apply this as
	//properties of this class.

	private Bitmap m_frame;

	public Bitmap Frame() {
		return m_frame;
	}

	//Takes in a float representing where in the video (in milliseconds) a frame should be pulled from.
	public boolean GetFrameAtTime( String _fName, float _msec ) {
		long uSec = (long)_msec * 1000L;
		MediaMetadataRetriever mRet = new MediaMetadataRetriever();
		mRet.setDataSource(_fName);

		try {
			m_frame = mRet.getFrameAtTime((long)uSec, MediaMetadataRetriever.OPTION_CLOSEST);
			return true;
		}
		catch (Exception e) {
			Log.e("VideoHandler", "Error grabbing frame at: " + uSec + "uSecs. Message: " + e.getMessage());
			return false;
		}
	}
}
