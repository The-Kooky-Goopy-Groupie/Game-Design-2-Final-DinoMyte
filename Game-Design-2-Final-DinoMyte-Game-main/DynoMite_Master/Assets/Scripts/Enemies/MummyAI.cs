using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

// AI for the Mummy 

public class MummyAI : NetworkComponent
{
    public Vector2 playerLoc; //This stores the position of the nearest player
    public Rigidbody2D myRig; // rigid body of the object
    public Vector2 LastMove; // last movement
    public bool inRange = false; // check there in range
    //public Text debugText;

    public bool right; //Fixes a nasty visual bug on the Mummies when trying to move.
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "MOVE")
        {
            string[] args = value.Split(',');
            myRig.velocity = new Vector2(float.Parse(args[0]), float.Parse(args[1]) + myRig.velocity.y); //used to determine the movement of the mummy
        }
        /*
        if (flag == "DEBUG")
        {
            string[] args = value.Split(',');
            //debugText.text = value;
        }
        */
    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer)
        {
            //This is only called once the object is initialized.
            
            playerLoc = new Vector3(-1500, -1500, 0);
            //This is used to make sure the object can stop a pursuit.
        }
        
        while(IsServer)
        {
            //Write the AI here!

            //Only run this if there is at least one player online

            while (GameObject.FindObjectOfType<GameCharacter>() != null)
            {
                playerLoc = new Vector3(-1500, -1500, 0);
                //Contingency for this event
                if (myRig.position.y < -20)
                {
                    //Destroys this instance if it exits the map
                    MyCore.NetDestroyObject(this.NetId);
                }

                //Find the nearest player
                foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                {
                    //If this player is closer than the last one
                    if ((this.transform.position - gc.transform.position).magnitude < (myRig.position - playerLoc).magnitude)
                    {
                        if ((this.transform.position - gc.transform.position).magnitude < 10.0f && gc.entity.HP > 0)
                        {
                            playerLoc = gc.transform.position; //This location is within range.
                            inRange = true;
                        } else
                        {
                            inRange = false; // there is no player in range
                        }
                    }
                    
                }
                if(inRange) //Engage pursuit behavior.
                {
                    if (playerLoc.x > this.transform.position.x)
                    {
                        myRig.velocity = new Vector2(1, myRig.velocity.y); // get's the location of the  if the player is right or left and then swaps to go that way
                        SendUpdate("MOVE", "1,0");
                    }
                    else
                    {
                        myRig.velocity = new Vector2(-1, myRig.velocity.y);
                        SendUpdate("MOVE", "-1,0");
                    }
                } else //Engage patrol behavior.
                {
                    int rand = Random.Range(0,2); // randomly moves back and forth in a small area
                    if(rand == 0)
                    {
                        myRig.velocity = new Vector2(-1, myRig.velocity.y);
                        SendUpdate("MOVE", "-1,0");
                        
                    } else
                    {
                        myRig.velocity = new Vector2(1, myRig.velocity.y);
                        SendUpdate("MOVE", "1,0");
                    }
                    yield return new WaitForSeconds(2.8f);
                }
                yield return new WaitForSeconds(.1f);
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
        //if (IsClient)
        {
            this.GetComponent<Animator>().SetInteger("HP", this.gameObject.GetComponent<Entity>().HP); // get's the mummys animator
            //this.GetComponent<Animator>().SetFloat("YVEL", myRig.velocity.y);
        }

        //Orient the mummy in the direction it is moving
        if (myRig.velocity.x > 0.1)
            right = true;
        else if (myRig.velocity.x < 0.1)
            right = false;
        this.GetComponent<SpriteRenderer>().flipX = right;

        
    }

    private void OnDestroy()
    {
        if (Random.Range(0, 2) == 1 && IsServer)
            MyCore.NetCreateObject(11, -1, new Vector3(myRig.position.x, myRig.position.y, 0));
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}
