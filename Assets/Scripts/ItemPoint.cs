using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 道具刷新点
/// </summary>
public class ItemPoint : MonoBehaviour
{
    public Vector3 rotEuler;
    public float cooldownTime;

    public Item[] itemList;
    private int itemIndex = -1; //-1代表当前没道具出现
    private ItemOnGround itemInstance;

    private void Update()
    {
        transform.Rotate(rotEuler * Time.deltaTime);
    }
    private void Start()
    {
        StartCoroutine(RefreshItem());
    }

    [ContextMenu("Generate")]
    private void GenerateItem()
    {
        itemIndex = Random.Range(0, itemList.Length);
        itemInstance = GameObject.Instantiate(itemList[itemIndex].prefab, transform.position + itemList[itemIndex].offset, Quaternion.identity, transform).GetComponent<ItemOnGround>();
        itemInstance.theItem = itemList[itemIndex];
        itemInstance.onPicked += StartRefreshItem;
    }

    private void StartRefreshItem()
    {
        StartCoroutine(RefreshItem());
    }

    private IEnumerator RefreshItem()
    {
        yield return new WaitForSeconds(cooldownTime);
        GenerateItem();
    }

}
