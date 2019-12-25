using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LandTile {
    public enum TileType {
        FullGrass,
        TopGrass,
        BottomGrass,
        RightGrass,
        LeftGrass,
        BLGrass,
        BRGrass,
        TLGrass,
        TRGrass,
        Grass_S,
        Grass_NS,
        Grass_N,
        Grass_E,
        Grass_EW,
        Grass_W,
        Grass,
        Grass_NE,
        Grass_NES,
        Grass_SE,
        Grass_ESW,
        Grass_NESW,
        Grass_NEW,
        Grass_SW,
        Grass_NSW,
        Grass_NW,
        Grass_Dia_SE,
        Grass_Full_NES_T,
        Grass_Full_NES_B,
        Grass_Dia_NE,
        Grass_Full_ESW_L,
        Grass_IC_TL,
        Grass_IC_BL,
        Grass_Full_NEW_L,
        Grass_ESW_R,
        Grass_IC_TR,
        Grass_IC_BR,
        Grass_Full_NEW_R,
        Grass_Dia_SW,
        Grass_Full_NSW_T,
        Grass_Full_NSW_B,
        Grass_Dia_NW,
        Grass_Exit_Top,
        Grass_Exit_Right,
        Grass_Exit_Left,
        Grass_Exit_Bottom,
        Grass_Full_SW_NE,
        Grass_Full_NW_SE,

        FullDirt,
        TopDirt,
        BottomDirt,
        RightDirt,
        LeftDirt,
        BLDirt,
        BRDirt,
        TLDirt,
        TRDirt,
        Dirt_S,
        Dirt_NS,
        Dirt_N,
        Dirt_E,
        Dirt_EW,
        Dirt_W,
        Dirt,
        Dirt_NE,
        Dirt_NES,
        Dirt_SE,
        Dirt_ESW,
        Dirt_NESW,
        Dirt_NEW,
        Dirt_SW,
        Dirt_NSW,
        Dirt_NW,
        Dirt_Dia_SE,
        Dirt_Full_NES_T,
        Dirt_Full_NES_B,
        Dirt_Dia_NE,
        Dirt_Full_ESW_L,
        Dirt_IC_TL,
        Dirt_IC_BL,
        Dirt_Full_NEW_L,
        Dirt_ESW_R,
        Dirt_IC_TR,
        Dirt_IC_BR,
        Dirt_Full_NEW_R,
        Dirt_Dia_SW,
        Dirt_Full_NSW_T,
        Dirt_Full_NSW_B,
        Dirt_Dia_NW,
        Dirt_Exit_Top,
        Dirt_Exit_Right,
        Dirt_Exit_Left,
        Dirt_Exit_Bottom,
        Dirt_Full_SW_NE,
        Dirt_Full_NW_SE,

        Generic,
        Empty
    }
    public enum TileBasicType { Generic, Grass, Dirt, Empty }
    public string[, ] neigh { get; protected set; }
    TileType type = TileType.Generic;
    TileBasicType basictype = TileBasicType.Generic;
    Action<LandTile> cbTileTypeChanged;
    public TileType Type {
        get { return type; }
        set {
            TileType oldType = type;
            type = value;

            //Calls a function and sets this tile as a parameter
            if (cbTileTypeChanged != null && oldType != type) { cbTileTypeChanged (this); }
            if (type == TileType.FullGrass ||
                type == TileType.TopGrass ||
                type == TileType.BottomGrass ||
                type == TileType.RightGrass ||
                type == TileType.LeftGrass ||
                type == TileType.BLGrass ||
                type == TileType.BRGrass ||
                type == TileType.TLGrass ||
                type == TileType.TRGrass ||
                type == TileType.Grass_S ||
                type == TileType.Grass_NS ||
                type == TileType.Grass_N ||
                type == TileType.Grass_E ||
                type == TileType.Grass_EW ||
                type == TileType.Grass_W ||
                type == TileType.Grass ||
                type == TileType.Grass_NE ||
                type == TileType.Grass_NES ||
                type == TileType.Grass_SE ||
                type == TileType.Grass_ESW ||
                type == TileType.Grass_NESW ||
                type == TileType.Grass_NEW ||
                type == TileType.Grass_SW ||
                type == TileType.Grass_NSW ||
                type == TileType.Grass_NW ||
                type == TileType.Grass_Dia_SE ||
                type == TileType.Grass_Full_NES_T ||
                type == TileType.Grass_Full_NES_B ||
                type == TileType.Grass_Dia_NE ||
                type == TileType.Grass_Full_ESW_L ||
                type == TileType.Grass_IC_TL ||
                type == TileType.Grass_IC_BL ||
                type == TileType.Grass_Full_NEW_L ||
                type == TileType.Grass_ESW_R ||
                type == TileType.Grass_IC_TR ||
                type == TileType.Grass_IC_BR ||
                type == TileType.Grass_Full_NEW_R ||
                type == TileType.Grass_Dia_SW ||
                type == TileType.Grass_Full_NSW_T ||
                type == TileType.Grass_Full_NSW_B ||
                type == TileType.Grass_Dia_NW ||
                type == TileType.Grass_Exit_Top ||
                type == TileType.Grass_Exit_Right ||
                type == TileType.Grass_Exit_Left ||
                type == TileType.Grass_Exit_Bottom ||
                type == TileType.Grass_Full_SW_NE ||
                type == TileType.Grass_Full_NW_SE) { basictype = TileBasicType.Grass; } else if (type == TileType.FullDirt ||
                type == TileType.TopDirt ||
                type == TileType.BottomDirt ||
                type == TileType.RightDirt ||
                type == TileType.LeftDirt ||
                type == TileType.BLDirt ||
                type == TileType.BRDirt ||
                type == TileType.TLDirt ||
                type == TileType.TRDirt ||
                type == TileType.Dirt_S ||
                type == TileType.Dirt_NS ||
                type == TileType.Dirt_N ||
                type == TileType.Dirt_E ||
                type == TileType.Dirt_EW ||
                type == TileType.Dirt_W ||
                type == TileType.Dirt ||
                type == TileType.Dirt_NE ||
                type == TileType.Dirt_NES ||
                type == TileType.Dirt_SE ||
                type == TileType.Dirt_ESW ||
                type == TileType.Dirt_NESW ||
                type == TileType.Dirt_NEW ||
                type == TileType.Dirt_SW ||
                type == TileType.Dirt_NSW ||
                type == TileType.Dirt_NW ||
                type == TileType.Dirt_Dia_SE ||
                type == TileType.Dirt_Full_NES_T ||
                type == TileType.Dirt_Full_NES_B ||
                type == TileType.Dirt_Dia_NE ||
                type == TileType.Dirt_Full_ESW_L ||
                type == TileType.Dirt_IC_TL ||
                type == TileType.Dirt_IC_BL ||
                type == TileType.Dirt_Full_NEW_L ||
                type == TileType.Dirt_ESW_R ||
                type == TileType.Dirt_IC_TR ||
                type == TileType.Dirt_IC_BR ||
                type == TileType.Dirt_Full_NEW_R ||
                type == TileType.Dirt_Dia_SW ||
                type == TileType.Dirt_Full_NSW_T ||
                type == TileType.Dirt_Full_NSW_B ||
                type == TileType.Dirt_Dia_NW ||
                type == TileType.Dirt_Exit_Top ||
                type == TileType.Dirt_Exit_Right ||
                type == TileType.Dirt_Exit_Left ||
                type == TileType.Dirt_Exit_Bottom ||
                type == TileType.Dirt_Full_SW_NE ||
                type == TileType.Dirt_Full_NW_SE) { basictype = TileBasicType.Dirt; } else if (type == TileType.Generic) {
                basictype = TileBasicType.Generic;
            } else if (type == TileType.Empty) {
                basictype = TileBasicType.Empty;
            }
            Debug.Log ("TileType:" + type + " Basic TileType" + basictype);
        }
    }

    LooseObject looseObject;
    public Furniture installedObject { get; protected set; }

    World world;
    int x;
    public int X {
        get { return x; }
    }
    int y;
    public int Y {
        get { return y; }
    }

    public LandTile (World world, int x, int y) {
        this.world = world;
        this.x = x;
        this.y = y;
    }

    //FUNCTIONS TO CALL WHEN TILE TYPE CHANGES
    public void RegisterTileTypeChangedCallback (Action<LandTile> callback) {
        cbTileTypeChanged += callback;

    }
    public void UnregisterTileTypeChangedCallback (Action<LandTile> callback) {
        cbTileTypeChanged -= callback;

    }

    public bool PlaceObject (Furniture objInstance) {
        if (objInstance == null) {
            installedObject = null;
            return true;
        }
        if (installedObject != null) {
            Debug.LogError ("Trying to assign an InstalledObject to a tile that already has one!");
            return false;
        }
        installedObject = objInstance;
        return true;

    }

    

}