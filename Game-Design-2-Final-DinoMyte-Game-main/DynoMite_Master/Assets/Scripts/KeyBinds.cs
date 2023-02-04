using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************
 * HOLDER FOR KEYBINDS
 * VALUES SET HERE ARE DEFAULTS
 * OPTIONS CAN BE ADDED TO ALLOW CUSTOMIZATIONS (Later)
 *  I don't really need to add anything here but note our jump is set to space instead
 ******************************/
public class KeyBinds
{
    KeyCode left = KeyCode.A;
    KeyCode right = KeyCode.D;
    KeyCode up = KeyCode.W; //Jump
    KeyCode down = KeyCode.S;
    KeyCode interact = KeyCode.F;
    KeyCode attack = KeyCode.Space;
    KeyCode abilityOne = KeyCode.Q;
    KeyCode abilityTwo = KeyCode.E;
}
