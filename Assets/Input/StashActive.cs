using System.Collections.Generic;
using UnityEngine;

public class StashActive : MonoBehaviour {
    public List<ItemInfo> Inventories;

    private PlayerActive player;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            player = other.GetComponent<PlayerActive>();
            player.Pocket.Stash = gameObject;
            player.Equipment.Stash = gameObject;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerActive.Instance.OnInteraction += null;
            player.Pocket.Stash = null;
            player.Equipment.Stash = null;
        }
    }
}