using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceOutlineHelper {
	private static Material outlineMaterial;
	public static GameObject GenerateMeshOutline(GameObject piece) {
		var outlineGameObject = new GameObject();
		outlineGameObject.transform.parent = piece.transform;
		outlineGameObject.transform.localPosition = new Vector3(-0.5f,-0.5f,-0.05f);
		var meshFilter = outlineGameObject.AddComponent<MeshFilter>();
		var meshRenderer = outlineGameObject.AddComponent<MeshRenderer>();
		if (outlineMaterial == null) {
			outlineMaterial = (Material)Resources.Load ("OutlineMaterial");
		}
		meshRenderer.material = outlineMaterial;
		var mesh = new Mesh();
		meshFilter.mesh = mesh;

		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var normals = new List<Vector3>();
		var uvs = new List<Vector2>();
		var borderWidth = 0.05f;
		var width = piece.transform.localScale.x;
		var height = piece.transform.localScale.y;
		CreateQuad(new Rect(0,			0,			width+borderWidth,			borderWidth), vertices, triangles, normals, uvs, mesh);
		CreateQuad(new Rect(width,		0,			borderWidth,				height+borderWidth), vertices, triangles, normals, uvs, mesh);
		CreateQuad(new Rect(0,			height,		width+borderWidth,			borderWidth), 	vertices, triangles, normals, uvs, mesh);
		CreateQuad(new Rect(0,			0,			borderWidth,				height+borderWidth), vertices, triangles, normals, uvs, mesh);
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		return outlineGameObject;
	}

	private static void CreateQuad(Rect rect,List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs, Mesh mesh) {
		float width = 1;
		float height = 1;
		vertices.Add(new Vector3(rect.x,			rect.y,				0));
		vertices.Add(new Vector3(rect.x+rect.width,	rect.y,				0));
		vertices.Add(new Vector3(rect.x,			rect.y+rect.height,	0));
		vertices.Add(new Vector3(rect.x+rect.width,	rect.y+rect.height,	0));

		mesh.vertices = vertices.ToArray();

		var verticeMaxIndex = vertices.Count-1;
		triangles.Add(verticeMaxIndex-3);
		triangles.Add(verticeMaxIndex-1);
		triangles.Add(verticeMaxIndex-2);

		triangles.Add(verticeMaxIndex-1);
		triangles.Add(verticeMaxIndex);
		triangles.Add(verticeMaxIndex-2);

		normals.Add(Vector3.forward);
		normals.Add(-Vector3.forward);
		normals.Add(-Vector3.forward);
		normals.Add(-Vector3.forward);

		uvs.Add(new Vector2(0, 0));
		uvs.Add(new Vector2(1, 0));
		uvs.Add(new Vector2(0, 1));
		uvs.Add(new Vector2(1, 1));
	}
}
