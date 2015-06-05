package com.sherif.cardboard3d.bitmaphandler;

import android.graphics.SurfaceTexture;
import android.media.MediaPlayer;
import android.media.MediaMetadataRetriever;
import android.util.Log;
import android.view.Surface;

import java.io.IOException;

/**
 * Created by Sherif on 04-Jun-15.
 */
public class VideoPlayer implements SurfaceTexture.OnFrameAvailableListener {

	private static final String TAG = "VideoHandler";

	private MediaPlayer m_MediaPlayer;
	private SurfaceTexture m_SurfTex;

	private int m_Height = -1;
	private int m_Width = -1;
	private int m_Length = -1;

	private boolean m_Ready = false;

	public int Height() {
		return m_Height;
	}
	public int Width() {
		return m_Width;
	}
	public int Length() {
		return m_Length;
	}
	public boolean IsPlaying() {
		if (m_Ready)
			return m_MediaPlayer.isPlaying();
		else {
			return m_Ready;
		}
	}

	public VideoPlayer() {
		m_MediaPlayer = new MediaPlayer();
	}

	public boolean PrepareVideo(String _path, int _texPtr) {
		m_SurfTex = new SurfaceTexture(_texPtr);
		m_SurfTex.setOnFrameAvailableListener(this);

		try {
			m_MediaPlayer.setDataSource(_path);
			Surface surface = new Surface(m_SurfTex);
			m_MediaPlayer.setSurface(surface);
			surface.release();

			m_Height = m_MediaPlayer.getVideoHeight();
			m_Width = m_MediaPlayer.getVideoWidth();
			m_Length = m_MediaPlayer.getDuration();

			m_MediaPlayer.prepareAsync();
			m_Ready = true;
		}
		catch (IOException e) {
			Log.e(TAG, "Error loading media from :" + _path + " - Message: " + e.getMessage());
			m_Ready = false;
		}
		return m_Ready;
	}

	public void StartVideo() {
		if (m_Ready)
			m_MediaPlayer.start();
	}

	public void StopVideo() {
		if (m_MediaPlayer.isPlaying())
			m_MediaPlayer.stop();
	}

	public void PauseVideo() {
		if (m_MediaPlayer.isPlaying())
			m_MediaPlayer.pause();
	}

	public void ResumeVideo() {
		if (!m_MediaPlayer.isPlaying())
			m_MediaPlayer.start();
	}

	public void DiscardVideo() {
		if (m_MediaPlayer.isPlaying())
			m_MediaPlayer.stop();

		m_MediaPlayer.release();
	}

	public void onFrameAvailable(SurfaceTexture _surfaceTex) {
		m_SurfTex.updateTexImage();
	}
}
