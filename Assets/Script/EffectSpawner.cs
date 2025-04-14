using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    public GameObject scoreEffectPrefab; // Prefab for the score effect
    private Transform m_effectRoot;
    public GameObject disappearEffectPrefab; // Prefab for the disappear effect
    public Queue<GameObject> m_disappearEffectPool = new Queue<GameObject>(); // Pool for the disappear effect objects
    private Queue<TextMeshPro> m_scoreEffectPool = new Queue<TextMeshPro>();

    public void ShowDisappearEffect(Vector3 pos)
    {
        GameObject Obj;
        if(m_disappearEffectPool.Count > 0)
        {
            Obj = m_disappearEffectPool.Dequeue(); // Get an object from the pool
        }
        else
        {
            Obj = Instantiate(disappearEffectPrefab); // Instantiate a new object if the pool is empty
            Obj.transform.SetParent(m_effectRoot, false); // Set the parent of the object to the effect root
            var bhv = Obj.GetComponent<AnimationEvent>(); // Get the AnimationEvent component from the object
            bhv.aniEventCb += (eventName) => { // Subscribe to the animation event callback
                if (eventName == "finish")
                {
                    Obj.SetActive(false); // Deactivate the object when the animation ends
                    m_disappearEffectPool.Enqueue(Obj); // Enqueue the object back to the pool
                }

            };

        }
        Obj.SetActive(true);
        Obj.transform.position = pos; // Set the position of the object to the specified position 
    }

    private void Awake()
    {
        m_effectRoot = transform; // Get the transform of the effect root
        EventDispatcher.instance.Regist(EventDef.EVENT_BUNDLE_DISAPPEAR, OnBundleDisappear); // Register the event for bundle disappearance
    }

    private void OnDestroy()
    {
        EventDispatcher.instance.UnRegist(EventDef.EVENT_BUNDLE_DISAPPEAR, OnBundleDisappear); // Unregister the event for bundle disappearance
    }

    private void OnBundleDisappear(params object[] args)
    {
        var pos = (Vector3)args[0]; // Get the position from the event arguments
        ShowDisappearEffect(pos); // Show the disappear effect at the specified position
        ShowScoreEffect(pos,200); // Show the score effect at the specified position
        
    }

    public void ShowScoreEffect(Vector3 pos, int addScore)
    {
        TextMeshPro textMesh = null;
        if (m_scoreEffectPool.Count > 0)
            textMesh = m_scoreEffectPool.Dequeue();
        else
        {
            var obj = Instantiate(scoreEffectPrefab);
            obj.transform.SetParent(m_effectRoot, false);
            textMesh = obj.GetComponent<TextMeshPro>();
            var aniEvent = obj.GetComponent<AnimationEvent>();
            aniEvent.aniEventCb = (str) =>
            {
                if ("finish" == str)
                {
                    obj.SetActive(false);
                    m_scoreEffectPool.Enqueue(textMesh);
                }
            };
        }
        textMesh.gameObject.SetActive(true);
        textMesh.transform.position = pos;
        textMesh.text = addScore.ToString();
    }


    private void Update()
    {
        if (m_disappearEffectPool.Count > 0)
        {
            var obj = m_disappearEffectPool.Dequeue(); // Dequeue the object from the pool
            obj.SetActive(false); // Deactivate the object
            m_disappearEffectPool.Enqueue(obj); // Enqueue the object back to the pool
        }
    }
}
