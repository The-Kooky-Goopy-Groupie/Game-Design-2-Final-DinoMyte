using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;


// Scrapped Script - was attempt to fix player music client glitch


public class ServerMute : NetworkComponent
{
    public AudioSource music;
    public override void HandleMessage(string flag, string value)
    {
        //throw new System.NotImplementedException();
    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer)
        {
            music.volume = 0f; // Sets audio sources that are music to have no volume if they are servers 
        }
        yield return new WaitForSeconds(.1f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}
