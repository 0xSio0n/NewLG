using System;
using UnityEditor.PackageManager;
using UnityEngine;

//[CreateAssetMenu(fileName = "EquipmentModule", menuName = "AddOn Module/Equipment", order = 2)]
[Serializable]
public class EquipmentModule /*: ScriptableObject*/ {
    public delegate GameObject Instance(GameObject prefab, Transform parent);

    public ItemInfo Weapon;
    public ItemInfo Armor;
    public ItemInfo Talent;

    [Header("Player Equip Right")]
    public GameObject RightWeapon;
    public bool IsRightArmed;

    [Header("Player Equip Left")]
    public GameObject LeftWeapon;
    public bool IsLeftArmed;

    [Header("Player Armor")]
    public GameObject ArmorTier;

    public EquipmentModule(Instance instantiate, Action<GameObject> destroy) {
        Instantiate = instantiate;
        Destroy = destroy;
    }

    public GameObject Stash { get; set; }
    public Action<GameObject> Destroy { get; set; }
    public Instance Instantiate { get; set; }

    public int EquipCheck {
        get {
            var stash = Stash.GetComponent<StashActive>().Inventories;
            if (Equiphand.Uid > 0) {
                for (int i = 0; i < stash.Count; i++) {
                    if (stash.ToArray()[i].Uid == Equiphand.Uid) {
                        stash.Remove(stash.ToArray()[i]);
                    }
                }
                Equiphand = new();
            }
            return 0;
        }
        set {
            var stash = Stash.GetComponent<StashActive>().Inventories;
            if (Equiphand.Uid > 0) {
                for (int i = 0; i < stash.Count; i++) {
                    if (stash[i].Uid == Equiphand.Uid && stash[i].IsEquipment) {
                        if (stash[i].Type == 1 && Weapon.Uid <= 0) {
                            WeaponSlotSetup(stash[i]);
                            RightWeapon = Instantiate?.Invoke(Weapon.Point, PlayerActive.Instance.FirstSlot);
                            RightWeapon.GetComponent<IEquipment>().TypeItem = Weapon.Type;   
                            stash.Remove(stash[i]);
                            break;
                        } else if (stash[i].Type == 5 && Armor.Uid <= 0) {
                            ArmorSlotSetup(stash[i]);
                            ArmorTier = Armor.Point;
                            ArmorTier.GetComponent<IEquipment>().TypeItem = Armor.Type;
                            stash.Remove(stash[i]);
                            break;
                        } else if (stash[i].Type == 6 && Talent.Uid <= 0) {
                            TalentSlotSetup(stash[i]);
                            Instantiate?.Invoke(Talent.Point, PlayerActive.Instance.transform);
                            stash.Remove(stash[i]);
                        }
                    }
                }
                Equiphand = new();
            } else {
                if (Weapon.Uid > 0 && RightWeapon.GetComponent<IEquipment>().TypeItem < 5) {
                    RightWeapon.GetComponent<IEquipment>().TypeItem = 0;
                    stash.Add(Weapon);
                    Destroy?.Invoke(RightWeapon);
                    RightWeapon = null;
                    Weapon = new();
                } else if (Talent.Uid > 0) {
                    var skill = PlayerActive.Instance.transform;
                    Destroy?.Invoke(skill.Find("AbilitesTree(Clone)").gameObject);
                    stash.Add(Talent);
                    Talent = new();
                } else if (Armor.Uid > 0 && ArmorTier.GetComponent<IEquipment>().TypeItem == 5) {
                    ArmorTier.GetComponent<IEquipment>().TypeItem = 0;
                    stash.Add(Armor);
                    ArmorTier = null;
                    Armor = new();
                }
            }
        }
    }

    public ItemInfo Equiphand {
        get {
            return ItemTmp;
        }
        set {
            ItemTmp = value;
            itemCallbackHandler += delegate {
                var player = PlayerActive.Instance;
                if (player.InputPlay != null) {
                    switch (ItemTmp.Type) {
                        case 1: // Weapon First Slot
                            WeaponSlotSetup(ItemTmp);
                            RightWeapon = Instantiate?.Invoke(Weapon.Point, player.FirstSlot);
                            RightWeapon.GetComponent<IEquipment>().TypeItem = Weapon.Type; 
                            player.OnEquipReady = ChangeBattleMode;
                            break;
                        case 2: // Weapon Second Slot
                            WeaponSlotSetup(ItemTmp);
                            LeftWeapon = Instantiate?.Invoke(Weapon.Point, player.SecondSlot);
                            break;
                        case 3: // Weapon Third Slot
                            //WeaponSlotSetup(ItemTmp);
                            //RightWeapon = Instantiate?.Invoke(Weapon.Point, player.ThirdSlot);
                            //RightWeapon.GetComponent<IEquipment>().TypeItem = Weapon.Type;
                            break;
                        case 4: // Weapon Fourth Slot
                            //WeaponSlotSetup(ItemTmp);
                            //LeftWeapon = Instantiate?.Invoke(Weapon.Point, player.FourthSlot);
                            break;
                        case 5: // Armor Suit
                            ArmorSlotSetup(ItemTmp);
                            ArmorTier = Armor.Point;
                            ArmorTier.GetComponent<IEquipment>().TypeItem = Armor.Type;
                            break;
                        case 6: // Ability 
                            TalentSlotSetup(ItemTmp);
                            Instantiate?.Invoke(Talent.Point, player.transform);
                            break;
                    }
                }
            };
            if (value.Uid > 1) {
                uiCallbackHandler?.Invoke(true);
            }
        }
    }

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
            } else {
                itemCallbackHandler();
            }
        }
        remove {
            if (value == null) {
                itemCallbackHandler = null;
            }
            itemCallbackHandler -= value;
        }
    }

    private Action<bool> uiCallbackHandler;
    private Action itemCallbackHandler;

    //private void OnEnable() {
    //    Equiphand = new();
    //}

    private void ChangeBattleMode() {
        if (RightWeapon) {
            IsRightArmed = !IsRightArmed;
            var type = RightWeapon.GetComponent<IEquipment>().TypeItem;
            var player = PlayerActive.Instance;
            switch (type) {
                case 1:
                    RightWeapon.transform.SetParent(IsRightArmed ? player.RightSlot : player.FirstSlot);
                    break;
            }
            RightWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }

    private void WeaponSlotSetup(ItemInfo item) {
        Weapon.Uid = item.Uid;
        Weapon.Type = item.Type;
        Weapon.Name = item.Name;
        Weapon.Description = item.Description;
        Weapon.Texture = item.Texture;
        Weapon.Point = item.Point;
        Weapon.Qty = item.Qty;
        Weapon.IsEquipment = item.IsEquipment;
        Weapon.IsPowerUp = item.IsPowerUp;
    }

    private void ArmorSlotSetup(ItemInfo item) {
        Armor.Uid = item.Uid;
        Armor.Type = item.Type;
        Armor.Name = item.Name;
        Armor.Description = item.Description;
        Armor.Texture = item.Texture;
        Armor.Point = item.Point;
        Armor.Qty = item.Qty;
        Armor.IsEquipment = item.IsEquipment;
        Armor.IsPowerUp = item.IsPowerUp;
    }

    private void TalentSlotSetup(ItemInfo item) {
        Talent.Uid = item.Uid;
        Talent.Type = item.Type;
        Talent.Name = item.Name;
        Talent.Description = item.Description;
        Talent.Texture = item.Texture;
        Talent.Point = item.Point;
        Talent.Qty = item.Qty;
        Talent.IsEquipment = item.IsEquipment;
        Talent.IsPowerUp = item.IsPowerUp;
    }
}

