using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{   
    public GameObject bottomBlockObj; // Prefab for the block to spawn
    // Start is called before the first frame update
    void Start()
    {
        for (int rowIndex = 0; rowIndex < GlobalDef.RowCount; rowIndex++){
            for (int columnIndex = 0; columnIndex < GlobalDef.ColumnCount; columnIndex++){
                var obj = Instantiate(bottomBlockObj); // Instantiate a new block
                obj.transform.SetParent(bottomBlockObj.transform.parent, false); // Set the parent of the block to the block prefab
                obj.transform.localPosition = new Vector3((columnIndex - GlobalDef.ColumnCount / 2f) * GlobalDef.CellSize + GlobalDef.CellSize / 2f, (rowIndex - GlobalDef.RowCount / 2f) * GlobalDef.CellSize + GlobalDef.CellSize / 2f, 0); // Set the position of the block in the grid
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
