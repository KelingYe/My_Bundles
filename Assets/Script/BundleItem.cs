using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class BundleItem : MonoBehaviour
{
    public int rowIndex; // Row index of the item in the grid
    public int columnIndex; // Column index of the item in the grid
    public int bundleColor; // Color of the item in the grid
    public int boomType; // Type of the item in the grid
    public GameObject bundleSpriteObj; // Sprite of the item in the grid
    private Transform m_selfTransform; // Transform of the item in the grid
    private void Awake()
    {
        m_selfTransform = transform; // Get the transform of the item in the grid
    }

    public void CreateBundleBg(int color,GameObject prefab){
        this.bundleColor = color;
        bundleSpriteObj = Instantiate(prefab); // Instantiate the prefab for the item in the grid
        bundleSpriteObj.transform.localScale = Vector3.one*GlobalDef.CellSize; // Set the scale of the item in the grid
        bundleSpriteObj.transform.SetParent(m_selfTransform, false); // Set the parent of the item to the grid
    }

    public void CreateBoomBG(int type, GameObject prefab)
    {
        this.boomType = type;
        bundleSpriteObj = Instantiate(prefab); // Instantiate the prefab for the item in the grid
        bundleSpriteObj.transform.localScale = Vector3.one * GlobalDef.CellSize; // Set the scale of the item in the grid
        bundleSpriteObj.transform.SetParent(m_selfTransform, false); // Set the parent of the item to the grid
    }

    public void UpdatePosition(int rowIndex, int columnIndex, bool dotween = false)
    {
        this.rowIndex = rowIndex;
        this.columnIndex = columnIndex;
        var targetPos = new Vector3((columnIndex - GlobalDef.ColumnCount / 2f) * GlobalDef.CellSize + GlobalDef.CellSize / 4f, (rowIndex - GlobalDef.RowCount / 2f) * GlobalDef.CellSize, 0);
        if (dotween)
        {
            m_selfTransform.DOLocalMove(targetPos, 0.3f); // Move the item to the target position with a tween animation
        }else
        {
            m_selfTransform.localPosition = targetPos; // Set the position of the item in the grid without animation
        }
    }

    public void DestroyBundle(){
        Destroy(bundleSpriteObj); // Destroy the bundle sprite object
        bundleSpriteObj = null; // Set the bundle sprite object to null
        EventDispatcher.instance.DispatchEvent(EventDef.EVENT_BUNDLE_DISAPPEAR, m_selfTransform.position); // Dispatch the event for bundle disappearance
    }
}
