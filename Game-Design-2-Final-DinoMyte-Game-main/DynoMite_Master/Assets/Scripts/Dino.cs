using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

// Class that was used to template out the dinoasurs, but does not get used.  

[RequireComponent(typeof(Rigidbody2D))]
public class Dino : Entity
{
    //Private variables [DO NOT USE]
    private int _attackValue;
    private int _defenseValue;
    private int _speedBase;
    private float _speed;

    private int _abilityOneBaseCD;
    private float _abilityOneCD;
    private int _abilityTwoBaseCD;
    private float _abilityTwoCD;

    private KeyBinds playerKeyBinds;
    private Inventory playerInventory;
    //private DinoModel _playerModel; //CUSTOMIZABLE DINOS

    //Public variables [USE THESE ONES]
    public int AttackValue;
    public int DefenseValue;
    public int SpeedBase;
    public float Speed;

    public int AbilityOneCD;
    public int AbilityTwoCD;

    public KeyBinds PlayerKeyBinds;
    public Inventory PlayerInventory;
    //public DinoModel PlayerModel; //CUSTOMIZABLE DINOS

    //Public Components [ASSIGN IN INSPECTOR]
    public Rigidbody2D dinoRB;

    public override IEnumerator SlowUpdate()
    {
        yield return null;
    }

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag,value);

    }
    /*
    public override void NetworkedStart()
    {
        base.NetworkedStart();

    }
    */
    void Start()
    {
      
    }

    void Update()
    {
        
    }

    public override void Death()
    {
        base.Death();

    }
    /*
    public override void Damage(int incomingDamage)
    {
        base.Damage(incomingDamage);

    }
    */
    protected virtual void Attack()
    {
        //EMPTY - TO BE FILLED IN PlayerClass CLASS
    }

    protected virtual void UseAbilityOne()
    {
        //EMPTY - TO BE FILLED IN PlayerClass CLASS
    }

    protected virtual void UseAbilityTwo()
    {
        //EMPTY - TO BE FILLED IN PlayerClass CLASS
    }

    protected virtual IEnumerator AbilityOneCooldownTimer()
    {
        yield return null; //EMPTY - TO BE FILLED IN PlayerClass CLASS
    }

    protected virtual IEnumerator AbilityTwoCooldownTimer()
    {
        yield return null; //EMPTY - TO BE FILLED IN PlayerClass CLASS
    }
}
