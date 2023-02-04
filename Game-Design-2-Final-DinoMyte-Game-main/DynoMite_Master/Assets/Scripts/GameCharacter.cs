using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class GameCharacter : NetworkComponent
{
    public string Pname;
    public Rigidbody2D MyRig;

    public Text nameDisplay; // used to display a players name
    //public Text debugDisplay;
    
    public bool isGrounded = true; //checks if they are on the ground
    public bool canAttack = true; //checks if they can attack
    public Vector2 LastMove; // the last movement they made
    public float speed = 3.5f; // speed of the player
    //public int Color;
    //public Text MyTextbox;
    //public int Score = 0;
    public int DinoClass; // type of dino player is 
    public int hat; // players hat
    public int skin; // players color
    public int rememberLast; //Used to reset player if they fall out of the map - respawns them at the last door they were at

    public GameObject class_1;
    public GameObject class_2;
    public GameObject class_3;
    public GameObject class_4;
    public GameObject class_5;
    public GameObject myAnimator;

    public GameObject myHat; //
    public Entity entity; //
    public GameObject hud;
    public GameObject escMenu;

    public int powerups0 = 0; //number of HP powerups
    public int powerups1 = 0; //number of SPD powerups
    public int powerups2 = 0; //number of ATK powerups

    //respective buttons
    public Button power0;
    public Button power1;
    public Button power2;
    public GameObject HPBar;
    public GameObject LoadScreen;

    public AudioSource pickupPowerSound;
    public AudioSource usePowerSound;
    public AudioSource jumpSound;
    public AudioSource attack1Sound;
    public AudioSource attack2Sound;

    public bool timeToDie = false; //Used by players that have undergone scene transitions

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "PNAME")
        {
            Pname = value;
            nameDisplay.text = value;
            if (IsServer)
            {
                SendUpdate("PNAME", Pname);
            }
        }
        if(flag == "HAT") 
        {
            hat = int.Parse(value);
            if (hat < 0)
                hat = 24;
            if (hat > 24)
                hat = 0;

            if (hat == 0)
            {
                myHat.SetActive(false);
            }
            else
            {
                myHat.SetActive(true);
                myHat.GetComponent<Animator>().SetInteger("HAT", hat);
            }
            if (IsServer)
            {
                SendUpdate("HAT", hat.ToString());
            }
        }
        if (flag == "SKIN") // This is used to change the color of the player
        {
            skin = int.Parse(value);
            if (skin < 0)
                skin = 7;
            if (skin > 7)
                skin = 0;
            Color32 color = new Color32();
            switch(skin)
            {
                case 0:// default lime - KEEP - Achived by using the White Color at max alpha with the color of the sprites
                    color = new Color32(255, 255, 255, 255);
                    break;
                case 1:// Default lime used to color - KEEP - (gives it a green tint that looks like a gameboy)
                    color = new Color32(148, 227, 68, 255);
                    break;
                case 2://Green Green - Honestly Looks pretty good So Keeping But likely moving
                    color = new Color32(0, 255, 0, 255);
                    break;
                case 3://red - now looks more like an orange tint, not the best but hey every game needs that one bad skin
                    color = new Color32(200, 20, 20, 255);
                    break;
                case 4:// The BETTER YELLOW -KEEP -
                    color = new Color32(255, 255, 0, 255);
                    break;
                case 5:// Dark - KEEP - Emulates that smash bros darken character effect
                    color = new Color32(95, 95, 95, 255);
                    break;
                case 6://Blue - LOOKS FUNKY IN A COOL WAY
                    color = new Color32(44, 122, 255, 255);
                    break;
                case 7:// Cyan - KEEP - Looks really good with the base color combination to give it a nice bluish green
                    color = new Color32(68, 253, 242, 255);
                    break;
            }
            if (myAnimator == null)
            {
                switch (DinoClass) //This assigns the player's animator 
                {
                    case 1:
                        myAnimator = class_1;
                        break;
                    case 2:
                        myAnimator = class_2;
                        break;
                    case 3:
                        myAnimator = class_3;
                        break;
                    case 4:
                        myAnimator = class_4;
                        break;
                    case 5:
                        myAnimator = class_5;
                        break;
                }
            }
            myAnimator.GetComponent<SpriteRenderer>().color = color;
            gameObject.GetComponent<Entity>().myAnim = myAnimator.GetComponent<Animator>();
            gameObject.GetComponent<Entity>().spR = myAnimator.GetComponent<SpriteRenderer>();
            gameObject.GetComponent<Entity>().color = myAnimator.GetComponent<SpriteRenderer>().color;
            if (IsServer)
            {
                SendUpdate("SKIN", skin.ToString());
            }
        }
        if (flag == "DCLASS")
        {
            DinoClass = int.Parse(value);
            //Insert code here that sets the dinosaur class.
            switch(DinoClass)
            {
                case 1:
                    class_1.SetActive(true);
                    break;
                case 2:
                    class_2.SetActive(true);
                    break;
                case 3:
                    class_3.SetActive(true);
                    break;
                case 4:
                    class_4.SetActive(true);
                    break;
                case 5:
                    class_5.SetActive(true);
                    break;
            }
            //if (IsServer)
            {
                SendUpdate("DCLASS", value);
            }
        }
        if (flag == "MOVE" && IsServer)
        {
            LastMove = new Vector2(float.Parse(value), 0) * speed;
            SendUpdate("MOVE", value);
        }
        if(flag == "JUMP" && isGrounded && IsServer)
        {
            MyRig.velocity = new Vector2(MyRig.velocity.x, float.Parse(value));
            isGrounded = false;
            SendUpdate("JUMP", value);
        }
        if(flag == "REMEMBER")
        {
            rememberLast = int.Parse(value);
            SendUpdate("REMEMBER", value);
        }
        if (flag == "SCENE_INFO")
        {
            GameObject temp = GameObject.Find(value); //0_1
            if (temp != null)
            {
                this.transform.position = temp.transform.position;
            }
        }
        if (flag == "ATTACK")
        {
            int dC = int.Parse(value);
            myAnimator.GetComponent<Animator>().SetBool("ATTACK", true);
            canAttack = false;
            SendUpdate("ATTACK", value);
            StartCoroutine(Attack(dC));
        }
        if(flag == "REVIVE")
        {
            foreach(GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
            {
                if(gc.entity.HP <= 0 && (gc.transform.position - this.transform.position).magnitude < 2.5f) //If the player is dead and within sufficient range
                {
                    gc.entity.Heal(gc.entity.maxHP / 4); //Revive them with 25% HP
                }
            }
            SendUpdate("REVIVE", "0");
        }
        if(flag == "GAINPOWER")
        {
            if(IsServer)    
                MyCore.NetDestroyObject(int.Parse(value));
        }
        if(flag == "USEPOWER")
        {
            usePowerSound.Play();
            //Make the powerup affect the user
            if (value == "0")
                entity.Heal(5);
            else
                StartCoroutine(UsePower(int.Parse(value)));
            //Additionally, powerups will affect nearby players
            foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
            {
                if ((gc.transform.position - this.transform.position).magnitude < 15f) //If the player is within sufficient range
                {
                    if (value == "0")
                        gc.entity.Heal(5); //Full heal
                    else
                        gc.StartCoroutine(UsePower(int.Parse(value))); //Power up player

                }
            }
            SendUpdate("USEPOWER", value);
        }
        if(flag == "DESTROY")
        {
            timeToDie = true;
            SendUpdate("DESTROY", value);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsLocalPlayer)
        {
            SendCommand("PNAME", GameObject.FindObjectOfType<PlayerManager>().PNAME);
            SendCommand("DCLASS", GameObject.FindObjectOfType<PlayerManager>().DINOCLASS.ToString());
            SendCommand("HAT", (GameObject.FindObjectOfType<PlayerManager>().HAT.ToString()));
            SendCommand("SKIN", (GameObject.FindObjectOfType<PlayerManager>().SKIN.ToString()));
            entity.Heal(0);
            SendCommand("SCENE_INFO", GameObject.FindObjectOfType<PlayerManager>().LastScene.ToString() + "_" + GameObject.FindObjectOfType<PlayerManager>().CurrentScene.ToString());
            SendCommand("REMEMBER", GameObject.FindObjectOfType<PlayerManager>().LastScene.ToString());
            GameObject.FindObjectOfType<PlayerManager>().LastScene = GameObject.FindObjectOfType<PlayerManager>().CurrentScene;
            hud.SetActive(true);
            HPBar.SetActive(false);
        }
        if(IsServer)
        {
            foreach (GameCharacter gc in GameObject.FindObjectsOfType<GameCharacter>())
                gc.IsDirty = true;
        }
        while (true)
        {
            // New Grounded Check Function ~Janar
            if (IsGrounded())
            {
                isGrounded = true;
            }
            if (MyRig.position.y < -20)
            {
                //This resets the player if they fall out of the map
                MyRig.position = GameObject.Find(rememberLast.ToString() + "_" + GameObject.FindObjectOfType<PlayerManager>().CurrentScene.ToString()).transform.position;
                entity.Damage(1);
            }
            //Player controller
            if (IsLocalPlayer && entity.HP > 0 && !escMenu.activeInHierarchy) //Only allow the player to interact with the world if they are alive
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                SendCommand("MOVE", h.ToString());
                if (Input.GetAxisRaw("Jump") > 0 && isGrounded)
                {
                    jumpSound.Play();
                    if (GameObject.FindObjectOfType<PlayerManager>().CurrentScene == 5)
                        SendCommand("JUMP", "6");
                    else
                        SendCommand("JUMP", "10");
                    isGrounded = false;
                }
                if (Input.GetAxisRaw("Fire1") > 0 && canAttack)
                {
                    SendCommand("ATTACK", DinoClass.ToString());
                    canAttack = false;
                }
                //Eventually add a condition here that checks if the player is at the hat shop.
                if (v != 0 && GameObject.FindObjectOfType<PlayerManager>().CurrentScene == 1) //Only if we are at the Diner
                {
                    if (MyRig.position.x < -5 && MyRig.position.x > -9) //If we are at the HAT SHOP
                    {
                        SendCommand("HAT", (hat + v).ToString());
                        GameObject.FindObjectOfType<PlayerManager>().SetHat(hat + (int)v);
                        yield return new WaitForSeconds(0.5f);
                    }
                    else if (MyRig.position.x < 2 && MyRig.position.x > -2) //If we are at the SKIN SHOP
                    {
                        SendCommand("SKIN", (skin + v).ToString());
                        GameObject.FindObjectOfType<PlayerManager>().SetSkin(skin + (int)v);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                //Mechanic that allows dinosaurs to revive each other.
                if(v > 0 && GameObject.FindObjectOfType<PlayerManager>().CurrentScene != 1) //Press UP arrow to revive teammates
                {
                    SendCommand("REVIVE", "0");
                }
            }
            else if(IsLocalPlayer && (entity.HP <= 0 || escMenu.activeInHierarchy))
                SendCommand("MOVE", "0");
            if (IsServer)
            {
                if (IsDirty) // the check if is dirty section
                {
                    SendUpdate("PNAME", Pname);
                    SendUpdate("DCLASS", DinoClass.ToString());
                    SendUpdate("HAT", hat.ToString());
                    SendUpdate("SKIN", skin.ToString());
                    IsDirty = false;
                }
                if(timeToDie)
                {
                    yield return new WaitForSeconds(5f);
                    MyCore.NetDestroyObject(this.NetId);
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    //Collisions
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Finish" && IsLocalPlayer && timeToDie)
        {
            MyCore.Disconnect(MyCore.LocalPlayerId);
            int desiredScene = int.Parse(collision.name.Split('_')[1]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(desiredScene);
        }
    }
    //Triggers
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Finish" && IsLocalPlayer)
        {
            SendCommand("DESTROY", this.NetId.ToString());
        }
        if(other.tag == "Powerup" && IsLocalPlayer)
        {
            pickupPowerSound.Play();
            //if(IsLocalPlayer)
            {
                int t = other.GetComponent<Powerup>().type;
                if (t == 0)
                    powerups0++;
                else if (t == 1)
                    powerups1++;
                else
                    powerups2++;
                SendCommand("GAINPOWER", other.GetComponent<NetworkID>().NetId.ToString());
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(myAnimator != null)
        {
            if (MyRig.velocity.x < -0.5 && canAttack) //Players shouldn't be able to change directions while attacking
                myAnimator.GetComponent<SpriteRenderer>().flipX = true;
            if (MyRig.velocity.x > 0.5 && canAttack) //Using an else here caused an annoying bug where the default direction to face was right.
                myAnimator.GetComponent<SpriteRenderer>().flipX = false;
            //The following lines of code fix the Diplodocus not being centered in their attack animation.
            if (myAnimator.GetComponent<SpriteRenderer>().flipX && myAnimator.GetComponent<Animator>().GetBool("ATTACK"))
                class_3.transform.position = this.gameObject.transform.position + new Vector3(-1, 0.5f, 0);
            else if (!myAnimator.GetComponent<SpriteRenderer>().flipX && myAnimator.GetComponent<Animator>().GetBool("ATTACK"))
                class_3.transform.position = this.gameObject.transform.position + new Vector3(1, 0.5f, 0);
            else
                class_3.transform.position = this.gameObject.transform.position + new Vector3(0, 0.5f, 0);
        }
        if (IsServer)
        {
            if (entity.HP > 0)
                MyRig.velocity = LastMove + new Vector2(0, MyRig.velocity.y);
            else
                MyRig.velocity = new Vector2(0, MyRig.velocity.y);
        }
        if(IsClient)
        {
            //Get animator here
            if(myAnimator == null) // used to get the aniamtor and then.... idk class_ doesn't tell me that much
            {
                switch(DinoClass)
                {
                    case 1:
                        myAnimator = class_1;
                        break;
                    case 2:
                        myAnimator = class_2;
                        break;
                    case 3:
                        myAnimator = class_3;
                        break;
                    case 4:
                        myAnimator = class_4;
                        break;
                    case 5:
                        myAnimator = class_5;
                        break;
                }
            } else
            {
                //Manipulate animator here

                //Variables: MOVE, AIR, ATTACK, YVEL (float)
                Animator myAnim = myAnimator.GetComponent<Animator>();
                myAnim.SetFloat("YVEL", MyRig.velocity.y);

                if (Mathf.Abs(MyRig.velocity.y) <= 0.01f) //This is more reliable than isGrounded for animators.
                    myAnim.SetBool("AIR", false);
                else
                    myAnim.SetBool("AIR", true);
                if (Mathf.Abs(MyRig.velocity.x) <= 0.01f) //If x vel is close to 0
                    myAnim.SetBool("MOVE", false);
                else
                    myAnim.SetBool("MOVE", true);
            }
        }
        if (IsLocalPlayer) //Put camera follow here!
        {
            bool paused = escMenu.activeInHierarchy;
            //Pause menu controls here
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(paused)
                    escMenu.SetActive(false);
                else
                    escMenu.SetActive(true);
            }
            if (paused && Input.GetKeyDown(KeyCode.E))
                Diner();
            else if (paused && Input.GetKeyDown(KeyCode.Q))
                MainMenu();

            if (Input.GetKeyDown(KeyCode.Alpha1) && powerups0 > 0)
            {
                SendCommand("USEPOWER", "0");
                powerups0 -= 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && powerups1 > 0)
            {
                SendCommand("USEPOWER", "1");
                powerups1 -= 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && powerups2 > 0)
            {
                SendCommand("USEPOWER", "2");
                powerups2 -= 2;
            }

            if (GameObject.FindObjectOfType<PlayerManager>().CurrentScene == 5)
                Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 3, -5);
            else
                Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1, -5);
            if (GameObject.FindObjectOfType<PlayerManager>().CurrentScene == 1) //If this is the Dino Diner
                Camera.main.orthographicSize = 3; //Provide a closer zoom
            else if(GameObject.FindObjectOfType<PlayerManager>().CurrentScene == 5)
                Camera.main.orthographicSize = 7;
            else
                Camera.main.orthographicSize = 5;
            Camera.main.GetComponent<AudioSource>().mute = false;

            power0.interactable = (powerups0 > 0);
            power1.interactable = (powerups1 > 0);
            power2.interactable = (powerups2 > 0);
        }
    }
    public IEnumerator UsePower(int p)
    {
        float temp = speed;
        if (p == 1)
            speed += 1;
        else if (p == 2 && IsServer)
        {
            GameObject ptera = MyCore.NetCreateObject(14, this.NetId, transform.position);
            ptera.GetComponent<Projectile>().followEntity = this;
        }
        yield return new WaitForSeconds(15f);
        if (p == 1)
            speed = temp;
    }
    public IEnumerator Attack(int c)
    {
        myAnimator.GetComponent<Animator>().SetBool("ATTACK", true);
        if (c == 1) //Tyranno attack code.
        {
            yield return new WaitForSeconds(0.4f);
            entity.immune = true;
            attack1Sound.Play();
            if (IsServer)
            {
                if (myAnimator.GetComponent<SpriteRenderer>().flipX)
                {
                    GameObject bite = MyCore.NetCreateObject(5, -1, this.transform.position + new Vector3(0, 0.5f, 0));
                    bite.transform.position = this.transform.position + new Vector3(-2, 0.5f, 0);
                    bite.GetComponent<Projectile>().damage += entity.atk;
                    bite.GetComponent<Projectile>().followEntity = this;
                    bite.GetComponent<Projectile>().followLeft = myAnimator.GetComponent<SpriteRenderer>().flipX;
                    bite.GetComponent<Projectile>().followVel = MyRig.velocity;
                } else
                {
                    GameObject bite = MyCore.NetCreateObject(5, -1, this.transform.position + new Vector3(0, 0.5f, 0));
                    bite.GetComponent<Projectile>().damage += entity.atk;
                    bite.GetComponent<Projectile>().followEntity = this;
                    bite.GetComponent<Projectile>().followLeft = myAnimator.GetComponent<SpriteRenderer>().flipX;
                    bite.GetComponent<Projectile>().followVel = MyRig.velocity;
                }
            }
            yield return new WaitForSeconds(0.5f);
            entity.immune = false;
            canAttack = true;
        } else if (c==2) //Raptor attack code.
        {
            yield return new WaitForSeconds(0.3f);
            attack1Sound.Play();
            if (IsServer)
            {
                if (myAnimator.GetComponent<SpriteRenderer>().flipX)
                {
                    GameObject bubble = MyCore.NetCreateObject(6, -1, this.transform.position + new Vector3(-1.5f, 0.25f, 0));
                    bubble.GetComponent<Projectile>().color = myAnimator.GetComponent<SpriteRenderer>().color;
                    bubble.GetComponent<Projectile>().damage += entity.atk;
                    bubble.GetComponent<Rigidbody2D>().velocity = new Vector2(-5,0);
                }
                else
                {
                    GameObject bubble = MyCore.NetCreateObject(6, -1, this.transform.position + new Vector3(1.5f, 0.25f, 0));
                    bubble.GetComponent<Projectile>().color = myAnimator.GetComponent<SpriteRenderer>().color;
                    bubble.GetComponent<Projectile>().damage += entity.atk;
                    bubble.GetComponent<Rigidbody2D>().velocity = new Vector2(5, 0);
                }
            }
            yield return new WaitForSeconds(0.1f);
            canAttack = true;
        }
        else if (c == 3) //Diplo attack code.
        {
            yield return new WaitForSeconds(0.3f);
            entity.immune = true;
            attack1Sound.Play();
            if (IsServer)
            {
                if (myAnimator.GetComponent<SpriteRenderer>().flipX)
                {
                    GameObject neck = MyCore.NetCreateObject(7, -1, this.transform.position + new Vector3(-2, 0.5f, 0));
                    neck.transform.position = this.transform.position + new Vector3(-2, 0.5f, 0);
                    neck.GetComponent<Projectile>().damage += entity.atk;
                    neck.GetComponent<Projectile>().followEntity = this;
                    neck.GetComponent<Projectile>().followLeft = myAnimator.GetComponent<SpriteRenderer>().flipX;
                    neck.GetComponent<Projectile>().followVel = MyRig.velocity;
                }
                else
                {
                    GameObject neck = MyCore.NetCreateObject(7, -1, this.transform.position + new Vector3(1, 0.5f, 0));
                    neck.transform.position = this.transform.position + new Vector3(1, 0.5f, 0);
                    neck.GetComponent<Projectile>().damage += entity.atk;
                    neck.GetComponent<Projectile>().followEntity = this;
                    neck.GetComponent<Projectile>().followLeft = myAnimator.GetComponent<SpriteRenderer>().flipX;
                    neck.GetComponent<Projectile>().followVel = MyRig.velocity;
                }
            }
            yield return new WaitForSeconds(0.4f);
            entity.immune = false;
            canAttack = true;
        } else if (c==4) //Ankylo attack code.
        {
            yield return new WaitForSeconds(0.2f);
            entity.immune = true;
            attack2Sound.Play();
            if (IsServer)
            {
                GameObject spin = MyCore.NetCreateObject(8, -1, this.transform.position + new Vector3(0, 0.5f, 0));
                spin.transform.position = this.transform.position + new Vector3(0, 0.5f, 0);
                spin.GetComponent<Projectile>().damage += entity.atk;
                spin.GetComponent<Projectile>().followEntity = this;
                spin.GetComponent<Projectile>().followLeft = myAnimator.GetComponent<SpriteRenderer>().flipX;
                spin.GetComponent<Projectile>().followVel = MyRig.velocity;
            }
            yield return new WaitForSeconds(0.9f);
            entity.immune = false;
            canAttack = true;
        } else if (c==5) //Stego attack code.
        {
            float temp = speed;
            speed = 1f;
            attack2Sound.Play();
            if (IsServer)
            {
                GameObject spin = MyCore.NetCreateObject(9, -1, this.transform.position + new Vector3(0, 0.5f, 0));
                spin.transform.position = this.transform.position + new Vector3(0, 0.5f, 0);
                spin.GetComponent<Projectile>().damage += entity.atk;
                spin.GetComponent<Projectile>().followEntity = this;
                spin.GetComponent<Projectile>().followLeft = myAnimator.GetComponent<SpriteRenderer>().flipX;
                spin.GetComponent<Projectile>().followVel = MyRig.velocity;
            }
            yield return new WaitForSeconds(0.4f);
            speed = temp;
            canAttack = true;
        }
        else //This code shouldn't be called but it's here in case something goes wrong.
        {
            yield return new WaitForSeconds(1f);
        }
        myAnimator.GetComponent<Animator>().SetBool("ATTACK", false);
        canAttack = true;
    }
    protected bool IsGrounded() // used to check if the player is on the ground or not.
    {
        LayerMask layerMask = (1 << LayerMask.NameToLayer("EnvCollider"));
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position , Vector2.down, 1.0f, layerMask); // raycast scan to check if there ground below the character
        if (hit.collider != null && hit.transform.CompareTag("Ground")) // check if it hits the ground
        {
            return true;
        }
        return false; // otherwise it is false
    }

    public void Resume()
    {
        //if(IsLocalPlayer)
        {
            escMenu.SetActive(false);
        }
    }
    public IEnumerator Return(int n)
    {
        while(IsLocalPlayer && !timeToDie) //This code will continue to wait until timeToDie is set
        {
            yield return new WaitForSeconds(.05f);
        }
        if(n != 0)
        {
            MyCore.Disconnect(MyCore.LocalPlayerId);
            UnityEngine.SceneManagement.SceneManager.LoadScene(n);
        }
        else
        {
            if(IsLocalPlayer)
                Application.Quit();
        }
    }
    public void Diner()
    {
        if(IsLocalPlayer)
        {
            SendCommand("DESTROY", this.NetId.ToString());
            StartCoroutine(Return(1));
        }
    }
    public void MainMenu()
    {
        if (IsLocalPlayer)
        {
            SendCommand("DESTROY", this.NetId.ToString());
            StartCoroutine(Return(0));
        }
    }
    public void PowerButton(int n)
    {
        if(IsLocalPlayer)
        {
            if (n == 0)
            {
                SendCommand("USEPOWER", n.ToString());
                powerups0 -= 2;
            } else if(n == 1)
            {
                SendCommand("USEPOWER", n.ToString());
                powerups1 -= 2;
            }
            else if(n == 2)
            {
                SendCommand("USEPOWER", n.ToString());
                powerups2 -= 2;
            }
        }
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}