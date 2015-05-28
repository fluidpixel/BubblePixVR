package com.sherif.cardboard3d.bitmaphandler;

import android.graphics.Bitmap;
import android.media.MediaMetadataRetriever;
import android.util.Log;

import java.io.ByteArrayOutputStream;

/**
 * Created by Sherif Salem on 27/5/2015.
 * Takes in the path for the video file to be decoded, can then return a frame at a given time in
 * the video. Will store information related to the video file so that unity can access this.
 */

public class VideoHandler {

	private MediaMetadataRetriever m_MetaData;
	private String m_Path;
	private int m_Width;
	private int m_Height;
	private long m_Length;
	private String m_FileName;

	public int Width() {
		return m_Width;
	}

	public int Height() {
		return m_Height;
	}

	public long Length() {
		return m_Length;
	}

	public String FileName() {
		return m_FileName;
	}

	public VideoHandler(String _path) {
		try {
			m_MetaData = new MediaMetadataRetriever();
			m_MetaData.setDataSource(_path);
			m_Height = Integer.parseInt(m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_VIDEO_HEIGHT));
			m_Width = Integer.parseInt(m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_VIDEO_WIDTH));
			m_Length = Long.parseLong(m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_DURATION));
			m_FileName = m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_TITLE);
		}
		catch (Exception e) {
			m_Width = m_Height = -1;
			m_Length = -1L;
			m_FileName = "";
			Log.e("VideoHandler", "Error initialising. Message: " + e.getMessage());
		}
	}

	//Takes in a float representing where in the video (in milliseconds) a frame should be pulled from.
	public byte[] GetFrameAtTime( float _msec ) {
		long uSec = (long)_msec * 1000L;
		byte[] outBytes;
		try {
			Bitmap bm = m_MetaData.getFrameAtTime(uSec, MediaMetadataRetriever.OPTION_CLOSEST);

			ByteArrayOutputStream out = new ByteArrayOutputStream();

			bm.compress(Bitmap.CompressFormat.JPEG, 100, out);

			outBytes = out.toByteArray();
			out.flush();
			out.close();
			return outBytes;
		}
		catch (Exception e) {
			Log.e("VideoHandler", "Error grabbing frame at: " + uSec + "uSecs. Message: " + e.getMessage());
			return null;
		}
	}
}
