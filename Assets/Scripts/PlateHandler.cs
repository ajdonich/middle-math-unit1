using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateHandler : MonoBehaviour
{
    public TypedNumber.Type plateId;
    private GameObject gameNumber;

    void Awake() {
        gameNumber = transform.parent.Find("game_number").gameObject;
    }

    void OnTriggerEnter2D(Collider2D col) {
        Animator animComp = GetComponent<Animator>();
        animComp.SetInteger("PlateHoverId", (int)plateId);
        gameNumber.SendMessage("OnPlateEnter2D", plateId);
    }

    void OnTriggerExit2D(Collider2D col) {
        Animator animComp = GetComponent<Animator>();
        animComp.SetInteger("PlateHoverId", (int)TypedNumber.Type._UNDEF);
        gameNumber.SendMessage("OnPlateExit2D", plateId);
    }
}
