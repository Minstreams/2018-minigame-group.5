using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 道具刷新点
/// </summary>
public class ItemPoint : NetworkBehaviour
{
    public Vector3 rotEuler;
    public float cooldownTime;

    public Item[] itemList;
    [SyncVar]
    private int syncCurrentItemIndex = -1;
    private ItemOnGround itemInstance;

    private void Update()
    {
        transform.Rotate(rotEuler * Time.deltaTime);
    }
    private void Start()
    {
        if (itemList == null || itemList.Length == 0) return;
        if (isServer)
            StartCoroutine(RefreshItem());
        else if (syncCurrentItemIndex >= 0)
        {
            ClientGenerateItem(syncCurrentItemIndex);
        }
    }

    [ClientRpc]
    private void RpcGenerateItem(int itemIndex)
    {
        ClientGenerateItem(itemIndex);
    }

    private void ClientGenerateItem(int itemIndex)
    {
        itemInstance = GameObject.Instantiate(itemList[itemIndex].prefab, transform.position + itemList[itemIndex].offset, Quaternion.identity, transform).GetComponent<ItemOnGround>();
        itemInstance.theItem = itemList[itemIndex];
        if (isServer)
            itemInstance.onPicked += () => { StartCoroutine(RefreshItem()); syncCurrentItemIndex = -1; };
    }

    private IEnumerator RefreshItem()
    {
        yield return new WaitForSeconds(cooldownTime);
        RpcGenerateItem(syncCurrentItemIndex = Random.Range(0, itemList.Length));
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position - Vector3.up * 0.8f, Vector3.up, ItemOnGround.pickRange / 2);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, ItemOnGround.pickRange);
        UnityEditor.Handles.color = Color.white;
    }
#endif

}
