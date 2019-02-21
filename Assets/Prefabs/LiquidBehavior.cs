using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidBehavior : MonoBehaviour {

    public SpriteRenderer liquidDrips;
    public Sprite[] liquidDripSprites;

    private int partitions;
    private float liquidHeightOffGround;
    private float liquidDepthBelowGround;
    private float maxLedgePaint;
    private float minLedgeHeight;
    private float maxDistanceToGround;
    private LayerMask moppable;

    private Vector3[] verts;
    private int[] triangles;

    private Mesh mesh;
    private MeshCollider liquidMeshCollider;

    private Vector3 center;

    private float rightBounds;
    private float leftBounds;
    private float height;

    private float maxWidth;


    public void Create(Vector3 position)
    {
        liquidMeshCollider = GetComponent<MeshCollider>();
        mesh = new Mesh();
        moppable = LayerMask.GetMask("Moppable");
        LoadGlobals();
        GetComponent<MeshFilter>().mesh = mesh;
        center = position;

        rightBounds = leftBounds = position.x;
        height = position.y;
    }

    public void UpdateMesh(Vector3 position)
    {
        if (position.x > rightBounds)
        {
            rightBounds = position.x;
        }
        else if (position.x < leftBounds)
        {
            leftBounds = position.x;
        }
        else {
            return;
        }

        FillVerticesArray();

        // No vertices could be placed, we're done
        if (verts.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        // PlaceDripsAndFixLedges();
		AddMeshDepth();
        GenerateTriangles();

        mesh.vertices = verts;
        mesh.triangles = triangles;

        // Set collider bounds
        mesh.RecalculateBounds();
        liquidMeshCollider.sharedMesh = mesh;
    }

	private void AddMeshDepth()
	{
		Vector3[] newVerts = new Vector3[verts.Length * 2];

		for (int i = 0; i < verts.Length; i++) {
			newVerts[i] = verts[i];
			newVerts[i].z += 0.01f;
		}
		for (int i = verts.Length; i < verts.Length * 2; i++) {
			newVerts[i] = verts [i - verts.Length];
			newVerts[i].z -= 0.03f;
		}
		verts = newVerts;
	}

    private void FillVerticesArray()
    {
        float width = rightBounds - leftBounds;

        if (width > maxWidth)
            width = maxWidth;

        partitions = (int) (width * 200);

        if (partitions == 0)
            return;

        verts = new Vector3[partitions * 2];

        int deletedVertexCount = 0;

        for (int i = 0; i < partitions; i++)
        {
            int index = i - deletedVertexCount;
            // Each raycast is 1 / partitions more of the total width to the right
            Vector3 raycastPosition = new Vector3(leftBounds + i * (width / partitions), height, center.z);
            RaycastHit2D hit = Physics2D.Raycast(raycastPosition, Vector2.down, maxDistanceToGround, moppable);
            Debug.DrawRay(raycastPosition, Vector2.down, Color.red, maxDistanceToGround);

            if (hit.collider != null)
            {
                // Raycast is inside of collider, try again at the max height of the collider
                if (hit.distance == 0)
                {
                    float colliderMaxY = hit.collider.bounds.max.y;
                    Vector3 newRaycastPosition = new Vector3(raycastPosition.x, colliderMaxY, raycastPosition.z);
                    RaycastHit2D hit2 = Physics2D.Raycast(newRaycastPosition, Vector2.down, maxDistanceToGround, moppable);

                    // We don't place liquid ontop of nearby ledges if they are sufficiently tall
                    if(Mathf.Abs(hit.point.y - hit2.point.y) < maxLedgePaint)
                    {
                        height = colliderMaxY;
                    }
                    deletedVertexCount++;
                    continue;
                }
                verts[index] = new Vector3(hit.point.x, hit.point.y + liquidHeightOffGround);
                // Store lower vertices in second half
                verts[index + partitions] = new Vector3(hit.point.x, hit.point.y - liquidDepthBelowGround);
            }
            else
            {
                // Raycast didn't hit anything, so don't draw these vertices
                deletedVertexCount++;
            }
        }
        RemoveDeletedVertices(deletedVertexCount);
    }

    private void RemoveDeletedVertices(int deletedVertexCount)
    {
        int newVertSize = partitions - deletedVertexCount;
        Vector3[] newVerts = new Vector3[newVertSize * 2];
        // Copy array, shifting the bottom vertices lower in the array (to fit)
        for (int i = 0; i < newVertSize; i++)
        {
            newVerts[i] = verts[i];
            newVerts[i + newVertSize] = verts[i + partitions];
        }
        verts = newVerts;
        // Update partitions for triangle generation
        partitions = newVertSize;

    }

    // This draws vertical liquid on "tall" edges
    private void PlaceDripsAndFixLedges()
    {
        Vector3 prevVert = verts[0];


        for (int i = 1; i < partitions - 1; i++)
        {
            Vector3 currVert = verts[i];
            Vector3 nextVert = verts[i + 1];

            // Check if this vertex is sufficiently low enough to warrent drawing vertical liquid
            if (currVert.y < prevVert.y - minLedgeHeight)
            {
                verts[i] = new Vector3(nextVert.x, prevVert.y, currVert.z);
                verts[i + partitions].x = prevVert.x;
            }

            // Check if this vertex is sufficiently high enough to warrent drawing vertical liquid
            else if (currVert.y > prevVert.y + minLedgeHeight)
            {
                Vector3 prevBottomVert = verts[i - 1 + partitions];
                verts[i] = new Vector3(prevVert.x, currVert.y, currVert.z);
                verts[i + partitions].y = prevBottomVert.y;
                verts[i + partitions].x = nextVert.x;
            }

            // Place drip prefabs
            if (i >  9 && i < partitions - 9 && i % 10 == 0)
            {
				Vector3 position = new Vector3(verts[i].x, verts[i].y - liquidDepthBelowGround - liquidHeightOffGround + 0.01f, verts[i].z);
                SpriteRenderer drips = Instantiate(liquidDrips, position, Quaternion.identity);
                drips.transform.parent = gameObject.transform;
                liquidDrips.sprite = liquidDripSprites[Random.Range(0, liquidDripSprites.Length)];
            }
			prevVert = currVert;
        }
    }

    private void GenerateTriangles()
    {
		triangles = new int[verts.Length * 2 * 3 + 12];

        // Connect vertexes using triangles, e.g.:
        // .___.
        // |  /|
        // | / |
        // |/  |
        // ˙‾‾‾˙

		int tNum = 24;

        for (int i = 0; i < partitions - 1; i++)
        {
            // Don't connect vertices if they're far apart
            if (Mathf.Abs(verts[i].x - verts[i + 1].x) > 0.2f)
                continue;
        
            // Front left triangle
            triangles[i * tNum + 0] = i;
			triangles[i * tNum + 1] = i + partitions;
			triangles[i * tNum + 2] = i + 1;

            // Front right triangle
			triangles[i * tNum + 3] = i + 1;
			triangles[i * tNum + 4] = i + partitions;
			triangles[i * tNum + 5] = i + partitions + 1;

			// Back left triangle
			triangles[i * tNum + 6] = partitions * 2 + i;
			triangles[i * tNum + 7] = partitions * 2 + i + partitions;
			triangles[i * tNum + 8] = partitions * 2 + i + 1;

			// Back right triangle
			triangles[i * tNum + 9] = partitions * 2 + i + 1;
			triangles[i * tNum + 10] = partitions * 2 + i + partitions;
			triangles[i * tNum + 11] = partitions * 2 + i + partitions + 1;

			// Top left triangle
			triangles[i * tNum + 12] = i + 1;
			triangles[i * tNum + 13] = partitions * 2 + i + 1;
			triangles[i * tNum + 14] = partitions * 2 + i;

			// Top right triangle
			triangles[i * tNum + 15] = i;
			triangles[i * tNum + 16] = i + 1;
			triangles[i * tNum + 17] = partitions * 2 + i;

			// Bottom left triangle
			triangles[i * tNum + 18] = i + partitions;
			triangles[i * tNum + 19] = partitions * 2 + i + partitions;
			triangles[i * tNum + 20] = partitions * 2 + i + partitions + 1;

			// Bottom right triangle
			triangles[i * tNum + 21] = i + partitions;
			triangles[i * tNum + 22] = i + partitions + 1;
			triangles[i * tNum + 23] = partitions * 2 + i + partitions + 1;
        }

		// Draw left side triangles
		triangles[triangles.Length - 12] = 0;
		triangles[triangles.Length - 11] = partitions;
		triangles[triangles.Length - 10] = partitions * 2;

		triangles[triangles.Length - 9] = partitions;
		triangles[triangles.Length - 8] = partitions * 2;
		triangles[triangles.Length - 7] = partitions * 2 + partitions;

		// Draw right side triangles
		triangles[triangles.Length - 6] = partitions + 0 - 1;
		triangles[triangles.Length - 5] = partitions + partitions - 1;
		triangles[triangles.Length - 4] = partitions + partitions * 2 - 1;

		triangles[triangles.Length - 3] = partitions + partitions - 1;
		triangles[triangles.Length - 2] = partitions + partitions * 2 - 1;
		triangles[triangles.Length - 1] = partitions + partitions * 2 + partitions - 1;

    }

    private void LoadGlobals()
    {
        // We store globals in LiquidControl so that we can modify them from that object
        partitions = LiquidControl.instance.partitions;
        liquidHeightOffGround = LiquidControl.instance.liquidHeightOffGround;
        liquidDepthBelowGround = LiquidControl.instance.liquidDepthBelowGround;
        maxLedgePaint = LiquidControl.instance.maxLedgePaint;
        minLedgeHeight = LiquidControl.instance.minLedgeHeight;
        maxDistanceToGround = LiquidControl.instance.maxDistanceToGround;
        maxWidth = LiquidControl.instance.maxWidth;
    }
}
