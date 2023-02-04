using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

// AI for the Skeleton

public class SkeletonAI : NetworkComponent
{
    public Vector2 playerLoc; //This stores the position of the nearest player
    public Rigidbody2D myRig; // rig of the body
    public Vector2 LastMove; // Last movement
    public bool inRange = false; // Check something is in range 
    //public Text debugText;
    //public bool isGrounded = true; //Used to determine if this enemy can move
    public bool attacking = false; //AI behavior; this is when a loaded skeleton wants to dunk.
    public bool reloading = false; //AI behavior; this is when an unloaded skeleton wants to reload.
    public bool fired = false; //AI behavior; this determines whether this skeleton has a skull or not.

    public bool right; //Fixes a nasty visual bug when trying to move.
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "MOVE")
        {
            string[] args = value.Split(',');
            myRig.velocity = new Vector2(float.Parse(args[0]), float.Parse(args[1]) + myRig.velocity.y); // used to determine the skeleton moving back and forth.
        }
        if(flag == "RELOAD")
        {
            reloading = bool.Parse(value);
            fired = false; //Now reloaded
        }
        if(flag == "ATTACK")
        {
            attacking = bool.Parse(value);
            //If this is false, a skull projectile should be spawned. (Called after the dunk animation plays.)
            
            fired = true;
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
                        if ((this.transform.position - gc.transform.position).magnitude < 12.0f && gc.entity.HP > 0)
                        {
                            playerLoc = gc.transform.position; //This location is within range.
                            inRange = true;
                        }
                        else
                        {
                            inRange = false;
                        }
                    }

                }
                if(fired && myRig.velocity.y == 0) //If you have expended your skull and you're on the ground
                {
                    SendUpdate("MOVE", "0,0"); //Stand still
                    reloading = true;
                    SendUpdate("RELOAD", true.ToString());
                    yield return new WaitForSeconds(0.8f); //Let this animation play
                    reloading = false;
                    SendUpdate("RELOAD", false.ToString());
                    fired = false; //Now reloaded
                }
                if (inRange) //Engage pursuit behavior.
                {
                    if(myRig.velocity.y == 0)
                    {
                        if(!fired && !attacking) //If you are in range of a player, you haven't fired, and you aren't attacking..
                        {
                            attacking = true;
                            SendUpdate("ATTACK", true.ToString());
                            yield return new WaitForSeconds(0.8f); //Let this animation play
                            attacking = false;
                            SendUpdate("ATTACK", false.ToString());
                            fired = true; //Now unloaded

                            GameObject skull = MyCore.NetCreateObject(4, -1, new Vector3(myRig.position.x, myRig.position.y + 3, 0)); //Get dunked on.
                            Physics2D.IgnoreCollision(this.GetComponent<Collider2D>(), skull.GetComponent<Collider2D>());
                            if (playerLoc.x < myRig.position.x)
                                skull.GetComponent<Rigidbody2D>().velocity = new Vector2(-2, 0);
                            else
                                skull.GetComponent<Rigidbody2D>().velocity = new Vector2(2, 0);
                            
                        }
                        if (playerLoc.x > this.transform.position.x)
                        {
                            myRig.velocity = new Vector2(1, 8);
                            SendUpdate("MOVE", "1,8");
                        }
                        else
                        {
                            myRig.velocity = new Vector2(-1, 8);
                            SendUpdate("MOVE", "-1,8");
                        }
                    }
                    yield return new WaitForSeconds(0.2f);
                }
                else //Engage patrol behavior. This involves bouncing randomly from left to right.
                {
                    if (myRig.velocity.y == 0)
                    {
                        int rand = Random.Range(0, 2);
                        if (rand == 0)
                        {
                            myRig.velocity = new Vector2(-1, 8); //???
                            SendUpdate("MOVE", "-1,8");

                        }
                        else
                        {
                            myRig.velocity = new Vector2(1, 8); //???
                            SendUpdate("MOVE", "1,8");
                        }
                        yield return new WaitForSeconds(1.3f);
                    }
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
        { // This is used to update the items that are needed the values that are needed for this enemy such as the hp and the like
            this.GetComponent<Animator>().SetInteger("HP", this.gameObject.GetComponent<Entity>().HP);
            this.GetComponent<Animator>().SetBool("ATTACKING", attacking);
            this.GetComponent<Animator>().SetBool("RELOADING", reloading);
            this.GetComponent<Animator>().SetBool("FIRED", fired); 
        }

        //Orient in the direction it is moving
        if (myRig.velocity.x > 0.1)
            right = true;
        else if (myRig.velocity.x < 0.1)
            right = false;
        this.GetComponent<SpriteRenderer>().flipX = right;
    }
    /*
    protected bool IsGrounded()
    {
        LayerMask layerMask = (1 << LayerMask.NameToLayer("EnvCollider"));
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, 1.0f, layerMask);
        if (hit.collider != null && hit.transform.CompareTag("Ground"))
        {
            return true;
        }
        return false;
    }
    */
    private void OnDestroy()
    {
        if(Random.Range(0,2) == 1 && IsServer)
            MyCore.NetCreateObject(13, -1, new Vector3(myRig.position.x, myRig.position.y, 0));
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}
