using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// these set up the enemy spawners to respawn enemy so long as a player is not nearby and is less then the enity max

public class EnemySpawner : NetworkComponent
{
    public int entities = 0; //current number of entities
    public int entityMax; //max number of entities
    public int entityType; //corresponds to prefab array on network managers
    public float proximity = 0f; //minimum distance from player to start spawning enemies
    public Vector3 closestPlayer; // checks for the closet player in range
    public override void HandleMessage(string flag, string value)
    {

    }
    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
    public override IEnumerator SlowUpdate()
    {
        while(IsServer)
        {
            if(entities < entityMax)
            {
                bool flag = true;
                //For every player on the server
                foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                {
                    //If no players are closer than the proximity defined...
                    if (((this.transform.position - gc.transform.position).magnitude < proximity))
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    GameObject enemy = MyCore.NetCreateObject(entityType, -1, this.transform.position); //Go ahead and spawn an enemy!
                    if (enemy.GetComponent<SpawnManager>() != null)
                        enemy.GetComponent<SpawnManager>().parent = this; //Link the child monster to this spawner so it knows when it has been deleted
                    entities++;
                }
            }
            yield return new WaitForSeconds(1f);
        }
        //throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
