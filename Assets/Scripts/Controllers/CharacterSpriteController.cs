using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour {
    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSprites;

    World world {
        get { return WorldController.Instance.World; }
    }

    // Start is called before the first frame update
    void Start () {
        LoadSprites ();
        characterGameObjectMap = new Dictionary<Character, GameObject> ();
        world.RegisterCharacterCreated (OnCharacterCreated);

        //DEBUG
        //tile, speed, character name, build speed
        Character c = world.CreateCharacter (new Vector3Int (world.Width / 2, world.Height / 2, 0), 5, "Astrid", 5f);
        //Character a = world.CreateCharacter (new Vector3Int (world.Width / 2-2, world.Height / 2-2, 0), 5, "Bren", 2f);
        
    }
    public void OnCharacterCreated (Character character) {
        GameObject char_go = new GameObject ();
        characterGameObjectMap.Add (character, char_go);
        char_go.name = character.name;
        char_go.transform.position = character.currTile;
        char_go.transform.SetParent (this.transform, true);
        char_go.AddComponent<SpriteRenderer> ().sprite = characterSprites["AH_SpriteSheet_People1_1"];
        char_go.GetComponent<SpriteRenderer> ().sortingLayerName = "Characters";
        character.RegisterCharacterMovedCallback (OnCharacterMoved);
    }

    Sprite GetSpriteForCharacter (Character character) {
        return null;
    }
    void LoadSprites () {
        characterSprites = new Dictionary<string, Sprite> ();
        Sprite[] characterSpritesNames = Resources.LoadAll<Sprite> ("Images/Characters/");
        foreach (var s in characterSpritesNames) {
            characterSprites.Add (s.name.ToString (), s);
        }
    }

    void OnCharacterMoved (Character c) {
        if (characterGameObjectMap.ContainsKey (c) == false) {
            return;
        }
        GameObject char_go = characterGameObjectMap[c];
        char_go.transform.position = new Vector3 (c.X, c.Y, 0);
    }

    // Update is called once per frame

}