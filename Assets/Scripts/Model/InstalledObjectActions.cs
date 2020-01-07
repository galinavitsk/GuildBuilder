using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObjectActions {
    public static void Door_UpdateAction (InstalledObject door, float deltaTime) {
        Debug.Log ("Door_UpdateAction");
        if (door.installedObjectParamenters["is_opening"] == 1) {
            door.installedObjectParamenters["openess"] += deltaTime;
            if (door.installedObjectParamenters["openess"] >= 1) {
                door.installedObjectParamenters["is_opening"] = 0;
            }
        } else {
            door.installedObjectParamenters["openess"] -= deltaTime;
        }
        door.installedObjectParamenters["openess"] = Mathf.Clamp01 (door.installedObjectParamenters["openess"]);
    }
    public static ENTERABILITY Door_IsEnterable (InstalledObject door) {
        door.installedObjectParamenters["is_opening"] = 1;
        if (door.installedObjectParamenters["openess"] >= 1) {
            return ENTERABILITY.Yes;
        } else { return ENTERABILITY.Soon; }
    }
}