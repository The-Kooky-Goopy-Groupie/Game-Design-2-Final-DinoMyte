using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;
//
public class Entity : NetworkComponent // the main classes that is used for the various living enities withing the games
{
    public int HP; // What Out of the Max Hp is the value Set to 
    public int maxHP; //How much damage this entity can take
    public int atk; //How much damage this entity deals to other entities
    public float immunityTime; //How long you are immune
    public bool immune = false; //Used to determine whether to take damage
    public bool hurt = false;
    public Color32 color; //Used to display immunity time
    public Color32 flash; //Used to display immunity time

    public Slider hpBar; // Used to determine how big the hp bar should be?
    public Slider UIHP;

    public Animator myAnim; //Used to show death animations
    public SpriteRenderer spR;
    public AudioSource hurtSound;
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "HP") // if flag is hp for either server or client 
        {
            HP = int.Parse(value);
            if (HP > maxHP) // set to hp value that is corrent
                HP = maxHP;
            if (HP < 0) // if it's a negative value set it to 0
                HP = 0;
            hpBar.value = ((float)HP / (float)maxHP); // update the HP Bar visual
            if(UIHP != null) //catches an exception for enemies
                UIHP.value = (HP*0.72f) + 2.8f;
            if(IsServer)
                SendUpdate("HP", value); // pretty  sure this is to update the hp values on server end then send back to the clients
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsLocalPlayer && GameObject.FindObjectOfType<PlayerManager>() != null)
        {
            if (GameObject.FindObjectOfType<PlayerManager>().CurrentScene != 1)
                HP = GameObject.FindObjectOfType<PlayerManager>().HP;
            else
                HP = maxHP;
        }
        if(IsLocalPlayer)
            SendCommand("HP", HP.ToString()); //Send an HP update when initializing an object
        if (IsServer && !IsLocalPlayer)
            SendUpdate("HP", HP.ToString());
        if(IsServer)
        {
            foreach(Entity e in GameObject.FindObjectsOfType<Entity>())
            {
                e.IsDirty = true;
            }
            if(gameObject.tag == "Enemy") //This fixes a bug where enemies required one hit in order to take damage.
            {
                HP = 500;
                if (HP > maxHP) // set to hp value that is corrent
                    HP = maxHP;
                if (HP < 0) // if it's a negative value set it to 0
                    HP = 0;
            }
        }
        if (myAnim == null)
        {
            if (gameObject.tag != "Player") //If this is not a player object
            {
                myAnim = this.gameObject.GetComponent<Animator>(); // gets the animator
            }
        }
        //This code only plays once, letting the spawn animation play out.
        yield return new WaitForSeconds(1f);
        if(myAnim != null)
            myAnim.SetBool("SPAWN", false); //Stop playing the spawn animation
        while (true)
        {
            if (IsLocalPlayer && GameObject.FindObjectOfType<PlayerManager>() != null)
            {
                if (GameObject.FindObjectOfType<PlayerManager>().CurrentScene == 1)
                    GameObject.FindObjectOfType<PlayerManager>().SetHP(maxHP);
                else
                    GameObject.FindObjectOfType<PlayerManager>().SetHP(HP);
            }
            while (immune && hurt)
            {
                if(spR != null)
                {
                    if (spR.color.a > 0)
                        spR.color = flash;
                    else
                        spR.color = color;
                }
                yield return new WaitForSeconds(.1f);
            }
            if (HP == 0 && gameObject.tag != "Player") //Only enemies will call the following code when their HP drops to 0
            {
                atk = 0; //Nullify their damage
                yield return new WaitForSeconds(1f); //Lets enemy death animation play
                Death(); //Destroy the object
            }
            if(IsServer && IsDirty)
            {
                SendUpdate("HP", HP.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Start()
    {
        // not used yet
    }

    void Update()
    {
        if(IsClient && myAnim != null)
        {
            myAnim.SetInteger("HP", HP); //Updates all animators
        }
        if(IsServer)
        {
            if(IsDirty)
            {
                SendUpdate("HP", HP.ToString());
                IsDirty = false;
            }
        }
    }

    public virtual void Death()
    {
        if (gameObject.tag != "Player")
            MyCore.NetDestroyObject(this.NetId); // Destroys the Object when an entity dies, unless they are a player
    }
    public IEnumerator Immunity()
    {
        immune = true; hurt = true;
        yield return new WaitForSeconds(immunityTime);
        immune = false; hurt = false;
        if(spR != null)
            spR.color = color;
    }
    public void Heal(int h) //Heal this entity
    {
        if(gameObject.GetComponent<GameCharacter>() != null) //Run by players!
            SendCommand("HP", (HP + h).ToString());
        else //Run by Bone Kapone.
        {
            SendUpdate("HP", (HP + h).ToString()); //This updates the HP for clients.
            HP = HP + h; //This updates the HP on the server.
            if (HP > maxHP) // set to hp value that is corrent
                HP = maxHP;
            if (HP < 0) // if it's a negative value set it to 0
                HP = 0;
            hpBar.value = ((float)HP / (float)maxHP); // update the HP Bar visual
        }
    }
    public void Damage(int d) //Damage this entity
    {
        if(d > 0 && HP > 0 && !immune)
        {
            hurtSound.Play();
            if(gameObject.GetComponent<GameCharacter>() != null) //Run by players!
                SendCommand("HP", (HP - d).ToString());
            else //Run by enemies.
            {
                if(IsServer)
                {
                    SendUpdate("HP", (HP - d).ToString()); //This updates the HP for clients.
                    HP = HP - d; //This updates the HP on the server.
                    if (HP > maxHP) // set to hp value that is corrent
                        HP = maxHP;
                    if (HP < 0) // if it's a negative value set it to 0
                        HP = 0;
                }
                
            }
            //Either way, update the hpBar.
            hpBar.value = ((float)HP / (float)maxHP); // update the HP Bar visual
            StartCoroutine(Immunity());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(gameObject.tag == "Player" && collision.gameObject.tag == "Enemy")
        {
            //This is called in the event that an enemy is contacting this player, and makes them take dmg equal to the enemy atk value
            if(!immune)
            {
                int enemyAtk = collision.GetComponent<Entity>().atk;
                Damage(enemyAtk);
            }
        }
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}
