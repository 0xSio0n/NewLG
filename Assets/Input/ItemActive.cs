using UnityEngine;

public interface IEquipment {
    PlayerActive ProfilePlayer { get; set; }
    int TypeItem { get; set; }
}

public enum DamageSet {
    PlayerPhs, PlayerMag,
    PlayerHeal,
    EnemyPhs,
}

public class ItemActive : MonoBehaviour {
    //public InputPlayModule InputPlay;

    public bool IsPowerUp;

    [Header("Item Data")]
    public int Uid;
    public string Name;
    public string Description;
    public string MeshItem;
    public string Texture;
    public int TypeItem;
    public bool IsEquipment;

    public EquipmentModule EquipPlayer { get; set; }
    public PocketModule PocketPlayer { get; set; }

    private void PickItem() {
        PocketPlayer = PlayerActive.Instance.Pocket;
        if (PocketPlayer.Itemhand.Point == null) {
            var mesh = Resources.Load<GameObject>("ItemWorld/" + MeshItem);
            var texture = Resources.LoadAll("Texture/" + Texture);
            var icon = texture[Uid] as Sprite;

            PocketPlayer.OnItemCallback += () => {
                Destroy(gameObject);
                PlayerActive.Instance.OnInteraction -= Interaction_Handler;
                //PlayerActive.Instance.OnInteract -= PickItem;
                for (int i = 0; i < PocketPlayer.Pockets.Length; i++) {
                    if (PocketPlayer.Pockets[i].Uid == 0) {
                        PlayerActive.Instance.InputPlay.OnPocket[i] = null;
                    }
                }
                PocketPlayer.Itemhand = new();
                PocketPlayer.OnUiCallback += null ;
                PocketPlayer.OnItemCallback += null;
            };

            var obj = new ItemInfo { 
                Uid = Uid, 
                Type = TypeItem,
                Name = Name, 
                Description = Description, 
                Point = mesh, 
                Texture = icon, 
                IsEquipment = IsEquipment,
                IsPowerUp = IsPowerUp,
                Qty = 1,
            };
            PocketPlayer.Itemhand = obj;
        }
    }

    private void PickEquip() {
        EquipPlayer = PlayerActive.Instance.Equipment;
        if (EquipPlayer.Equiphand.Point == null) {
            var mesh = Resources.Load<GameObject>("Equipment/" + MeshItem);
            var texture = Resources.LoadAll("Texture/" + Texture);
            var icon = texture[Uid] as Sprite;

            EquipPlayer.OnItemCallback += delegate {
                Destroy(gameObject);
            };

            var obj = new ItemInfo { 
                Uid = Uid, 
                Type = TypeItem,
                Name = Name, 
                Description = Description, 
                Point = mesh, 
                Texture = icon,
                IsEquipment = IsEquipment,
                IsPowerUp = IsPowerUp,
                Qty = 1, 
            };
            EquipPlayer.Equiphand = obj;
        }
    }

    private void Interaction_Handler() {
        if (IsEquipment) {
            PickEquip();
        } else {
            PickItem();
        }
    }

    private void OnTriggerEnter(Collider collision) {
        if (collision.CompareTag("Player")) {
            PlayerActive.Instance.OnInteraction += Interaction_Handler; 
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (collision.CompareTag("Player")) {
            PocketPlayer = PlayerActive.Instance.Pocket;
            if (PocketPlayer.Itemhand.Point != null) {
                PocketPlayer.Itemhand = new();
            }
            PocketPlayer.OnUiCallback += null;

            EquipPlayer = PlayerActive.Instance.Equipment;
            if (EquipPlayer.Equiphand.Point != null) {
                EquipPlayer.Equiphand = new();
            }
            EquipPlayer.OnUiCallback += null; //UiCallback(false); 

            for (int i = 0; i < PocketPlayer.Pockets.Length; i++) {
                if (PocketPlayer.Pockets[i].Uid == 0) {
                    PlayerActive.Instance.InputPlay.OnPocket[i] = null;
                }
            }
            PlayerActive.Instance.OnInteraction -= Interaction_Handler;
            //Input_GamePlay.Instance.OnInteract -= IsEquipment ? PickEquip : PickItem;
        }
    }
}