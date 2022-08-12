using UnityEngine;
using System.Collections;

public static class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve) {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		MeshData meshData = new MeshData (width, height);
		int vertexIndex = 0;

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier, topLeftZ - y);
				meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + width + 1, vertexIndex + width);
					meshData.AddTriangle (vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}

	public static MeshData GenerateCell(Vector2 pos, MeshData islandMeshData)
    {
		MeshData meshData = new MeshData(2, 2);
		int[] triangles = new int[6];
		int j = (int)(pos.y * (IslandGenerator.mapChunkSize-1) + pos.x) * 6;
		Vector3[] vertices = new Vector3[6];
		for (int i = 0; i < 6; i++) {
			vertices[i] = islandMeshData.vertices[islandMeshData.triangles[j]] + new Vector3(0, 0.1f, 0);
			j++;
		}

		meshData.vertices = new Vector3[] { vertices[0], vertices[5], vertices[2], vertices[1] };
		meshData.AddTriangle(0, 3, 2);
		meshData.AddTriangle(3, 0, 1);
		meshData.uvs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };

		return meshData;
	}

	public static Vector3 GetBuildingPosition(Vector2[] selectedCells, Vector2 hoveredCell, int vertexIndex, MeshData islandMeshData)
    {
		float heightSum = 0;
		foreach(Vector2 cell in selectedCells)
        {
			Vector3[] cellVertices = GetCellVertices(hoveredCell, islandMeshData);
			heightSum += (cellVertices[0].y + cellVertices[1].y + cellVertices[2].y + cellVertices[3].y) / 4;
		}
		Vector3 vertex = GetCellVertices(hoveredCell, islandMeshData)[vertexIndex];
		return new Vector3(vertex.x, heightSum / selectedCells.Length, vertex.z);
    }

	public static void GetCropPositions(Vector2[] cells, MeshData islandMeshData, out Vector3[] positions)
    {
		positions = new Vector3[cells.Length];
		int index = 0;
		foreach (Vector2 cell in cells)
		{
			Vector3[] newVertices = GetCellVertices(cell, islandMeshData);
			Vector3 center = (newVertices[0] + newVertices[1] + newVertices[2] + newVertices[3]) / 4;
			positions[index] = center + new Vector3(0, -0.1f, 0);
			index++;
		}
	}

	public static void GetFencePositions(Vector2[] cells, MeshData islandMeshData, out Vector3[] positions, out Quaternion[] rotations)
    {
		int minX = IslandGenerator.mapChunkSize, maxX = 0, minY = IslandGenerator.mapChunkSize, maxY = 0;
		foreach (Vector2 cell in cells)
		{
			if (cell.x < minX) minX = (int)cell.x;
			if (cell.x > maxX) maxX = (int)cell.x;

			if (cell.y < minY) minY = (int)cell.y;
			if (cell.y > maxY) maxY = (int)cell.y;
		}
		int rows = maxX - minX;
		int columns = maxY - minY;

		Vector3[] newVertices;
		positions = new Vector3[rows * 2 + columns * 2 + 4];
		rotations = new Quaternion[rows * 2 + columns * 2 + 4];
		int index = 0;
		foreach (Vector2 cell in cells)
		{
			int vertex = 0;
			int orientation = 0;
			bool ok = false;
			if (cell.y == minY && cell.x < maxX)
			{
				vertex = 0;
				orientation = 0;
				ok = true;
			}
			else if (cell.x == maxX && cell.y < maxY)
            {
				vertex = 1;
				orientation = 1;
				ok = true;
			}
			else if (cell.x == minX && cell.y > minY)
			{
				vertex = 2;
				orientation = 3;
				ok = true;
			}
			else if (cell.y == maxY && cell.x > minX)
			{
				vertex = 3;
				orientation = 2;
				ok = true;
			}
			if(ok)
            {
				newVertices = GetCellVertices(cell, islandMeshData);

				positions[index] = newVertices[vertex] + new Vector3(0, -0.1f, 0);
				rotations[index] = Quaternion.Euler(0, 90 * orientation, 0);
				index++;

				if(cell.y == minY && cell.x == maxX)
                {
					positions[index] = newVertices[0] + new Vector3(0, -0.1f, 0);
					rotations[index] = Quaternion.Euler(0, 0, 0);
					index++;
				}
				else if (cell.x == maxX && cell.y == maxY)
                {
					positions[index] = newVertices[1] + new Vector3(0, -0.1f, 0);
					rotations[index] = Quaternion.Euler(0, 90, 0);
					index++;
				}
				else if (cell.x == minX && cell.y == minY)
				{
					positions[index] = newVertices[2] + new Vector3(0, -0.1f, 0);
					rotations[index] = Quaternion.Euler(0, 270, 0);
					index++;
				}
			}
		}

		newVertices = GetCellVertices(new Vector2(minX, maxY), islandMeshData);
		positions[index] = newVertices[3] + new Vector3(0, -0.1f, 0);
	}

	private static Vector3[] GetCellVertices(Vector2 cell, MeshData islandMeshData)
    {
		int j = (int)(cell.y * (IslandGenerator.mapChunkSize - 1) + cell.x) * 6;
		Vector3[] vertices = new Vector3[6];
		for (int i = 0; i < 6; i++)
		{
			vertices[i] = islandMeshData.vertices[islandMeshData.triangles[j]] + new Vector3(0, 0.1f, 0);
			j++;
		}

		return new Vector3[] { vertices[0], vertices[5], vertices[2], vertices[1] };
	}
}

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight) {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}

}