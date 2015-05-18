﻿using UnityEngine;
using System.Collections;

public class ColumnAnchor : MonoBehaviour {

	private float m_Velocity = 0.0f;
	private float m_Aceleration = 0.0f;
	private float m_BottomPoint;

	void Update() {
		if ( Pos.x < -2.0f || Pos.x > 2.0f ) {
			m_Velocity = m_Aceleration = 0.0f;
		}
	}

	public float Velocity {
		get { return m_Velocity; }
		set { m_Velocity = value; }
	}

	public float Acceleration {
		get { return m_Aceleration; }
		set { m_Aceleration = value; }
	}

	public float BottomPoint {
		get { return m_BottomPoint; }
		set { m_BottomPoint = value; }
	}

	public Vector3 Pos {
		get { return this.transform.position; }
		set { this.transform.position = value; }
	}

	public Vector3 LocalPos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}

	public float LocalY {
		get { return LocalPos.y; }
		set { Vector3 loc = LocalPos;
				loc.y = value;
				LocalPos = loc; }
	}
}
