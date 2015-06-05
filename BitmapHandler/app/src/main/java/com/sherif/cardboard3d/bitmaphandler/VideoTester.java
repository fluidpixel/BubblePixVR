package com.sherif.cardboard3d.bitmaphandler;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.SurfaceTexture;
import android.media.MediaMetadataRetriever;
import android.opengl.GLES20;
import android.opengl.GLUtils;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.nio.ByteBuffer;

/**
 * Created by Sherif Salem on 27/5/2015.
 * Takes in the path for the video file to be decoded, can then return a frame at a given time in
 * the video. Will store information related to the video file so that unity can access this.
 */

public class VideoTester {

	private MediaMetadataRetriever m_MetaData;
	private int m_Width;
	private int m_Height;
	private long m_Length;
	private String m_FileName;
	private int m_Size;

	private byte[][] m_Frames;

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

	public int BitmapSize() {
		return m_Size;
	}

	public byte[][] ByteVideo() {
		return m_Frames;
	}

	public VideoTester(String _path) {
		try {
			m_MetaData = new MediaMetadataRetriever();
			m_MetaData.setDataSource(_path);
			m_Height = Integer.parseInt(m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_VIDEO_HEIGHT));
			m_Width = Integer.parseInt(m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_VIDEO_WIDTH));
			m_Length = Long.parseLong(m_MetaData.extractMetadata(MediaMetadataRetriever.METADATA_KEY_DURATION));
		}
		catch (Exception e) {
			m_Width = m_Height = -1;
			m_Length = -1L;
			m_FileName = "";
			Log.e("VideoHandler", "Error initialising. Message: " + e.getMessage());
		}
	}

	public VideoTester() {
		m_Width = m_Height = -1;
		m_Length = -1L;
		m_FileName = "";
	}

	//Takes in a float representing where in the video (in milliseconds) a frame should be pulled from.
	public byte[] GetFrameAtTime( long _msec ) {
		long uSec = _msec * 1000L;
		byte[] outBytes;
		try {
			Bitmap bm = m_MetaData.getFrameAtTime(uSec);
			//Log.e("VideoHandler", "Grabbed frame at: " + _msec + "mSecs.");
			ByteArrayOutputStream out = new ByteArrayOutputStream();

			bm.compress(Bitmap.CompressFormat.JPEG, 100, out);
			m_Size = bm.getByteCount();
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

	public byte[][] GetFrames(int _frameRate) {
		long totalFrames = m_Length * 1000 * _frameRate;
		long invFrameRate = 1 / _frameRate;
		Bitmap bm;
		ByteBuffer buffer;
		ByteArrayOutputStream out = new ByteArrayOutputStream();

		byte[][] bytes = new byte[(int)totalFrames][];
		for (long currentFrame = 0L; currentFrame <= totalFrames; ++currentFrame) {
			try {
				bm = m_MetaData.getFrameAtTime(currentFrame * invFrameRate);
				bm.compress(Bitmap.CompressFormat.JPEG, 10, out);
				bytes[(int) currentFrame] = out.toByteArray();
				out.flush();
				bm.recycle();
			}
			catch (Exception e) {
				Log.e("VideoHandler", "Error with compression: " + e.getMessage());
			}
		}
		m_Frames = bytes;
		return bytes;
	}

	public void SetTexture(Context cont, int _texPointer) {

		if (_texPointer != 0) {
			final BitmapFactory.Options options = new BitmapFactory.Options();
			options.inScaled = false;
			options.inJustDecodeBounds = false;
			final Bitmap bitmap = BitmapFactory.decodeFile("/storage/emulated/0/Pictures/black.jpg", options);

			GLES20.glActiveTexture(_texPointer);

			GLUtils.texImage2D(GLES20.GL_TEXTURE_2D, 0, bitmap, 0);

			int err = GLES20.glGetError();

			Log.i("VideoHandler", err == 0 ? "No OpenGL ES Errors" : "Error detected: " + err);

			bitmap.recycle();
		}
		else {
			Log.e("VideoHandler", "Issue loading in texture");
		}
	}
}
