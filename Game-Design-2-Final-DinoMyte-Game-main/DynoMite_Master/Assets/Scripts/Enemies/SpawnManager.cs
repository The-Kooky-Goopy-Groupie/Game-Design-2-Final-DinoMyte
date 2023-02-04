using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : NetworkComponent
{
    public EnemySpawner parent;
    public override void HandleMessage(string flag, string value)
    {
        //throw new System.NotImplementedException();
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(1f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        if(IsServer)
            parent.entities--; // I AINT GOT SHIZZZZZ for this what does IT MEANNNNNNNNNNNN
    }

    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}
