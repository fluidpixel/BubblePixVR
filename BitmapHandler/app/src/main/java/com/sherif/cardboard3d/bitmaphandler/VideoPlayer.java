package com.sherif.cardboard3d.bitmaphandler;

import android.content.Context;
import android.graphics.SurfaceTexture;
import android.media.MediaMetadataRetriever;
import android.media.MediaPlayer;
import android.net.Uri;
import android.util.Log;
import android.view.Surface;

import java.io.IOException;

/**
 * Created by Sherif on 04-Jun-15.
 */
public class VideoPlayer implements SurfaceTexture.OnFrameAvailableListener, MediaPlayer.OnPreparedListener {

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
	public boolean IsReady() {
		return m_Ready;
	}

	public VideoPlayer(String _path, Context _cont) {
		m_MediaPlayer = new MediaPlayer();
		MediaMetadataRetriever mdr = new MediaMetadataRetriever();
		mdr.setDataSource(_path);
		m_Height = Integer.parseInt(mdr.extractMetadata(MediaMetadataRetriever.METADATA_KEY_VIDEO_HEIGHT));
		m_Width = Integer.parseInt(mdr.extractMetadata(MediaMetadataRetriever.METADATA_KEY_VIDEO_WIDTH));
		m_Length = Integer.parseInt(mdr.extractMetadata(MediaMetadataRetriever.METADATA_KEY_DURATION));
		mdr.release();
		mdr = null;

		try {
			Uri uri = Uri.parse(_path);
			Log.i(TAG, uri.getPath());
			m_MediaPlayer.setDataSource(_cont, uri);
			m_MediaPlayer.setOnPreparedListener(this);
		}
		catch (IOException e) {
			Log.e(TAG, "Error setting data source: " + e.getMessage());
		}
		m_Ready = true;
	}

	public void PrepareVideo(int _texPtr) {
		try {
			m_SurfTex = new SurfaceTexture(_texPtr);
			m_SurfTex.setOnFrameAvailableListener(this);
			Log.i(TAG, "Surface Texture ready");
			Surface surface = new Surface(m_SurfTex);
			m_MediaPlayer.setSurface(surface);
			Log.i(TAG, "Surface Ready");
			surface.release();

			m_MediaPlayer.prepare();
		}
		catch (IOException | IllegalStateException e) {
			Log.e(TAG, "Error preparing player: " + e.getMessage());
			m_Ready = false;
		}
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
		m_MediaPlayer = null;
	}

	public void onFrameAvailable(SurfaceTexture _surfaceTex) {
		m_SurfTex.updateTexImage();
		_surfaceTex.updateTexImage();
	}

	public void onPrepared(MediaPlayer _mediaPlayer) {
		Log.i(TAG, "Media player ready");
		StartVideo();
	}
}
