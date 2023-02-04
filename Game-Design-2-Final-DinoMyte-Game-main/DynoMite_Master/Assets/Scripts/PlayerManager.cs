using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using NETWORK_ENGINE;
using System.Text;

public class PlayerManager : MonoBehaviour
{
    Process Server2;
    Process Server3;
    Process Server4;
    Process Server5;
    bool isServer = false;

    public string PNAME;
    public int HAT = 0; //Keeps track of which hat the player is wearing
    public int SKIN = 0; //Keeps track of player skin
    public int DINOCLASS = 0;
    public int HP = 10;
    //0 = Invalid
    //1 = T-Rex
    //2 = Raptor
    //3 = Diplodocus
    //4 = Ankylo
    //5 = Stego
    
    public int LastScene = 0;
    public int CurrentScene = 0;
    public GameObject loginCanvas; //disable this when logging in
    public Button startButton; //disable this until a name is entered and a class is picked
    public GameObject helpScreen;
    public GameObject loadScreen;
    public string PublicIP = "71.44.212.42";
    public string PrivateIP = "10.200.208.182";
    public bool usePublic = false;
    public bool usePrivate = false;
    public bool useLocal = false;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += SceneChanger;
        //Check to see if you are the server1.
        string[] args = System.Environment.GetCommandLineArgs();
        //args[0] should be this program.

        //UnityEngine.Debug.Log(System.Environment.CurrentDirectory + "\\" + args[0]);

        // This is to check which sever is which
        int goalScene = 0;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "SERVER1") //This is the Diner server.
            {
                //Spawn server2. (Grassland server)
                goalScene = 1;
                isServer = true;
                Server2 = new Process();
                Server2.StartInfo.FileName = args[0];
                Server2.StartInfo.Arguments = "SERVER2";
                Server2.StartInfo.CreateNoWindow = true; //Set to false when testing
                Server2.Start();

                //Spawn server3. (Cave server)
                goalScene = 1;
                Server3 = new Process();
                Server3.StartInfo.FileName = args[0];
                Server3.StartInfo.Arguments = "SERVER3";
                Server3.StartInfo.CreateNoWindow = true; //Set to false when testing
                Server3.Start();

                //Spawn server4. (Graveyard server)
                goalScene = 1;
                Server4 = new Process();
                Server4.StartInfo.FileName = args[0];
                Server4.StartInfo.Arguments = "SERVER4";
                Server4.StartInfo.CreateNoWindow = true; //Set to false when testing
                Server4.Start();

                //Spawn server5. (Boss server)
                goalScene = 1;
                Server5 = new Process();
                Server5.StartInfo.FileName = args[0];
                Server5.StartInfo.Arguments = "SERVER5";
                Server5.StartInfo.CreateNoWindow = true; //Set to false when testing
                Server5.Start();
            }
            if (args[i] == "SERVER2")
            {
                isServer = true;
                goalScene = 2;
            }
            if (args[i] == "SERVER3")
            {
                isServer = true;
                goalScene = 3;
            }
            if (args[i] == "SERVER4")
            {
                isServer = true;
                goalScene = 4;
            }
            if (args[i] == "SERVER5")
            {
                isServer = true;
                goalScene = 5;
            }
        }
        if (goalScene != 0)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            SceneManager.LoadScene(goalScene);
        }
        //If so spawn server2 and server3.
        //switchScenes
        //Start server

        //If not ser ver, show player UI + get information + connect to server1 for starting the game.
    }
    public void SetName(string p) // sets the player name
    {
        PNAME = p;
    }
    public void SetDinoClass(int i) // sets what type of dino
    {
        DINOCLASS = i;
    }
    public void SetHat(int h) // sets the hat
    {
        HAT = h;
    }
    public void SetSkin(int s) // sets this players skin
    {
        SKIN = s;
    }

    public void SetHP(int p)
    {
        HP = p;
    }
    public void StartClient() // starts at scene one and turns off the login canvas
    {
        SceneManager.LoadScene(1);
        loginCanvas.SetActive(false);
    }

    public void HelpButton()
    {
        helpScreen.SetActive(true);
    }
    public void BackButton()
    {
        helpScreen.SetActive(false);
    }
    public void SceneChanger(Scene s, LoadSceneMode m)//
    {
        if(!isServer) //Activates loading screens
            loadScreen.SetActive(true);
        CurrentScene = s.buildIndex;
        StartCoroutine(SlowStart());
    }
    public IEnumerator FindIP()
    {
        //bool UsePublic = false;
        //bool UseFlorida = false;

        //Ping Public Ip address to see if we are external..........
        GenericNetworkCore.Logger("Trying Public IP Address: " + PublicIP.ToString());
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        System.Net.NetworkInformation.PingOptions po = new System.Net.NetworkInformation.PingOptions();
        po.DontFragment = true;

        string data = "HELLLLOOOOO!";
        byte[] buffer = ASCIIEncoding.ASCII.GetBytes(data);
        int timeout = 500;

        System.Net.NetworkInformation.PingReply pr = ping.Send(PublicIP, timeout, buffer, po);
        yield return new WaitForSeconds(1.5f);

        if (pr.Status == System.Net.NetworkInformation.IPStatus.Success)
        {
            GenericNetworkCore.Logger("The public IP responded with a roundtrip time of: " + pr.RoundtripTime);
            usePublic = true;
            
        }
        else
        {
            GenericNetworkCore.Logger("The public IP failed to respond");
            usePublic = false;
        }

        //-------------------If not public, ping Florida Poly for internal access.

        if (!usePublic)
        {
            GenericNetworkCore.Logger("Trying Florida Poly Address: " + PrivateIP.ToString());
            pr = ping.Send(PrivateIP, timeout, buffer, po);

            yield return new WaitForSeconds(1.5f);

            if (pr.Status.ToString() == "Success")
            {
                GenericNetworkCore.Logger("The Florida Poly IP responded with a roundtrip time of: " + pr.RoundtripTime);
                usePrivate = true;
            }
            else
            {
                GenericNetworkCore.Logger("The Florida Poly IP failed to respond");
                usePrivate = false;
            }
        }

        //Otherwise use local host, assume testing.

        if (!usePublic && !usePrivate)
        {
            useLocal = true;
            GenericNetworkCore.Logger("Using Home Address!");
        }
        //StartCoroutine(ClientStart()); ;
    }
    IEnumerator SlowStart() // used to initilaize the client and server
    {
        yield return new WaitForSeconds(.5f);
        if (isServer)
        {
            //Find out which server this is and associate the correct port with it.
            string[] args = System.Environment.GetCommandLineArgs();
            NetworkCore myCore = GameObject.FindObjectOfType<NetworkCore>();
            foreach (string i in args)
            {
                if(i == "SERVER1")
                {
                    myCore.PortNumber = 9001;
                    break;
                }
                if (i == "SERVER2")
                {
                    myCore.PortNumber = 9002;
                    break;
                }
                if (i == "SERVER3")
                {
                    myCore.PortNumber = 9003;
                    break;
                }
                if (i == "SERVER4")
                {
                    myCore.PortNumber = 9004;
                    break;
                }
                if (i == "SERVER5")
                {
                    myCore.PortNumber = 9005;
                    break;
                }

            }
            myCore.UI_StartServer();
        }
        else
        {
            if(!usePublic && !useLocal && !usePrivate)
            {
                yield return StartCoroutine(FindIP());
            }
            if(usePublic)
            {
                GameObject.FindObjectOfType<NetworkCore>().IP = PublicIP;
            }
            else if (usePrivate)
            {
                GameObject.FindObjectOfType<NetworkCore>().IP = PrivateIP;
                UnityEngine.Debug.Log("Private IP being used");
            }
            else if (useLocal)
            {
                GameObject.FindObjectOfType<NetworkCore>().IP = "127.0.0.1";
            }
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 1:
                    GameObject.FindObjectOfType<NetworkCore>().PortNumber = 9001;
                    UnityEngine.Debug.Log("Port No.: " + GameObject.FindObjectOfType<NetworkCore>().PortNumber);
                    break;
                case 2:
                    GameObject.FindObjectOfType<NetworkCore>().PortNumber = 9002;
                    break;
                case 3:
                    GameObject.FindObjectOfType<NetworkCore>().PortNumber = 9003;
                    break;
                case 4:
                    GameObject.FindObjectOfType<NetworkCore>().PortNumber = 9004;
                    break;
                case 5:
                    GameObject.FindObjectOfType<NetworkCore>().PortNumber = 9005;
                    break;
            }
            //GameObject.FindObjectOfType<NetworkCore>().IP = "127.0.0.1";
            UnityEngine.Debug.Log("IP Address: " + GameObject.FindObjectOfType<NetworkCore>().IP);
            GameObject.FindObjectOfType<NetworkCore>().UI_StartClient();
        }
    }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        if (PNAME.Contains("#") || PNAME.Contains("<") || PNAME.Contains(">") || PNAME.Contains("/") || PNAME.Contains("\\") || PNAME == "" || DINOCLASS == 0) //check to make sure the player isn't trying to break the game with their name
            startButton.interactable = false;
        else
            startButton.interactable = true; // allows you to start after putting in valid name

        if (GameObject.FindObjectOfType<GameCharacter>() != null)
            loadScreen.SetActive(false);
    }

    public void NetworkedStart()
    {
        //throw new System.NotImplementedException();
    }
}