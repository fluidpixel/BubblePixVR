using UnityEngine;
using System.Collections;

public class CurvedCylinder : MonoBehaviour {

	[SerializeField]
	private MeshFilter m_Mesh = null;

	[SerializeField]
	private int m_Height = 20;

	[SerializeField]
	private int m_Sides = 40;

	void Start() {
		m_Mesh.mesh = ProceduralMesh.GenerateCurvedCylinder(m_Height, m_Sides);
	}
}
