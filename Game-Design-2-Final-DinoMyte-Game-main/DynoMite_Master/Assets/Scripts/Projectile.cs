using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkComponent
{
    public int damage; //How much HP this projectile will remove from entities on contact
    public bool friendly; //If true, this is a player's projectile. If false, this is an enemy's projectile.
    public int owner; //Stores the connection ID of the owner. -1 for enemies.
    public int pierce; //If a projectile should not pierce, set this to 0.
    public bool noclip; //Determines whether this projectile interacts with the tilemap
    public float dieTime; //For projectiles that persist only for a certain amount of time

    //A lot goes into making a projectile follow the user.
    public bool follow; //Used by projectiles that are supposed to follow their senders
    public bool followLoose; //Used by the ptera
    public GameCharacter followEntity; //Object to follow
    public Vector2 followOffsetNegX; //Set in editor
    public Vector2 followOffsetPosX; //Set in editor
    public bool followLeft;
    public Vector2 followVel;
    public Color32 color = new Color32(255, 255, 255, 255);
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "PIERCE")
        {
            pierce = int.Parse(value);
            if (pierce < 0 && IsServer) //If the pierce is now below 0, destroy this projectile
                MyCore.NetDestroyObject(this.NetId);
            SendUpdate("PIERCE", value);
        }
    }
    public override IEnumerator SlowUpdate()
    {
        while (followLoose && IsServer) //Used by the Ptera minion
        {
            //Follow the player that summoned this minion if no enemies are nearby.
            bool enemiesNearby = false; //Assert that this is false
            Vector3 enemyLocation = new Vector3(-1500, -1500, 0); //Used to store enemy location if an enemy is found
            foreach(SpawnManager sm in GameObject.FindObjectsOfType<SpawnManager>()) //Only enemies have this script attached
            {
                if((sm.transform.position - this.transform.position).magnitude < 10f) //If any enemy is within 10 tiles
                {
                    enemiesNearby = true; //determines a change in behavior
                    enemyLocation = sm.transform.position;
                }
            }
            if(enemiesNearby) //Ptera will fly towards the enemy and attempt to deal damage
            {
                this.GetComponent<Rigidbody2D>().velocity = (enemyLocation - this.transform.position) * 1.5f;
                yield return new WaitForSeconds(.8f);
            } else //Ptera will follow the caster
            {
                if(followEntity != null)
                    this.GetComponent<Rigidbody2D>().velocity = (new Vector3(followEntity.transform.position.x, followEntity.transform.position.y + 1.5f, 0) - this.transform.position);
            }
            yield return new WaitForSeconds(.1f);
        }
        yield return new WaitForSeconds(.05f);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        //Players taking damage from enemies will use OnTriggerStay, so that enemies will constantly damage players that they are in contact with.
        if (!friendly && collision.gameObject.tag == "Player") //This code runs.
        {
            Entity player = collision.gameObject.GetComponent<Entity>();
            if (!player.immune)
                player.Damage(damage);
        }
        if (IsServer)
        {
            if (friendly && collision.gameObject.tag == "Enemy" && followLoose) //Fixes a bug where Ptera gets stuck inside enemies
            {
                if (gameObject.GetComponent<Rigidbody2D>().velocity.magnitude < 8f)
                    gameObject.GetComponent<Rigidbody2D>().velocity *= 1.1f;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Enemies taking damage from players will use OnTriggerEnter, so that enemies do not get harmed by the same projectile multiple times unintentionally.
        if(IsServer)
        {
            if (friendly && collision.gameObject.tag == "Enemy")
            {
                Entity enemy = collision.gameObject.GetComponent<Entity>();
                if (!enemy.immune)
                {
                    enemy.Damage(damage);
                    pierce--;
                    if (pierce < 0)
                        MyCore.NetDestroyObject(this.NetId);
                }     
            }
            //Handles enemy projectile piercing.
            if (!friendly && collision.gameObject.tag == "Player")
            {
                pierce--;
                if (pierce < 0)
                    MyCore.NetDestroyObject(this.NetId);
            }
            if (friendly && collision.gameObject.tag == "Ground" && !noclip && !followLoose) //Fixed a bug where Ptera was exhausting pierce by interacting with tiles
            {
                pierce--;
                if (pierce < 0)
                    MyCore.NetDestroyObject(this.NetId);
            }
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision) //Only physical collisions count for this
    {
        if (collision.gameObject.tag == "Ground" && !noclip && IsServer)
        {
            pierce--;
            if (pierce < 0)
                MyCore.NetDestroyObject(this.NetId); //For skulls, this lets them bounce!
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Update()
    {
        if (IsServer && follow) //Used by the attacks of Tyranno, Diplo, Ankylo, and Stego
        {
            if(followLeft)
                gameObject.GetComponent<Rigidbody2D>().position = (new Vector2(followEntity.transform.position.x, followEntity.transform.position.y)) + followOffsetNegX;
            else
                gameObject.GetComponent<Rigidbody2D>().position = (new Vector2(followEntity.transform.position.x, followEntity.transform.position.y)) + followOffsetPosX;
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(followVel.x, followVel.y*0.67f);
        }
        if(followLoose) //Used by the Ptera minion
        {
            if(IsClient)
                gameObject.GetComponent<SpriteRenderer>().flipX = (gameObject.GetComponent<Rigidbody2D>().velocity.x < 0); //Orient the Ptera the right way
        }
    }
    public IEnumerator Expire(float t)
    {
        yield return new WaitForSeconds(t);
        if (gameObject != null && IsServer)
        {
            pierce = -1;
            MyCore.NetDestroyObject(this.NetId);
        }
    }

    public override void NetworkedStart()
    {
        gameObject.GetComponent<SpriteRenderer>().color = color;
        if (dieTime > 0 && IsServer)
            StartCoroutine(Expire(dieTime));
        //throw new System.NotImplementedException();
    }
}
