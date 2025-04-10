using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleSpawner : MonoBehaviour
{
    public GameObject[] bundlePrefabs; // Prefab for the bundle item
    public ArrayList BundleList;    // List of bundle items
    private Transform m_bundleRoot; // Root transform for the bundle items
    private void Awake()
    {
        m_bundleRoot = transform;   // Get the transform of the bundle root
    }
    private BundleItem AddRandomBundle(int rowIndex, int columnIndex)
    {
        var bundleColor = Random.Range(0, bundlePrefabs.Length); // Randomly select a color for the bundle item
        var item = new GameObject("item");
        item.transform.SetParent(m_bundleRoot, false); // Set the parent of the item to the bundle root
        item.AddComponent<BoxCollider2D>().size = Vector2.one * GlobalDef.CellSize; // Add a box collider to the item
        var bhv = item.AddComponent<BundleItem>(); // Add the BundleItem component to the item
        bhv.UpdatePosition(rowIndex, columnIndex); // Update the position of the item in the grid
        bhv.CreateBundleBg(bundleColor, bundlePrefabs[bundleColor]); // Create the background for the item
        return bhv; // Return the BundleItem component
    }
    private void Start()
    {
        BundleList = new ArrayList(); // Initialize the bundle list
        for (int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++)
        {
            ArrayList temp = new ArrayList(); // Temporary list for the current row
            for (int columnIndex = 0; columnIndex < GlobalDef.ColumnCount; columnIndex++)
            {
                var item = AddRandomBundle(rowIndex, columnIndex); // Add a random bundle item to the list
                temp.Add(item); // Add the item to the temporary list
            }
            BundleList.Add(temp);   // Add the temporary list to the bundle list
        }
    }
}
