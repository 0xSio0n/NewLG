using System;
using UnityEngine;

[Serializable]
public struct ItemInfo {
    public int Uid;
    public int Type;
    public string Name;
    public string Description;
    public GameObject Point;
    public Sprite Texture;
    public int Qty;
    public bool IsEquipment;
    public bool IsPowerUp;
}

[CreateAssetMenu(fileName = "PocketModule", menuName = "AddOn Module/Pocket", order = 3)]
public class PocketModule : ScriptableObject {
    public ItemInfo[] Pockets;

    private GameObject stash;

    public int PocketCheck {
        get {
            var stash = Stash.GetComponent<StashActive>().Inventories;
            if (Itemhand.Uid > 0) {
                for (int i = 0; i < stash.Count; i++) {
                    if (stash.ToArray()[i].Uid == Itemhand.Uid) {
                        stash.Remove(stash.ToArray()[i]);
                    }
                }
                Itemhand = new();
            }
            return 0;
        }
        set {
            var stash = Stash.GetComponent<StashActive>().Inventories;
            if (Itemhand.Uid > 0) {
                for (int i = 0; i < stash.Count && Pockets[value].Uid <= 0; i++) {
                    if (stash[i].Uid == Itemhand.Uid && !stash[i].IsEquipment) {
                        Pockets[value].Uid = stash[i].Uid;
                        Pockets[value].Type = stash[i].Type;
                        Pockets[value].Name = stash[i].Name;
                        Pockets[value].Description = stash[i].Description;
                        Pockets[value].Point = stash[i].Point;
                        Pockets[value].Qty = Mathf.Clamp(stash[i].Qty, 0, 9);
                        Pockets[value].Texture = stash[i].Texture;
                        Pockets[value].IsEquipment = stash[i].IsEquipment;
                        Pockets[value].IsPowerUp = stash[i].IsPowerUp;
                        stash.Remove(stash.ToArray()[i]);
                    }
                }
                Itemhand = new();
            } else {
                if (Pockets[value].Uid <= 0) {
                    return;
                }

                for (int i = 0; i < stash.Count; i++) {
                    if (stash[i].Uid == Pockets[value].Uid && stash[i].Type < 1) {
                        var tmp = Mathf.Clamp(stash[i].Qty + Pockets[value].Qty, 0, 99);
                        stash.Remove(stash[i]);
                        Pockets[value].Qty = tmp;
                    }
                }

                stash.Add(Pockets[value]);
                Pockets[value] = new();
            }
        }
    }

    public GameObject Stash {
        get {
            return stash;
        }
        set {
            PlayerActive.Instance.OnInteraction += (value != null) ? itemStashHandler : null;
            stash = value;
        }
    }

    public ItemInfo Itemhand {
        get {
            return ItemTmp;
        }
        set {
            ItemTmp = value;
            //if (GetComponent<Input_GamePlay>().enabled) {
            if (PlayerActive.Instance.InputPlay != null) {
                for (int i = 0; i < Pockets.Length; i++) {
                    if (value.Name != null && value.Name == Pockets[i].Name) {
                        PocketUpdate(i);
                        return;
                    }
                }
                for (int i = 0; i < Pockets.Length; i++) {
                    if (Pockets[i].Point == null) {
                        PlayerActive.Instance.InputPlay.OnPocket[i] = (e) => { PocketInsert(e); };
                        IsFull = true;
                    }
                }
                if (!IsFull) {
                    return;
                }
                uiCallbackHandler?.Invoke(true);
            }
        }
    }

    //public Action PocketTransact { get; set; }
    //public Action ItemStash { get; set; }
    //public Action ItemCallback { get; set; }
    //public Action<bool> UiCallback { get; set; }

    public delegate bool ExtraAction(int n);

    public bool IsFull;
    public ItemInfo ItemTmp;

    public event Action<bool> OnUiCallback {
        add {
            if (value != null) {
                uiCallbackHandler = delegate { };
                uiCallbackHandler += value;
            } else {
                uiCallbackHandler(false);
            }
        }
        remove {
            uiCallbackHandler -= value;
            if (value == null) {
                uiCallbackHandler = null;
            }
        }
    }

    public event Action OnItemCallback {
        add {
            if (value != null) {
                itemCallbackHandler = delegate { };
                itemCallbackHandler += value;
            } 
            //else {
                //itemCallbackHandler();
            //}
        }
        remove {
            if (value == null) {
                itemCallbackHandler = null;
            }
            itemCallbackHandler -= value;
        }
    }

    public event Action OnItemStash { 
        add {
            if (value != null) {
                itemStashHandler = delegate { };
                itemStashHandler += value;
            } else {
                itemStashHandler();
            }
        }
        remove {
            if (value == null) {
                itemStashHandler = null;
            }
            itemStashHandler -= value;
        }
    }

    public event Action OnPocketTransact { 
        add {
            if (value != null) {
                pocketTransact = delegate { };
                pocketTransact += value;
            } else {
                pocketTransact();
            }
        }
        remove {
            if (value == null) {
                pocketTransact = null;
            }
            pocketTransact -= value;
        }
    }

    public event ExtraAction OnExtraEvent;

    private Action<bool> uiCallbackHandler;
    private Action itemCallbackHandler;
    private Action itemStashHandler;
    private Action pocketTransact;

    private void PocketInsert(int i) {
        //var i = (int)state;
        if (Pockets[i].Point == null) {
            Pockets[i].Uid = ItemTmp.Uid;
            Pockets[i].Type = ItemTmp.Type;
            Pockets[i].Name = ItemTmp.Name;
            Pockets[i].Description = ItemTmp.Description;
            Pockets[i].Point = ItemTmp.Point;
            Pockets[i].Qty = Mathf.Clamp(1, 0, 9);
            Pockets[i].Texture = ItemTmp.Texture;
            Pockets[i].IsEquipment = ItemTmp.IsEquipment;
            Pockets[i].IsPowerUp = ItemTmp.IsPowerUp;

            if (PlayerActive.Instance.InputPlay != null) {
                uiCallbackHandler?.Invoke(IsFull = false);
                OnExtraEvent?.Invoke(i);
                itemCallbackHandler?.Invoke();
            }
        }
    }

    private void PocketUpdate(int i) {
        if (ItemTmp.Type > 0) {
            return;
        }
        Pockets[i].Qty += Mathf.Clamp(1, 0, 9);
        OnExtraEvent?.Invoke(i);

        if (Pockets[i].Qty == 0) {
            Pockets[i].Name = null;
            Pockets[i].Point = null;
        }
        itemCallbackHandler?.Invoke();
    }

    private void PocketDelete(ActionState state) {
        var i = (int)state; // Cek delete berulang2 
        if (Pockets[i].Qty > 0) {
            //if (ExtraEvent?.Invoke(i) == true) {
            Pockets[i].Qty -= Mathf.Clamp(1, 0, 9);
            if (Pockets[i].Qty == 0) {  // Fungsi submit auto kepanggils
                //PocketTransact();
                Pockets[i].Uid = 0;
                Pockets[i].Type = 0;
                Pockets[i].Name = null;
                Pockets[i].Description = null;
                Pockets[i].Point = null;
                Pockets[i].Texture = null;
                Pockets[i].IsEquipment = false;
                Pockets[i].IsPowerUp = false;
            }
            //}
        }
    }

}
