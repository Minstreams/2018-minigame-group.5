﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 道具刷新点
/// </summary>
public class ItemPoint : NetworkBehaviour
{
    public float cooldownTime;

    public Item[] itemList;
    [SyncVar]
    private int syncCurrentItemIndex = -1;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(RefreshItem());
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (syncCurrentItemIndex >= 0)
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
        ItemOnGround iog = GameSystem.ItemSystem.GenerateItem(transform, new GameSystem.ItemSystem.ItemGenerationInformation(GameSystem.ItemSystem.ItemState.pickable, itemList[itemIndex].maxAmmo, itemList[itemIndex]));
        if (isServer) iog.onPicked += () => { StartCoroutine(RefreshItem()); syncCurrentItemIndex = -1; };
    }

    private IEnumerator RefreshItem()
    {
        yield return new WaitForSeconds(cooldownTime);
        RpcGenerateItem(syncCurrentItemIndex = Random.Range(0, itemList.Length));
    }

#if UNITY_EDITOR
    [ContextMenu("generate")]
    public void TestGenerateItem()
    {
        StopAllCoroutines();
        int itemIndex = Random.Range(0, itemList.Length);
        ItemOnGround iog = GameSystem.ItemSystem.GenerateItem(transform, new GameSystem.ItemSystem.ItemGenerationInformation(GameSystem.ItemSystem.ItemState.pickable, itemList[itemIndex].maxAmmo, itemList[itemIndex]));
    }

    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position - Vector3.up * 0.8f, Vector3.up, GameSystem.ItemSystem.PickRange / 2);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, GameSystem.ItemSystem.PickRange);
        UnityEditor.Handles.color = Color.white;
    }
#endif
}
