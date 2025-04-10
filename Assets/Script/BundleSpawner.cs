using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleSpawner : MonoBehaviour
{
    public GameObject[] bundlePrefabs; // Prefab for the bundle item
    public ArrayList BundleList;    // List of bundle items
    private Transform m_bundleRoot; // Root transform for the bundle items
    private ArrayList m_matchBundles; // List of matched bundle items
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

    private void AddMatchBundle(BundleItem item)
    {
        if (m_matchBundles == null) { 
           m_matchBundles = new ArrayList(); // Initialize the match bundles list
       }
        if (!m_matchBundles.Contains(item)) { // Correct check here
           m_matchBundles.Add(item);
       } // Add the item to the match bundles list if it is not already present
    }

    private BundleItem GetBundleItem(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= GlobalDef.RowCount) return null; // Check if the row index is out of bounds
        if (columnIndex < 0 || columnIndex >= GlobalDef.ColumnCount) return null; // Check if the column index is out of bounds
        var temp = BundleList[rowIndex] as ArrayList; // Get the listof bundle items for the current row
        return temp[columnIndex] as BundleItem;
    }

    private bool CheckXMatch()
    {
        bool isMatch = false; // Flag to check if there is a match in the X direction  
        for(int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++){
            for(int columnIndex = 0; columnIndex < GlobalDef.ColumnCount; columnIndex++){
                var item1 = GetBundleItem(rowIndex, columnIndex); // Get the bundle item at the current position
                var item2 = GetBundleItem(rowIndex, columnIndex + 1); // Get the bundle item to the right
                var item3 = GetBundleItem(rowIndex, columnIndex + 2); // Get the bundle item two positions to the right
                if (item1 != null && item2 != null && item3 != null) // Check if all three items are not null
                {
                    if (item1.bundleColor == item2.bundleColor && item1.bundleColor == item3.bundleColor) // Check if the colors of the items match
                    {
                        isMatch = true; // Set the match flag to true
                        AddMatchBundle(item1); // Add the first item to the match bundles list
                        AddMatchBundle(item2); // Add the second item to the match bundles list
                        AddMatchBundle(item3); // Add the third item to the match bundles list
                    }
                }
            }
        }
        return isMatch; // Return the match flag
    }

    private bool CheckYMatch()
    {
        bool isMatch = false; // Flag to check if there is a match in the Y direction
        for(int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++){
            for(int columnIndex = 0; columnIndex < GlobalDef.ColumnCount; columnIndex++){
                var item1 = GetBundleItem(rowIndex, columnIndex); // Get the bundle item at the current position
                var item2 = GetBundleItem(rowIndex + 1, columnIndex); // Get the bundle item below
                var item3 = GetBundleItem(rowIndex + 2, columnIndex); // Get the bundle item two positions below
                if (item1 != null && item2 != null && item3 != null) // Check if all three items are not null
                {
                    if (item1.bundleColor == item2.bundleColor && item1.bundleColor == item3.bundleColor) // Check if the colors of the items match
                    {
                        isMatch = true; // Set the match flag to true
                        AddMatchBundle(item1); // Add the first item to the match bundles list
                        AddMatchBundle(item2); // Add the second item to the match bundles list
                        AddMatchBundle(item3); // Add the third item to the match bundles list
                    }
                }
            }
        }   
        return isMatch; // Return the match flag
    }

    // private void RemoveMatchBundles() // Remove the matched bundles from the list
    // {
    //     for (int i = 0; i < m_matchBundles.Count; i++)
    //     {
    //         var item = m_matchBundles[i] as BundleItem; // Get the bundle item from the match bundles list
    //         item.DestroyBundle(); // Destroy the bundle item
    //         Debug.Log("Object destroyed:", item);   // Log the destruction of the item
    //     }
    // }
    private void RemoveMatchBundles() 
   {
       foreach (BundleItem item in new ArrayList(m_matchBundles)) //创建对原始列表的副本进行遍历
       {
           if (item != null) {
               item.DestroyBundle();
               Debug.Log("Object destroyed", item);
           }
       }
       m_matchBundles.Clear(); // 清空所有已处理的项目
   }

    IEnumerator CheckMatch() // Check for matches in the grid
    {
       bool foundMatch = CheckXMatch() | CheckYMatch();
       if(foundMatch){
           RemoveMatchBundles();
           yield return new WaitForSeconds(0.5f);
       }
       m_matchBundles.Clear(); // Clear list after checking
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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check for mouse click
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position in world coordinates
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero); // Perform a raycast to check for collisions
            if (hit.collider != null) // Check if there was a hit
            {
                StartCoroutine(CheckMatch()); // Start the coroutine to check for matches
            }
        }
    }
}
