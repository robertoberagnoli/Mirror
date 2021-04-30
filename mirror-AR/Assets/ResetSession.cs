using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class ResetSession : MonoBehaviour
{
  
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Reset_Session", 20);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Reset_Session()
    {
        ARCoreSession session = GameObject.Find("ARCore Device").GetComponent<ARCoreSession>();
        ARCoreSessionConfig myConfig = session.SessionConfig;

        DestroyImmediate(session);
        // Destroy(session);

        yield return null;

        session = GameObject.Find("ARCore Device").GetComponent<ARCoreSession>(); 

        session.SessionConfig = myConfig;
        session.enabled = true;

        Invoke("Reset_Session",10);
    }
}
