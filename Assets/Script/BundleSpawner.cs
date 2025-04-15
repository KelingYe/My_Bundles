using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间来处理UI元素

public class BundleSpawner : MonoBehaviour
{
    public Button BeginButton; 
    public GameObject[] boomPrefabs; // Prefab for the bundle item
    public GameObject[] bundlePrefabs; // Prefab for the bundle item
    public ArrayList BundleList;    // List of bundle items
    public ArrayList RowBoomList;    // List of bundle items to be removed
    public ArrayList ColumnBoomList; // List of bundle items to be removed
    public ArrayList BigBoomList; // List of bundle items to be removed
    public ArrayList SuperBoomList; // List of bundle items to be removed
    private Transform m_bundleRoot; // Root transform for the bundle items
    private ArrayList m_matchBundles; // List of matched bundle items
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

    //<————————添加炸弹Bundle——————————>
    private BundleItem AddBoomBundle(int rowIndex, int columnIndex, int boomType)
    {
        var item = new GameObject("item");
        item.transform.SetParent(m_bundleRoot, false);
        item.AddComponent<BoxCollider2D>().size = Vector2.one * GlobalDef.CellSize;
        var bhv = item.AddComponent<BundleItem>();
        bhv.UpdatePosition(rowIndex, columnIndex);
        bhv.CreateBoomBG(boomType, boomPrefabs[boomType]); // Create the background for the item
        return bhv; // Return the BundleItem component
    }

    //<————————添加匹配Bundle——————————>
    private void AddMatchBundle(BundleItem item)
    {
        if (m_matchBundles == null) { 
           m_matchBundles = new ArrayList(); // Initialize the match bundles list
       }
        if (!m_matchBundles.Contains(item)) { // Correct check here
           m_matchBundles.Add(item);
       } // Add the item to the match bundles list if it is not already present
    }


    //<————————获取BundleItem——————————>
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
                            var item = AddBoomBundle(rowIndex, startColumnIndex + 1, 0); // Create a new bundle item for the match
                            Debug.Log("RowBoomList: " + rowIndex + " " + (startColumnIndex + 1));
                            RowBoomList.Add(item);
                        }
                        else // 5及以上
                        {
                            var item = AddBoomBundle(rowIndex, startColumnIndex + continuousCount / 2, 2); // Create a new bundle item for the match
                            Debug.Log("SuperBoomList: " + rowIndex + " " + (startColumnIndex + continuousCount / 2));   
                            SuperBoomList.Add(item);
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
                            var item = AddBoomBundle(startRowIndex+1, columnIndex, 1);
                            Debug.Log("ColumnBoomList: " + (startRowIndex + 1) + " " + columnIndex);
                            ColumnBoomList.Add(item);
                        }
                        else // 5及以上
                        {
                            var item = AddBoomBundle(startRowIndex + continuousCount / 2, columnIndex, 3);                                 
                            Debug.Log("SuperBoomList: " + (startRowIndex + continuousCount / 2) + " " + columnIndex);
                            SuperBoomList.Add(item);
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
        foreach (BundleItem item in RowBoomList)
        {
            Debug.Log("Placing row boom at: " + item.rowIndex + ", " + item.columnIndex);
            SetBundleItem(item.rowIndex, item.columnIndex, item); // Set the bundle item at the current position
        }

        // Handle SuperBoom
        if (SuperBoomList == null)
        {
            Debug.Log("SuperBoomList is null.");
        return;
        }
        foreach (BundleItem item in SuperBoomList)
        {
            Debug.Log($"Placing super boom at: {item.rowIndex}, {item.columnIndex}");
            SetBundleItem(item.rowIndex, item.columnIndex, item);
        }
    
        // Handle ColumnBoom
        if (ColumnBoomList == null)
        {
            Debug.Log("ColumnBoomList is null.");
            return;
        }
        foreach (BundleItem item in ColumnBoomList)
        {
            Debug.Log("Placing column boom at: " + item.rowIndex + ", " + item.columnIndex);
            SetBundleItem(item.rowIndex, item.columnIndex, item); // Set the bundle item at the current position    
        }
    
        // Clear lists after processing
        RowBoomList.Clear();
        SuperBoomList.Clear();
        ColumnBoomList.Clear();
    
        // Drop down other bundles to fill the gaps
        // DropDownOtherBundles();
    }


    //<————————重用被移除的Bundle——————————>
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


    // <————————下落剩余Bundle——————————>
    private void DropDownOtherBundles() // Drop down the other bundles in the grid
    {
        Debug.Log("Dropping down other bundles...");
        for (int i = 0; i < m_matchBundles.Count; i++)
        {
            var item = m_matchBundles[i] as BundleItem; // Get the bundle item from the match bundles list
            // Debug.Log($"Dropping down bundle at: {item.rowIndex}, {item.columnIndex}");
            if(item.isBoom) // Check if the item is a bomb
            {
                // Debug.Log("Item is a bomb, skipping...");
                continue; // Skip the item if it is a bomb
            }
            for (int j = item.rowIndex + 1; j < GlobalDef.RowCount; j++)
            {
                var temp = GetBundleItem(j, item.columnIndex); // Get the bundle item below
                // Debug.Log($"Moving bundle down from {temp.rowIndex}, {temp.columnIndex} to {temp.rowIndex - 1}, {temp.columnIndex}");
                temp.rowIndex--; // Decrease the row index of the item below
                SetBundleItem(temp.rowIndex, temp.columnIndex, temp); // Set the bundle item to the new position
                temp.UpdatePosition(temp.rowIndex, temp.columnIndex, true);
            }
            ReuseRemovedBundles(item); // Reuse the removed bundle item
        }
        m_matchBundles.Clear(); // Clear the match bundles list 
    }

    //按照行列输出当前bundleList（坐标，颜色）
    public void PrintBundleList(){
        Debug.Log("Current Bundle List: ");
        for (int  i = GlobalDef.RowCount - 1; i >= 0; i--){
            var temp = BundleList[i] as ArrayList; // Get the list of bundle items for the current row
            for (int j = 0; j < GlobalDef.ColumnCount; j++){
                var item = temp[j] as BundleItem; // Get the bundle item at the current position
                if (item.isBoom){
                    Debug.Log($"Row: {item.rowIndex}, Column: {item.columnIndex}, Type: {item.boomType}");
                }else{
                    Debug.Log($"Row: {item.rowIndex}, Column: {item.columnIndex}, Color: {item.bundleColor}");
                }
            }
        }
        
        Debug.Log("-------------------------------"); 
    }


    //<————————检查匹配，移除匹配的Bundle, 填充炸弹， 掉落其他Bundle——————————>
    IEnumerator CheckMatch() // Check for matches in the grid
    {
        CheckRow4_5Match(); 
        CheckColumn4_5Match();
        bool foundMatch = CheckXMatch() | CheckYMatch();
        if (foundMatch)
        {
            RemoveMatchBundles();
            FillBoomsIfNeeded();
            yield return new WaitForSeconds(0.2f);
            DropDownOtherBundles();
            m_matchBundles = new ArrayList(); // Clear the match bundles list
            yield return new WaitForSeconds(0.4f);
            PrintBundleList(); // Print the current state of the bundle list
            StartCoroutine(AutoMatchAgain());
        }
        m_matchBundles.Clear(); // Clear list after checking
    }

    //

    IEnumerator AutoMatchAgain(){
        CheckRow4_5Match(); 
        CheckColumn4_5Match();
        if(CheckXMatch() | CheckYMatch()){
            RemoveMatchBundles();
            FillBoomsIfNeeded();
            yield return new WaitForSeconds(0.2f);
            DropDownOtherBundles();
            m_matchBundles = new ArrayList(); // Clear the match bundles list
            yield return new WaitForSeconds(0.5f);
            PrintBundleList();
            StartCoroutine(AutoMatchAgain()); // Start the coroutine again to check for matches
        }
    }


    private float lastClickTime = 0f; // Track the last time the button was clicked
    private float debounceInterval = 0.5f; // Debounce interval in seconds

    // Existing methods...
public void OnButtonClick()
{
    float currentTime = Time.time;
    if (currentTime - lastClickTime < debounceInterval)
    {
        // Debug.Log("Button click ignored due to debounce.");
        return; // Ignore the click if it's too soon after the last click
    }

    lastClickTime = currentTime; // Update the last click time
    // Debug.Log("按钮已点击！");
    PrintBundleList();
    // Here add the code to execute when the button is clicked
    for (int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++)
    {
        var temp = BundleList[rowIndex] as ArrayList; // Get the list of bundle items for the current row
        for (int j = 0; j < GlobalDef.ColumnCount; j++)
        {
            var item = temp[j] as BundleItem; // Get the bundle item at the current position
            item.isBoom = false;
            item.bundleColor = Random.Range(0, bundlePrefabs.Length); // Randomly select a color for the bundle item
            item.CreateBundleBg(item.bundleColor, bundlePrefabs[item.bundleColor]); // Update the background of the item
            // Debug.Log($"点击后更换的 Row: {item.rowIndex}, Column: {item.columnIndex}, Color: {item.bundleColor}");
        }
    }
}


    private void Start()
    {
        BeginButton.onClick.AddListener(OnButtonClick); // 给按钮添加点击事件的监听器
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
        PrintBundleList(); // Print the initial state of the bundle list
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
