using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasHUD : MonoBehaviour
{
    private float _abilityOneCooldownBase = 5.0f;
    private float _abilityOneCooldownCurrent;
    private float _abilityTwoCooldownBase = 5.0f;
    private float _abilityTwoCooldownCurrent;

    public Image AbilityOneImage;
    public Image AbilityTwoImage;
    public Image AbilityOneDisabled;
    public Image AbilityTwoDisabled;

    public bool useAbilityOneTest = false;

    // Start is called before the first frame update
    void Start()
    {
        _abilityOneCooldownCurrent = _abilityOneCooldownBase;
        _abilityTwoCooldownCurrent = _abilityTwoCooldownBase;
        AbilityOneDisabled.fillAmount = 0.0f; //avoids visual bugs
        AbilityTwoDisabled.fillAmount = 0.0f; 
    }

    // Update is called once per frame
    void Update()
    {
        if (useAbilityOneTest)
        {
            AbilityOneStartTimer();
        }
    }

    public void AbilityOneStartTimer()
    {
        StartCoroutine(nameof(AbilityOneCooldown));
    }

    public void AbilityTwoStartTimer()
    {
        StartCoroutine(nameof(AbilityTwoCooldown));
    }

    public void AbilityOneEnable()
    {
        AbilityOneDisabled.fillAmount = 0.0f;
    }

    public void AbilityTwoEnable()
    {
        AbilityTwoDisabled.fillAmount = 0.0f;
    }
    public void AbilityOneDisable()
    {
        AbilityOneDisabled.fillAmount = 1.0f;
    }

    public void AbilityTwoDisable()
    {
        AbilityTwoDisabled.fillAmount = 1.0f;
    }
    private IEnumerator AbilityOneCooldown()
    {
        float cooldownTimer = _abilityOneCooldownCurrent; // So that timer displays only for the cooldown at the time of activation and doesn't jump around if cooldown changes. 
        float time = _abilityOneCooldownCurrent;
        while (time > 0.0f)
        {
            time -= Time.deltaTime;
            AbilityOneDisabled.fillAmount = time / cooldownTimer;
            yield return null;
        }
    }

    private IEnumerator AbilityTwoCooldown()
    {
        yield return null;
    }
}
