using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScript : MonoBehaviour
{
    public GameObject BackgroundGraphic;
    public GameObject MainCharacter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Get the player this client is
        foreach(GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
        {
            if (gc.IsLocalPlayer) {
                MainCharacter = gc.gameObject; // bassically makes the camera of the clients go on they'e player and then sets it's postionion if there isnt a player it does the other one
            }
        }
        if(MainCharacter != null)
            BackgroundGraphic.transform.position = new Vector3(-MainCharacter.transform.position.x / 10f, (-MainCharacter.transform.position.y / 10f) - 8, 10);
    }
}
