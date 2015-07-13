using UnityEngine;
using System.Collections.Generic;

public class ProceduralMesh : MonoBehaviour {
	private static int numVertices;
	private static int numIndices;

	private static Vector3[] vertices;
	private static Vector2[] texCoords;
	private static Vector3[] normals;
	private static int[] indices;

	private struct Quad {
		public List<Vector3> Vertices;
		public List<Vector2> UVs;
		public List<Vector3> Normals;
		public int[] Indices;
	};

	public static Mesh GenerateCurvedCylinder( int _height, int _sides ) {
		float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)(_height - 2);
		float sideInc = 1.0f / (float)(_sides - 1);
		float degInc = 360.0f / ( (float)_sides - 1 );
		float bulgeInc = 90.0f / (float)_height - 1;
		float bulge = 15 * bulgeInc;
		float deg = 0.0f;
		float rad = 0.0f;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );
				rad = Mathf.Sin( Mathf.Deg2Rad * bulge );

				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = centrePos + unitPos * rad;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc);
				normals[i * _sides + j] = Vector3.Normalize( centrePos + unitPos * rad );
				deg += degInc;
			}
			deg = 0.0f;
		}

		int offset = 0;

		for ( int i = 1; i < _height - 1; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;
		ret.normals = normals;
		ret.RecalculateBounds();
		ret.RecalculateNormals();


		float diff = Time.realtimeSinceStartup - startTime;
		Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Verticies: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Vector3[] GenerateCurvedCylinderVerts( int _height, int _sides ) {
		float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;


		vertices = new Vector3[numVertices];

		float heightInc = 1.0f / (float)_height;
		float sideInc = 1.0f / ( (float)_sides - 1 );
		float height = -heightInc;
		float degInc = 360.0f / ( (float)_sides - 1 );
		float bulgeInc = 90.0f / (float)_height - 1;
		float bulge = 15 * bulgeInc;
		float deg = 0.0f;
		float rad = 0.0f;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height - 1; ++i ) {
			height += heightInc;
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );
				rad = Mathf.Sin( Mathf.Deg2Rad * bulge );

				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = centrePos + unitPos * rad;
		
				deg += degInc;
			}
			deg = 0.0f;
		}
		return vertices;
	}

	public static Mesh GenerateCylinder( int _height, int _sides ) {
		float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)_height;
		float sideInc = 1.0f / (float)_sides;
		float height = -heightInc;
		float degInc = 360.0f / ( (float)_sides - 1 );
		float deg = 0.0f;

		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			height += heightInc;
			for ( int j = 0; j < _sides; ++j ) {
				unitPos.y = i;
				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = unitPos;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc );
				deg += degInc;
			}					
			deg = 0.0f;
		}
		
		int offset = 0;
		
		for ( int i = 1; i < _height; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;

		ret.RecalculateBounds();
		ret.RecalculateNormals();

		float diff = Time.realtimeSinceStartup - startTime;
		Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Vertex count: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GenerateCup( int _height, int _sides ) {
		float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)_height;
		float sideInc = 1.0f / ( (float)_sides - 1 );
		float height = -heightInc;
		float degInc = 360.0f / ( (float)_sides - 1 );
		float bulgeInc = 45.0f / (float)_height - 1;
		float bulge = 0.0f;
		float deg = 0.0f;
		float rad = 0.0f;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			height += heightInc;
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );
				rad = Mathf.Sin( Mathf.Deg2Rad * bulge );

				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = centrePos + unitPos * rad;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc );
				normals[i * _sides + j] = Vector3.Normalize( centrePos + unitPos * rad );
				deg += degInc;
			}
			deg = 0.0f;
		}

		int offset = 0;

		for ( int i = 1; i < _height; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;
		ret.normals = normals;
		ret.RecalculateBounds();

		float diff = Time.realtimeSinceStartup - startTime;
		Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Vertex count: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GeneratePlane( int _height, int _width ) {
		float startTime = Time.realtimeSinceStartup;
		

		numVertices = _width * _height;
		numIndices = 6 * ( _width - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / ( (float)_height - 1);
		float sideInc = 1.0f / ( (float)_width - 1);

		float xInc = 1.0f / _height;
		float yInc = 1.0f / _width;

		float halfHeight = (_height - 1) / 2;
		float halfWidth = (_width - 1) /2;

		for ( int i = 0; i < _height; ++i ) {
			for ( int j = 0; j < _width; ++j ) {
				vertices[i * _width + j] = new Vector3( -(j - halfWidth) * xInc, (i - halfHeight) * yInc, 0.0f );
				texCoords[i * _width + j] = new Vector2( j * sideInc, i * heightInc );
				normals[i * _width + j] = Vector3.back;
			}
		}

		int offset = 0;

		for ( int i = 1; i < _height; ++i ) {
			for ( int j = 0; j < _width - 1; ++j ) {
				int a = i * _width + j;
				int b = ( i - 1 ) * _width + j;
				int c = ( i - 1 ) * _width + j + 1;
				int d = i * _width + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;

		ret.RecalculateBounds();
		ret.RecalculateNormals();

		float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Plane mesh generated in " + diff.ToString() + " seconds. Vertex count: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GenerateCurvedCylinderSegment( int _height, int _sides, float _arc, float _multi ) {
		Debug.Log(_multi.ToString());
		float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)( _height - 2 );
		float sideInc = 1.0f / (float)( _sides - 1 );
		float degInc = (_arc / _multi) / ( (float)_sides - 1 );
		float bulgeInc = 90.0f / (float)_height;
		float bulge = (_height / 2.0f) * bulgeInc;
		float deg = 0.0f;
		float rad = 0.0f;

		float halfHeight = ( _height - 1 ) / 2;
		float halfWidth = ( _sides - 1 ) / 2;

		float xInc = 1.0f / _height;
		float yInc = 1.0f / _sides;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );
				rad = Mathf.Sin( Mathf.Deg2Rad * bulge ); //Defines vertical bulge

				//unitPos = new Vector3( ( j - halfWidth ) * xInc, ( i - halfHeight ) * yInc, 0.0f );

				unitPos.x += Mathf.Cos( Mathf.Deg2Rad * deg) * _multi;
				unitPos.z += Mathf.Sin( Mathf.Deg2Rad * deg) / _multi;

				vertices[i * _sides + j] = centrePos + unitPos * rad;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc );
				normals[i * _sides + j] = Vector3.Normalize( centrePos + unitPos * rad );
				deg += degInc;
			}
			deg = 0.0f;
		}

		int offset = 0;

		for ( int i = 1; i < _height - 1; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;
		ret.normals = normals;
		ret.RecalculateBounds();
		ret.RecalculateNormals();


		float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Verticies: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GenerateQuad() {
		Quad quadVals = MakeQuad();
		Mesh ret = new Mesh();

		ret.vertices = quadVals.Vertices.ToArray();
		ret.uv = quadVals.UVs.ToArray();
		ret.normals = quadVals.Normals.ToArray();
		ret.triangles = quadVals.Indices;
		ret.RecalculateBounds();
		ret.RecalculateNormals();

		return ret;
	}

	private static Quad MakeQuad() {
		Quad ret = new Quad();

		ret.Vertices = new List<Vector3>();
		ret.UVs = new List<Vector2>();
		ret.Normals = new List<Vector3>();

		ret.Vertices.Add( new Vector3( 0.0f, 0.0f, 0.0f ) );
		ret.UVs.Add( Vector2.zero );
		ret.Normals.Add( Vector3.up );

		ret.Vertices.Add( new Vector3( 0.0f, 0.0f, 1.0f ) );
		ret.UVs.Add( new Vector2( 0.0f, 1.0f ) );
		ret.Normals.Add( Vector3.up );

		ret.Vertices.Add( new Vector3( 1.0f, 0.0f, 1.0f ) );
		ret.UVs.Add( new Vector2( 1.0f, 1.0f ) );
		ret.Normals.Add( Vector3.up );

		ret.Vertices.Add( new Vector3( 1.0f, 0.0f, 0.0f ) );
		ret.UVs.Add( new Vector2( 1.0f, 0.0f ) );
		ret.Normals.Add( Vector3.up );

		ret.Indices = new int[] { 0, 1, 2, 0, 2, 3 };

		return ret;
	}

	private static Quad MakeQuad( Vector3 _size ) {
		Quad ret = new Quad();

		ret.Vertices = new List<Vector3>();
		ret.UVs = new List<Vector2>();
		ret.Normals = new List<Vector3>();

		ret.Vertices.Add( new Vector3( 0.0f, 0.0f, 0.0f ) );
		ret.UVs.Add( Vector2.zero );
		ret.Normals.Add( Vector3.up );

		ret.Vertices.Add( new Vector3( 0.0f, 0.0f, 1.0f ) );
		ret.UVs.Add( new Vector2( 0.0f, 1.0f ) );
		ret.Normals.Add( Vector3.up );

		ret.Vertices.Add( new Vector3( 1.0f, 0.0f, 1.0f ) );
		ret.UVs.Add( new Vector2( 1.0f, 1.0f ) );
		ret.Normals.Add( Vector3.up );

		ret.Vertices.Add( new Vector3( 1.0f, 0.0f, 0.0f ) );
		ret.UVs.Add( new Vector2( 1.0f, 0.0f ) );
		ret.Normals.Add( Vector3.up );

		ret.Indices = new int[] { 0, 1, 2, 0, 2, 3 };

		return ret;
	}
}
