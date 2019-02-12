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


    public void BuildMesh(Vector3 position, float left, float right)
    {
        MeshCollider liquidMeshCollider = GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        moppable = LayerMask.GetMask("Moppable");

        LoadGlobals();
        GetComponent<MeshFilter>().mesh = mesh;

        FillVerticesArray(position, left, right);

        // No vertices could be placed, we're done
        if (verts.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        PlaceDripsAndFixLedges();
        GenerateTriangles();

        mesh.vertices = verts;
        mesh.triangles = triangles;

        // Set collider bounds
        mesh.RecalculateBounds();
        liquidMeshCollider.sharedMesh = mesh;
    }

    private void FillVerticesArray(Vector3 position, float left, float right)
    {
        float width = right + left;
        float leftPosition = position.x - left;
        verts = new Vector3[partitions * 2];

        int deletedVertexCount = 0;

        for (int i = 0; i < partitions; i++)
        {
            int index = i - deletedVertexCount;
            // Each raycast is 1 / partitions more of the total width to the right
            Vector3 raycastPosition = new Vector3(leftPosition + i * (width / partitions), position.y, position.z);
            RaycastHit2D hit = Physics2D.Raycast(raycastPosition, Vector2.down, maxDistanceToGround, moppable);

            if (hit.collider != null)
            {
                // Raycast is inside of collider, try again at the max height of the collider
                if (hit.distance == 0)
                {
                    float colliderMaxY = hit.collider.bounds.max.y;
                    Vector3 newRaycastPosition = new Vector3(raycastPosition.x, colliderMaxY, raycastPosition.z);
                    RaycastHit2D hit2 = Physics2D.Raycast(newRaycastPosition, Vector2.down, maxDistanceToGround, moppable);

                    // We don't place liquid ontop of nearby ledges if they are sufficiently tall
                    if(hit.point.y - hit2.point.y < maxLedgePaint)
                    {
                        deletedVertexCount++;
                        continue;
                    }
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
        partitions -= deletedVertexCount;
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
                Vector3 position = new Vector3(verts[i].x, verts[i].y - liquidDepthBelowGround, verts[i].z);
                SpriteRenderer drips = Instantiate(liquidDrips, position, Quaternion.identity);
                drips.transform.parent = gameObject.transform;
                liquidDrips.sprite = liquidDripSprites[Random.Range(0, liquidDripSprites.Length)];
            }
            prevVert = currVert;
        }
    }

    private void GenerateTriangles()
    {
        triangles = new int[partitions * 2 * 3];

        // Connect vertexes using triangles, drawn like:
        // .___.
        // |  /|
        // | / |
        // |/  |
        // ˙‾‾‾˙
        for (int i = 0; i < partitions - 1; i++)
        {
            // Left triangle
            triangles[i * 6 + 0] = i;
            triangles[i * 6 + 1] = i + partitions;
            triangles[i * 6 + 2] = i + 1;

            // Right triangle
            triangles[i * 6 + 3] = i + 1;
            triangles[i * 6 + 4] = i + partitions;
            triangles[i * 6 + 5] = i + partitions + 1;
        }
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
    }
}
