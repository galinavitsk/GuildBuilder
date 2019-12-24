using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LandTile
{
    public enum TileType {
                            FullGrass, BottomGrass, TopGrass, LeftGrass, RightGrass, TRGrass, TLGrass, BRGrass, BLGrass,
                            FullDirt, BottomDirt, TopDirt, LeftDirt, RightDirt, TRDirt, TLDirt, BRDirt, BLDirt,
                            LowGrass, Empty}
    
    TileType type=TileType.LowGrass;
    Action<LandTile> cbTileTypeChanged;
    public TileType Type{
        get{return type;}
        set{
            TileType oldType = type;
            type=value;
        //Calls a function and sets this tile as a parameter
        if(cbTileTypeChanged!=null && oldType!=type){ cbTileTypeChanged(this);}
        }
    }

    LooseObject looseObject;
    public Furniture installedObject{get;protected set;}

    World world;
    int x;
    public int X{
        get{ return x;}
    }
    int y;
    public int Y{
        get{ return y;}
    }

    public LandTile( World world, int x, int y){
        this.world=world;
        this.x=x;
        this.y=y;
    }


    //FUNCTIONS TO CALL WHEN TILE TYPE CHANGES
    public void RegisterTileTypeChangedCallback(Action<LandTile> callback){
        cbTileTypeChanged += callback;

    }
    public void UnregisterTileTypeChangedCallback(Action<LandTile> callback){
        cbTileTypeChanged -= callback;

    }

    public bool PlaceObject(Furniture objInstance){
        if(objInstance==null){
            installedObject = null;
            return true;
        }
        if(installedObject!=null){
            Debug.LogError("Trying to assign an InstalledObject to a tile that already has one!");
            return false;
        }
        installedObject=objInstance;
        return true;

    }



}
