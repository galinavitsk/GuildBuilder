using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    LandTile[,] tiles;
    Dictionary<string,Furniture> furniturePrototypes;
    int width;
    int height;
    Action<Furniture> cbFurnitureCreated;

    public int Width{
        get{
            return width;
        }
    }
    public int Height{
        get{
            return height;
        }
    }


    public World(int width=10,int height=10){
        this.width = width;
        this.height=height;
        tiles = new LandTile[width,height];
       //Creates all the tiles once the world size is created
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x,y]= new LandTile(this,x,y);
            }
        }
        Debug.Log("World created with "+(width*height)+" tiles.");
        CreateInstalledObjectPrototypes();
    }

    void CreateInstalledObjectPrototypes(){
        furniturePrototypes=new Dictionary<string, Furniture>();
        furniturePrototypes.Add("Wall",Furniture.CreatePrototype("Wall",0,1,1,true));
    }


    public void RandomizeTiles(){
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                switch (UnityEngine.Random.Range(0,3))
                {
                    case 0:
                        tiles[x,y].Type=LandTile.TileType.FullDirt;
                        break;
                    case 1:
                        tiles[x,y].Type=LandTile.TileType.FullGrass;
                        break;
                    default:
                        tiles[x,y].Type=LandTile.TileType.LowGrass;
                        break;
                }
                CheckNeighbors(x,y);
                
            }
        }
    }
    public LandTile GetTileAt(int x, int y){
        if (x>width || x<0){
            Debug.LogError("LandTile ("+x+","+y+") is out of range");
            return null;
        }
        return tiles[x,y];
    }

    public void PlaceInstalledObject(string objectType, LandTile t){
        //TODO: Function assumes 1x1 tiles, and no rotation--Change this
        if( furniturePrototypes.ContainsKey(objectType) == false){
            Debug.LogError("furniturePrototypes doesn't contain a poroto for the key:"+objectType);
            return;
        }
        Furniture obj=Furniture.PlaceInstance(furniturePrototypes[objectType], t);
        if(obj==null){
            //failed to place object
            return;
        }
        if(cbFurnitureCreated!=null){
            cbFurnitureCreated(obj);
        }
        
    }
    
    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc) {
		cbFurnitureCreated += callbackfunc;
	}

	public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc) {
		cbFurnitureCreated -= callbackfunc;
	}
    public LandTile.TileType CheckNeighbors(int x, int y){
        //Debug.Log("InitialTileCheck, tile checked:"+x+"_"+y);
        LandTile.TileType tile_under=LandTile.TileType.Empty;
        LandTile.TileType tile_left=LandTile.TileType.Empty;
        
        LandTile.TileType tile_top=LandTile.TileType.Empty;
        
        LandTile.TileType tile_right=LandTile.TileType.Empty;
    //Check tile under LandTile [x,y]
    if(y>0){//skip check if y is 0
        tile_under=tiles[x,y-1].Type;
    }
    else if(y==0){
        tile_under=LandTile.TileType.Empty;
    }

    if(x>0){//skip check if x is 0
        tile_left=tiles[x-1,y].Type;
    }
    else if(x==0){
        tile_left=LandTile.TileType.Empty;
    }
    if(y<height-1){//skip check if y is height
        tile_top=tiles[x,y+1].Type;
    }
    else if(y==height){
        tile_top=LandTile.TileType.Empty;
    }

    if(x<width-1){//skip check if x is wdith
        tile_right=tiles[x+1,y].Type;
    }
    else if(x==width){
        tile_right=LandTile.TileType.Empty;
    }
    //Checks that the tile to the left of x,y exists
        if(tile_under!=LandTile.TileType.Empty){//Checks that the tile under x,y exists
            if(tile_under==LandTile.TileType.FullDirt){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.FullDirt, LandTile.TileType.TopDirt,};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.FullGrass){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.FullGrass, LandTile.TileType.TopGrass,};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.RightGrass){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.RightGrass, LandTile.TileType.TRGrass,};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.RightDirt){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.RightDirt, LandTile.TileType.TRDirt,};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.LeftGrass){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.LeftGrass, LandTile.TileType.TLGrass,};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.LeftDirt){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.LeftDirt, LandTile.TileType.TLDirt,};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.TopGrass){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.LowGrass};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
            }
            else if(tile_under==LandTile.TileType.TopDirt){
                LandTile.TileType[] over=new LandTile.TileType[]{LandTile.TileType.LowGrass};
                tiles[x,y].Type=over[UnityEngine.Random.Range(0,over.Length)];
                }
        }
        if(tile_left!=LandTile.TileType.Empty){}
    return tiles[x,y].Type;
    }
}



