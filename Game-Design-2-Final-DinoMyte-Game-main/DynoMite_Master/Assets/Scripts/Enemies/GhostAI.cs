using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// AI made to becme the ghost.

public class GhostAI : NetworkComponent
{
    public Vector2 playerLoc; //This stores the position of the nearest player
    public Rigidbody2D myRig; // used to get the rig
    public Vector2 LastMove; // used to check the last movement made
    //public Text debugText;

    public bool right; //Fixes a nasty visual bug on the Mummies when trying to move.
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "MOVE")
        {
            string[] args = value.Split(',');
            myRig.velocity = new Vector2(float.Parse(args[0]), float.Parse(args[1]));// check the movement flag and get's the data to apply it to velocity.
        }
        /*
        if (flag == "DEBUG")
        {
            string[] args = value.Split(',');
            debugText.text = value;
        }
        */
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            //This is only called once the object is initialized.

            playerLoc = new Vector3(-1500, -1500, 0);
            //This is used to make sure the object can stop a pursuit.
        }

        while (IsServer)
        {
            //Write the AI here!
            //Only run this if there is at least one player online
            while (GameObject.FindObjectOfType<GameCharacter>() != null)
            {
                playerLoc = new Vector3(-1500, -1500, 0);
                //Find the nearest player
                foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                {
                    //If this player is closer than the last one
                    if ((this.transform.position - gc.transform.position).magnitude < (myRig.position - playerLoc).magnitude)
                    {
                        if ((this.transform.position - gc.transform.position).magnitude < 15.0f && gc.entity.HP > 0)
                        {
                            playerLoc = gc.transform.position; //This is the new location to go to
                        }
                    }
                }
                if ((playerLoc - myRig.position).magnitude < 15.0f) //used to go to the nearest player
                {
                    Vector2 vel = (playerLoc - myRig.position).normalized * 1.5f; //Ghost speed increased by 50%
                    myRig.velocity = vel;
                    SendUpdate("MOVE", vel.x.ToString() +","+ vel.y.ToString());
                } else
                {
                    Vector2 vel = Vector2.zero; // they stop chasing the player
                    myRig.velocity = vel;
                    SendUpdate("MOVE", vel.x.ToString() + "," + vel.y.ToString());
                }
                //Contingency for this event - if they fall off the map they die
                if (myRig.position.y < -20)
                {
                    //Destroys this instance if it exits the map
                    MyCore.NetDestroyObject(this.NetId);
                }
                yield return new WaitForSeconds(.1f);
            } while (GameObject.FindObjectOfType<GameCharacter>() == null) //Fixed a bug where ghosts would fly off the map if a player logged off within their range
            {
                myRig.velocity = Vector2.zero;
                SendUpdate("MOVE", "0,0");
                yield return new WaitForSeconds(5f);
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    //Animator controller goes here
    void Update()
    {
        //if (IsServer)
        {
            this.GetComponent<Animator>().SetInteger("HP", this.gameObject.GetComponent<Entity>().HP); // gets the aniamtor and the hp values 
        }

        //Orient the ghost sprite in the direction it is moving
        if (myRig.velocity.x > 0.1)
            right = true;
        else if (myRig.velocity.x < 0.1)
            right = false;
        this.GetComponent<SpriteRenderer>().flipX = right;
    }
    private void OnDestroy()
    {
        if (Random.Range(0, 2) == 1 && IsServer)
            MyCore.NetCreateObject(12, -1, new Vector3(myRig.position.x, myRig.position.y, 0));
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}
