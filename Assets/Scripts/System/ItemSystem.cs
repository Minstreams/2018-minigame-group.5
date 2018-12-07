using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;
using UnityEngine.Networking;

namespace GameSystem
{
    public class ItemSystem : SubSystem<ItemSystemSetting>
    {
        public enum ItemState
        {
            pickable,
            rock,
            onHand
        }

        public class ItemGenerationInformation
        {
            public ItemState state = ItemState.pickable;
            public int ammo = 0;
            public int itemIndex = -1;

            public ItemGenerationInformation() { }
            public ItemGenerationInformation(ItemState state, int ammo, Item item)
            {
                this.state = state;
                this.ammo = ammo;
                this.itemIndex = GetItemIndex(item);
            }
        }

        public static float PickRange { get { return Setting.pickRange; } }
        public static float ItemDropSpeed { get { return Setting.itemDropSpeed; } }

        public static Item GetItem(int index)
        {
            return Setting.itemList[index];
        }

        public static int GetItemIndex(Item item)
        {
            return Setting.itemList.IndexOf(item);
        }

        public static ItemOnGround GenerateItem(Transform parent, ItemGenerationInformation information)
        {
            Item item = GetItem(information.itemIndex);
            GameObject go = GameObject.Instantiate(item.prefab, parent.position + Vector3.zero, parent.rotation, parent);
            ItemOnGround iog = go.GetComponent<ItemOnGround>();
            iog.state = information.state;
            iog.ammo = information.ammo;
            iog.item = item;
            return iog;
        }
    }
}
