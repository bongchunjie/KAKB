using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour {

    [SerializeField] private float time = .1f;

    private void Update() {
        time -= Time.deltaTime;
        if (time <= 0f) {
            gameObject.SetActive(false);
        }
    }

    public void SetEnableTime(float time) {
        this.time = time;
        gameObject.SetActive(true);
    }

}