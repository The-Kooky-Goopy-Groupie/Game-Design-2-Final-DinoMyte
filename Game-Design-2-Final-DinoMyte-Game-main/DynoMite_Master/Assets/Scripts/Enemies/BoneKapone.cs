using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneKapone : NetworkComponent
{
    public Vector3 playerLoc; //Keeps track of the nearest player.
    public Entity ent;
    public Animator anim;
    public bool facingLeft; //Used to determine which way Bone Kapone should be facing, and which direction bones should be launched in.

    //AI behavior flags. These also impact Bone Kapone's animator.
    public bool active;
    public bool attacking;
    public bool anticipation; //This is true when Bone Kapone is about to hide / unhide.
    public bool unhide; //animator variable 

    //Flags that are disabled when Bone Kapone reaches these HP values for the first time.
    public bool flag75 = true;
    public bool flag50 = true;
    public bool flag25 = true;
    public bool enemiesPresent = false;
    public bool allDead = false;
    public override void HandleMessage(string flag, string value)
    {
        //Only the client calls the following code.
        if(flag == "FLIP")
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = bool.Parse(value);
        }
        if (flag == "ACTIVE")
            anim.SetBool("ACTIVE", bool.Parse(value));
        if (flag == "ATTACKING")
            anim.SetBool("ATTACKING", bool.Parse(value));
        if (flag == "ANTICIPATION")
            anim.SetBool("ANTICIPATION", bool.Parse(value));
        if (flag == "UNHIDE")
            anim.SetBool("UNHIDE", bool.Parse(value));
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsServer) //Determines AI behavior. Only the server runs the following code.
        {
            if (GameObject.FindObjectOfType<MummyAI>() == null && GameObject.FindObjectOfType<GhostAI>() == null && GameObject.FindObjectOfType<SkeletonAI>() == null)
                enemiesPresent = false;
            else
                enemiesPresent = true;

            if(GameObject.FindObjectOfType<GameCharacter>() != null)
            {
                allDead = true;
                foreach(GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                {
                    if (gc.entity.HP > 0)
                        allDead = false;
                }
            }
            if(GameObject.FindObjectOfType<GameCharacter>() == null || allDead) //Bone Kapone's behavior when a player is present or all current players are dead.
            {
                facingLeft = true;
                gameObject.GetComponent<SpriteRenderer>().flipX = facingLeft;
                SendUpdate("FLIP", facingLeft.ToString());
                if (active)
                {
                    active = false; //Hides Bone Kapone.
                    anim.SetBool("ACTIVE", active);
                    SendUpdate("ACTIVE", active.ToString());
                }
                ent.immune = active; //Bone Kapone is invincible while underground.
                if((ent.HP < ent.maxHP) && !active)
                {
                    ent.Heal(5);
                    yield return new WaitForSeconds(4.8f); //Regenerates HP when not engaged in combat.
                }
            }
            else if(GameObject.FindObjectOfType<GameCharacter>() != null) //Bone Kapone's behavior when a player is present.
            {
                //Find the nearest player
                playerLoc = new Vector3(-1500, -1500, 0); //Enables Bone Kapone to continuously track players.
                foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                {
                    //If this player is closer than the last one
                    if ((this.transform.position - gc.transform.position).magnitude < (this.transform.position - playerLoc).magnitude) //Bone Kapone does not have a limit to range.
                    {
                        playerLoc = gc.transform.position; //This will be the player Bone Kapone targets.   
                    }
                }
                if (playerLoc.x <= this.transform.position.x) //Update animators for clients.
                    facingLeft = true;
                else
                    facingLeft = false;
                gameObject.GetComponent<SpriteRenderer>().flipX = facingLeft;
                SendUpdate("FLIP", facingLeft.ToString());
                
                if (!active && !enemiesPresent) //Wakes up Bone Kapone when no minions are alive.
                {
                    anticipation = true;
                    anim.SetBool("ANTICIPATION", anticipation);
                    SendUpdate("ANTICIPATION", anticipation.ToString());
                    yield return new WaitForSeconds(1f);
                    anticipation = false;
                    unhide = true;
                    anim.SetBool("ANTICIPATION", anticipation);
                    SendUpdate("ANTICIPATION", anticipation.ToString());
                    anim.SetBool("UNHIDE", unhide);
                    SendUpdate("UNHIDE", unhide.ToString());
                    yield return new WaitForSeconds(0.3f);
                    unhide = false;
                    anim.SetBool("UNHIDE", unhide);
                    SendUpdate("UNHIDE", unhide.ToString());
                    active = true;
                    anim.SetBool("ACTIVE", active);
                    SendUpdate("ACTIVE", active.ToString());
                }
                
                if (((float)ent.HP / (float)ent.maxHP) > 0.75f) //Above 75% life
                {
                    ent.immune = !active; //Bone Kapone is invincible while underground.
                    yield return new WaitForSeconds(4f);
                    attacking = true;
                    anim.SetBool("ATTACKING", attacking);
                    SendUpdate("ATTACKING", attacking.ToString());
                    if (facingLeft)
                    {
                        GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                        bone.GetComponent<Rigidbody2D>().velocity = new Vector2(-2, 0);
                    }
                    else
                    {
                        GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                        bone.GetComponent<Rigidbody2D>().velocity = new Vector2(2, 0);
                    }
                    yield return new WaitForSeconds(.25f);
                    attacking = false;
                    anim.SetBool("ATTACKING", attacking);
                    SendUpdate("ATTACKING", attacking.ToString());

                }
                else if (((float)ent.HP / (float)ent.maxHP) > 0.5f) //Above 50% life
                {
                    if(flag75) //Initiate an invulnerability phase that spawns mummies.
                    {
                        anticipation = true; //Makes Bone Kapone cry
                        anim.SetBool("ANTICIPATION", anticipation);
                        SendUpdate("ANTICIPATION", anticipation.ToString());
                        yield return new WaitForSeconds(1f);
                        anticipation = false;
                        active = false;
                        anim.SetBool("ANTICIPATION", anticipation);
                        SendUpdate("ANTICIPATION", anticipation.ToString());
                        anim.SetBool("ACTIVE", active);
                        SendUpdate("ACTIVE", active.ToString());
                        ent.immune = !active; //Bone Kapone is invincible while underground.
                        for (int i = 0; i < 5; i++)
                        {
                            if(Random.Range(0, 2) == 0)
                                MyCore.NetCreateObject(1, -1, this.transform.position + new Vector3(-8, 8, 0));
                            else
                                MyCore.NetCreateObject(1, -1, this.transform.position + new Vector3(8, 8, 0));
                            yield return new WaitForSeconds(1f);
                        }
                        flag75 = false;
                    }
                    if (active && !flag75)
                    {
                        ent.immune = !active; //Bone Kapone is invincible while underground.
                        yield return new WaitForSeconds(3f);
                        attacking = true;
                        anim.SetBool("ATTACKING", attacking);
                        SendUpdate("ATTACKING", attacking.ToString());
                        if (facingLeft)
                        {
                            if(Random.Range(0,4) == 0)
                            {
                                GameObject skull = MyCore.NetCreateObject(4, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                skull.GetComponent<Rigidbody2D>().velocity = new Vector2(-2.5f, 2.5f);
                            } else
                            {
                                GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                bone.GetComponent<Rigidbody2D>().velocity = new Vector2(-2.5f, 0);
                            }
                        }
                        else
                        {
                            if (Random.Range(0, 4) == 0)
                            {
                                GameObject skull = MyCore.NetCreateObject(4, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                skull.GetComponent<Rigidbody2D>().velocity = new Vector2(2.5f, 2.5f);
                            }
                            else
                            {
                                GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                bone.GetComponent<Rigidbody2D>().velocity = new Vector2(2.5f, 0);
                            }
                        }
                        yield return new WaitForSeconds(.25f);
                        attacking = false;
                        anim.SetBool("ATTACKING", attacking);
                        SendUpdate("ATTACKING", attacking.ToString());
                    }
                }
                else if (((float)ent.HP / (float)ent.maxHP) > 0.25f) //Above 25% life
                {
                    if (flag50) //Initiate an invulnerability phase that spawns ghosts.
                    {
                        anticipation = true; //Makes Bone Kapone cry
                        anim.SetBool("ANTICIPATION", anticipation);
                        SendUpdate("ANTICIPATION", anticipation.ToString());
                        active = false;
                        yield return new WaitForSeconds(1f);
                        anticipation = false;
                        anim.SetBool("ANTICIPATION", anticipation);
                        SendUpdate("ANTICIPATION", anticipation.ToString());
                        anim.SetBool("ACTIVE", active);
                        SendUpdate("ACTIVE", active.ToString());
                        ent.immune = !active; //Bone Kapone is invincible while underground.
                        for (int i = 0; i < 4; i++)
                        {
                            if (Random.Range(0, 2) == 0)
                                MyCore.NetCreateObject(2, -1, this.transform.position + new Vector3(-8, 0, 0));
                            else
                                MyCore.NetCreateObject(2, -1, this.transform.position + new Vector3(8, 0, 0));
                            yield return new WaitForSeconds(1f);
                        }
                        flag50 = false;
                    }
                    if (active && !flag50)
                    {
                        ent.immune = !active; //Bone Kapone is invincible while underground.
                        for (int i = 0; i < 4; i++)
                        {
                            playerLoc = new Vector3(-1500, -1500, 0); //Enables Bone Kapone to continuously track players.
                            foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                            {
                                if ((this.transform.position - gc.transform.position).magnitude < (this.transform.position - playerLoc).magnitude)
                                    playerLoc = gc.transform.position;
                            }
                            if (playerLoc.x <= this.transform.position.x) //Update animators for clients.
                                facingLeft = true;
                            else
                                facingLeft = false;
                            gameObject.GetComponent<SpriteRenderer>().flipX = facingLeft;
                            SendUpdate("FLIP", facingLeft.ToString());

                            yield return new WaitForSeconds(2f);
                            attacking = true;
                            anim.SetBool("ATTACKING", attacking);
                            SendUpdate("ATTACKING", attacking.ToString());
                            if (facingLeft)
                            {
                                if (Random.Range(0, 4) == 0)
                                {
                                    GameObject skull = MyCore.NetCreateObject(4, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                    skull.GetComponent<Rigidbody2D>().velocity = new Vector2(-2.5f, 2.5f);
                                }
                                else
                                {
                                    GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                    bone.GetComponent<Rigidbody2D>().velocity = new Vector2(-2.5f, 0);
                                }
                            }
                            else
                            {
                                if (Random.Range(0, 4) == 0)
                                {
                                    GameObject skull = MyCore.NetCreateObject(4, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                    skull.GetComponent<Rigidbody2D>().velocity = new Vector2(2.5f, 2.5f);
                                }
                                else
                                {
                                    GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                    bone.GetComponent<Rigidbody2D>().velocity = new Vector2(2.5f, 0);
                                }
                            }
                            yield return new WaitForSeconds(.25f);
                            attacking = false;
                            anim.SetBool("ATTACKING", attacking);
                            SendUpdate("ATTACKING", attacking.ToString());
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-10 + (4*i), 15f, 0));
                            bone.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -5);
                        }
                        yield return new WaitForSeconds(3f);
                        for (int i = 0; i < 4; i++)
                        {
                            GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-8f + (4*i), 15f, 0));
                            bone.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -5);
                        }
                    }
                } else //Final 25% HP
                {
                    if (flag25) //Initiate an invulnerability phase that spawns skeletons.
                    {
                        anticipation = true; //Makes Bone Kapone cry
                        anim.SetBool("ANTICIPATION", anticipation);
                        SendUpdate("ANTICIPATION", anticipation.ToString());
                        active = false;
                        yield return new WaitForSeconds(1f);
                        anticipation = false;
                        anim.SetBool("ANTICIPATION", anticipation);
                        SendUpdate("ANTICIPATION", anticipation.ToString());
                        anim.SetBool("ACTIVE", active);
                        SendUpdate("ACTIVE", active.ToString());
                        ent.immune = !active; //Bone Kapone is invincible while underground.
                        for (int i = 0; i < 3; i++)
                        {
                            if (Random.Range(0, 2) == 0)
                                MyCore.NetCreateObject(3, -1, this.transform.position + new Vector3(-8, 8, 0));
                            else
                                MyCore.NetCreateObject(3, -1, this.transform.position + new Vector3(8, 8, 0));
                            yield return new WaitForSeconds(1f);
                        }
                        flag25 = false;
                    } 
                    if(active && !flag25)
                    {
                        ent.immune = !active; //Bone Kapone is invincible while underground.
                        for(int i = 0; i < 4; i++)
                        {
                            playerLoc = new Vector3(-1500, -1500, 0); //Enables Bone Kapone to continuously track players.
                            foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                            {
                                if ((this.transform.position - gc.transform.position).magnitude < (this.transform.position - playerLoc).magnitude)
                                    playerLoc = gc.transform.position;
                            }
                            if (playerLoc.x <= this.transform.position.x) //Update animators for clients.
                                facingLeft = true;
                            else
                                facingLeft = false;
                            gameObject.GetComponent<SpriteRenderer>().flipX = facingLeft;
                            SendUpdate("FLIP", facingLeft.ToString());

                            yield return new WaitForSeconds(1f);
                            attacking = true;
                            anim.SetBool("ATTACKING", attacking);
                            SendUpdate("ATTACKING", attacking.ToString());
                            if (facingLeft)
                            {
                                if (Random.Range(0, 4) == 0)
                                {
                                    GameObject skull = MyCore.NetCreateObject(4, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                    skull.GetComponent<Rigidbody2D>().velocity = new Vector2(-2.5f, 2.5f);
                                }
                                else
                                {
                                    GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                    bone.GetComponent<Rigidbody2D>().velocity = new Vector2(-2.5f, 0);
                                }
                            }
                            else
                            {
                                if (Random.Range(0, 4) == 0)
                                {
                                    GameObject skull = MyCore.NetCreateObject(4, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                    skull.GetComponent<Rigidbody2D>().velocity = new Vector2(2.5f, 2.5f);
                                }
                                else
                                {
                                    GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                    bone.GetComponent<Rigidbody2D>().velocity = new Vector2(2.5f, 0);
                                }
                            }
                            yield return new WaitForSeconds(.25f);
                            attacking = false;
                            anim.SetBool("ATTACKING", attacking);
                            SendUpdate("ATTACKING", attacking.ToString());
                        }
                        for(int i = 0; i < 15; i++)
                        {
                            GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-10 + i, 15f, 0));
                            bone.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -5);
                            yield return new WaitForSeconds(0.2f);
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            playerLoc = new Vector3(-1500, -1500, 0); //Enables Bone Kapone to continuously track players.
                            foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                            {
                                //If this player is closer than the last one
                                if ((this.transform.position - gc.transform.position).magnitude < (this.transform.position - playerLoc).magnitude) //Bone Kapone does not have a limit to range.
                                {
                                    playerLoc = gc.transform.position; //This will be the player Bone Kapone targets.   
                                }
                            }
                            if (playerLoc.x <= this.transform.position.x) //Update animators for clients.
                                facingLeft = true;
                            else
                                facingLeft = false;
                            gameObject.GetComponent<SpriteRenderer>().flipX = facingLeft;
                            SendUpdate("FLIP", facingLeft.ToString());

                            yield return new WaitForSeconds(1f);
                            attacking = true;
                            anim.SetBool("ATTACKING", attacking);
                            SendUpdate("ATTACKING", attacking.ToString());
                            if (facingLeft)
                            {
                                GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(-1, -0.75f, 0));
                                bone.GetComponent<Rigidbody2D>().velocity = new Vector2(-4, 0);
                            }
                            else
                            {
                                GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(1, -0.75f, 0));
                                bone.GetComponent<Rigidbody2D>().velocity = new Vector2(4, 0);
                            }
                            yield return new WaitForSeconds(.25f);
                            attacking = false;
                            anim.SetBool("ATTACKING", attacking);
                            SendUpdate("ATTACKING", attacking.ToString());
                        }
                        for (int i = 0; i < 15; i++)
                        {
                            GameObject bone = MyCore.NetCreateObject(16, -1, this.transform.position + new Vector3(10 - i, 15f, 0));
                            bone.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -5);
                            yield return new WaitForSeconds(0.2f);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(.05f);
        }
        yield return new WaitForSeconds(.05f);
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
