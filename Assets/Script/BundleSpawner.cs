using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleSpawner : MonoBehaviour
{
    public GameObject[] boomPrefabs; // Prefab for the bundle item
    public GameObject[] bundlePrefabs; // Prefab for the bundle item
    public ArrayList BundleList;    // List of bundle items
    public ArrayList RowBoomList;    // List of bundle items to be removed
    public ArrayList ColumnBoomList; // List of bundle items to be removed
    public ArrayList BigBoomList; // List of bundle items to be removed
    public ArrayList SuperBoomList; // List of bundle items to be removed
    private Transform m_bundleRoot; // Root transform for the bundle items
    private ArrayList m_matchBundles; // List of matched bundle items
    public int boomCount = 0;
    private void Awake()
    {
        m_bundleRoot = transform;
        RowBoomList = new ArrayList(); // Initialize RowBoomList
        SuperBoomList = new ArrayList(); // Initialize SuperBoomList
        ColumnBoomList = new ArrayList();
    }

    private void SetBundleItem(int rowIndex, int columnIndex, BundleItem item)
    {
        var temp = BundleList[rowIndex] as ArrayList; // Get the list of bundle items for the current row
        temp[columnIndex] = item; // Set the bundle item at the current position
    }

    //<————————初始化，生成随机Bundle—————————>
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
        var temp = BundleList[rowIndex] as ArrayList; // Get the list of bundle items for the current row
        return temp[columnIndex] as BundleItem;
    }

    //<————————匹配行四消及以上，确定行炸弹和超级炸弹位置————————>
    private bool CheckRow4_5Match()
    {
        Debug.Log("Checking for row matches...");
        bool foundMatch = false;

        for (int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++)
        {
            int continuousCount = 0; // Used to count the number of consecutive identical items
            int startColumnIndex = 0;
            int endColumnIndex = 0;

            BundleItem firstItem = null;
            for (int columnIndex = 0; columnIndex < GlobalDef.ColumnCount; columnIndex++)
            {
                BundleItem currentItem = GetBundleItem(rowIndex, columnIndex);

                if (currentItem != null && (firstItem == null || firstItem.bundleColor == currentItem.bundleColor))
                {
                    continuousCount++;
                    if (firstItem == null) {
                        firstItem = currentItem;
                        startColumnIndex = columnIndex;
                    }
                    endColumnIndex = columnIndex;
                }
                else
                {
                    // Check the size of the match collected
                    if (continuousCount >= 4)
                    {
                        foundMatch = true;
                        // 确定匹配类型并存储相关位置
                        if (continuousCount == 4)
                        {
                            Debug.Log("RowBoomList: " + rowIndex + " " + (startColumnIndex + 1));
                            RowBoomList.Add(new Vector2(rowIndex, startColumnIndex + 1));
                        }
                        else // 5及以上
                        {
                            Debug.Log("SuperBoomList: " + rowIndex + " " + (startColumnIndex + continuousCount / 2));   
                            SuperBoomList.Add(new Vector2(rowIndex, startColumnIndex + continuousCount / 2));
                        }
                    }
                    // Reset for the next possible match
                    continuousCount = 0;
                    firstItem = null;
                }
            }
        }

        return foundMatch;
    }

    //<————————匹配列四消及以上，确定列炸弹和超级炸弹位置——————————>
    private bool CheckColumn4_5Match()
    {
        Debug.Log("Checking for column matches...");
        bool foundMatch = false;

        for (int columnIndex = 0; columnIndex < GlobalDef.ColumnCount; columnIndex++)
        {
            int continuousCount = 0; // Used to count the number of consecutive identical items
            int startRowIndex = 0;
            int endRowIndex = 0;

            BundleItem firstItem = null;
            for (int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++)
            {
                BundleItem currentItem = GetBundleItem(rowIndex, columnIndex);

                if (currentItem != null && (firstItem == null || firstItem.bundleColor == currentItem.bundleColor))
                {
                    continuousCount++;
                    if (firstItem == null)
                    {
                        firstItem = currentItem;
                        startRowIndex = rowIndex;
                    }
                    endRowIndex = rowIndex;
                }
                else
                {
                    // Check the size of the match collected
                    if (continuousCount >= 4)
                    {
                        foundMatch = true;
                        // Determine match type and store relevant positions
                        if (continuousCount == 4)
                        {
                            Debug.Log("ColumnBoomList: " + (startRowIndex + 1) + " " + columnIndex);
                            ColumnBoomList.Add(new Vector2(startRowIndex + 1, columnIndex));
                        }
                        else // 5及以上
                        {
                            Debug.Log("SuperBoomList: " + (startRowIndex + continuousCount / 2) + " " + columnIndex);
                            SuperBoomList.Add(new Vector2(startRowIndex + continuousCount / 2, columnIndex));
                        }
                    }
                    // Reset for the next possible match
                    continuousCount = 0;
                    firstItem = null;
                }
            }
        }

        return foundMatch;
    }

    //<————————匹配三消——————————>
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

    private void RemoveMatchBundles() 
    {
       foreach (BundleItem item in new ArrayList(m_matchBundles)) // Create a copy of the original list for iteration
       {
           if (item != null) {
               item.DestroyBundle();
           }
       }
    }

    IEnumerator CheckMatch() // Check for matches in the grid
    {
        CheckRow4_5Match(); 
        CheckColumn4_5Match();
        boomCount = RowBoomList.Count + ColumnBoomList.Count + SuperBoomList.Count; // Count the number of matches found
        bool foundMatch = CheckXMatch() | CheckYMatch();
        if (foundMatch)
        {
            RemoveMatchBundles();
            FillBoomsIfNeeded();
            yield return new WaitForSeconds(0.2f);
            DropDownOtherBundles();
            m_matchBundles = new ArrayList(); // Clear the match bundles list
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(AutoMatchAgain());
        }
        m_matchBundles.Clear(); // Clear list after checking
    }

    //<————————生成炸弹——————————>
    private void FillBoomsIfNeeded()
    {
        Debug.Log("Filling booms...");

        // Handle RowBoom
        if (RowBoomList == null)
        {
            Debug.Log("RowBoomList is null.");
            return;
        }
        foreach (Vector2 pos in RowBoomList)
        {
            Debug.Log($"Placing row boom at: {pos}");
            PlaceBoomAt((int)pos.x, (int)pos.y, 1);
        }

        // Handle SuperBoom
        if (SuperBoomList == null)
        {
            Debug.Log("SuperBoomList is null.");
        return;
    }
    foreach (Vector2 pos in SuperBoomList)
    {
        Debug.Log($"Placing super boom at: {pos}");
        PlaceBoomAt((int)pos.x, (int)pos.y, 2);
    }

    // Handle ColumnBoom
    if (ColumnBoomList == null)
    {
        Debug.Log("ColumnBoomList is null.");
        return;
    }
    foreach (Vector2 pos in ColumnBoomList)
    {
        Debug.Log($"Placing column boom at: {pos}");
        PlaceBoomAt((int)pos.x, (int)pos.y, 3); // Assuming 3 is the type for column bombs
    }

    // Clear lists after processing
    RowBoomList.Clear();
    SuperBoomList.Clear();
    ColumnBoomList.Clear();

    // Drop down other bundles to fill the gaps
    DropDownOtherBundles();
}

    private void PlaceBoomAt(int rowIndex, int columnIndex, int boomType)
    {
        if (rowIndex < 0 || rowIndex >= GlobalDef.RowCount || columnIndex < 0 || columnIndex >= GlobalDef.ColumnCount)
        {
            Debug.LogWarning($"Position ({rowIndex}, {columnIndex}) is out of bounds.");
            return;
        }

        BundleItem item = GetBundleItem(rowIndex, columnIndex);
        if (item != null)
        {
            Destroy(item.gameObject); // Remove the existing BundleItem
        }

        item = AddBoomBundle(rowIndex, columnIndex, boomType);
        if (item != null)
        {
            SetBundleItem(rowIndex, columnIndex, item);
        }
        else
        {
            Debug.Log("Failed to create BoomBundle.");
        }
    }

    private BundleItem AddBoomBundle(int rowIndex, int columnIndex, int boomType)
    {
        GameObject boomprefab = boomPrefabs[boomType];
        Debug.Log($"Creating BoomBundle of type {boomType} at ({rowIndex}, {columnIndex})");
        var item = new GameObject("Boom");
        item.transform.SetParent(m_bundleRoot, false);
        item.AddComponent<BoxCollider2D>().size = Vector2.one * GlobalDef.CellSize;
        var bhv = item.AddComponent<BundleItem>();
        bhv.UpdatePosition(rowIndex, columnIndex);
        bhv.CreateBoomBG(boomType, boomprefab);
        return bhv;
    }





// <————————下落剩余Bundle——————————>
    private void DropDownOtherBundles() // Drop down the other bundles in the grid
    {
        for (int i = 0; i < m_matchBundles.Count; i++)
        {
            var item = m_matchBundles[i] as BundleItem; // Get the bundle item from the match bundles list
            if(IsBombAt(item.rowIndex, item.columnIndex)) // Check if the item is a bomb
            {
                Debug.Log("Item is a bomb, skipping...");
                continue; // Skip the item if it is a bomb
            }
            for (int j = item.rowIndex + 1; j < GlobalDef.RowCount; j++)
            {
                var temp = GetBundleItem(j, item.columnIndex); // Get the bundle item below
                temp.rowIndex--; // Decrease the row index of the item below
                SetBundleItem(temp.rowIndex, temp.columnIndex, temp); // Set the bundle item to the new position
                temp.UpdatePosition(temp.rowIndex, temp.columnIndex, true);
            }
            ReuseRemovedBundles(item); // Reuse the removed bundle item
        }
        m_matchBundles.Clear(); // Clear the match bundles list 
    }




    private bool IsBombAt(int rowIndex, int columnIndex)
    {
        // Check if there is a bomb at the given position
        foreach (Vector2 pos in RowBoomList)
        {
            if (pos.x == rowIndex && pos.y == columnIndex)
            {
                return true;
            }
        }
        foreach (Vector2 pos in ColumnBoomList)
        {
            if (pos.x == rowIndex && pos.y == columnIndex)
            {
                return true;
            }
        }
        foreach (Vector2 pos in SuperBoomList)
        {
            if (pos.x == rowIndex && pos.y == columnIndex)
            {
                return true;
            }
        }
        // foreach (Vector2 pos in BigBoomList)
        // {
        //     if (pos.x == rowIndex && pos.y == columnIndex)
        //     {
        //         return true;
        //     }
        // }
        return false;
    }

    private void ReuseRemovedBundles(BundleItem bundle) // Reuse the removed bundles
    {
        var color = Random.Range(0, bundlePrefabs.Length); // Randomly select a color for the bundle item
        bundle.rowIndex = GlobalDef.RowCount; // Set the row index of the bundle item to the last row
        bundle.CreateBundleBg(color, bundlePrefabs[color]); // Create the background for the bundle item
        bundle.UpdatePosition(bundle.rowIndex, bundle.columnIndex); // Update the position of the bundle item in the grid
        bundle.rowIndex--;
        SetBundleItem(bundle.rowIndex, bundle.columnIndex, bundle); // Set the bundle item to the new position
        bundle.UpdatePosition(bundle.rowIndex, bundle.columnIndex,true); // Update the position of the bundle item in the grid
    }

    IEnumerator AutoMatchAgain(){
        if(CheckXMatch() | CheckYMatch()){
            RemoveMatchBundles();
            FillBoomsIfNeeded();
            yield return new WaitForSeconds(0.2f);
            DropDownOtherBundles();
            m_matchBundles = new ArrayList(); // Clear the match bundles list
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(AutoMatchAgain()); // Start the coroutine again to check for matches
        }
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
