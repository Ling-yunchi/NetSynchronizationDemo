using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class Main : MonoBehaviour
{
    public GameObject humanPrefeb;
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();
    public GameObject virtualCamera;
    private CinemachineVirtualCamera _virtualCamera;

    void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.AddListener("List", OnList);
        NetManager.Connect("127.0.0.1", 9000);

        GameObject obj = Instantiate(humanPrefeb);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();
        
        _virtualCamera = virtualCamera.GetComponent<CinemachineVirtualCamera>();
        _virtualCamera.Follow = myHuman.transform;

        // Enter
        var pos = myHuman.transform.position;
        Vector3 euler = myHuman.transform.eulerAngles;
        string sendStr = $"Enter|{myHuman.desc},{pos.x},{pos.y},{pos.z},{euler.y}";
        NetManager.Send(sendStr);

        // List
        NetManager.Send("List|");
    }

    private void OnList(string msg)
    {
        Debug.Log($"OnList {msg}");
        string[] split = msg.Split(',');
        int count = (split.Length - 1) / 6;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i * 6 + 0];
            float x = float.Parse(split[i * 6 + 1]);
            float y = float.Parse(split[i * 6 + 2]);
            float z = float.Parse(split[i * 6 + 3]);
            float ry = float.Parse(split[i * 6 + 4]);
            int hp = int.Parse(split[i * 6 + 5]);
            if (desc == myHuman.desc)
            {
                myHuman.hp = hp;
                continue;
            }

            GameObject obj = Instantiate(humanPrefeb);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, ry, 0);
            BaseHuman human = obj.AddComponent<SyncHuman>();
            human.desc = desc;
            otherHumans.Add(desc, human);
        }
    }

    private void Update()
    {
        NetManager.Update();
    }

    private void OnDestroy()
    {
        NetManager.Disconnect();
    }

    private void OnLeave(string msg)
    {
        Debug.Log("OnLeave:" + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        if (otherHumans.ContainsKey(desc))
        {
            Destroy(otherHumans[desc].gameObject);
            otherHumans.Remove(desc);
        }
    }

    private void OnMove(string msg)
    {
        Debug.Log("OnMove:" + msg);
        var split = msg.Split(',');
        var desc = split[0];
        var x = float.Parse(split[1]);
        var y = float.Parse(split[2]);
        var z = float.Parse(split[3]);

        if (!otherHumans.ContainsKey(desc))
            return;

        BaseHuman human = otherHumans[desc];
        human.MoveTo(new Vector3(x, y, z));
    }

    private void OnEnter(string msg)
    {
        Debug.Log($"OnEnter {msg}");
        var arr = msg.Split(',');
        var desc = arr[0];
        var x = float.Parse(arr[1]);
        var y = float.Parse(arr[2]);
        var z = float.Parse(arr[3]);
        var ry = float.Parse(arr[4]);

        if (desc == NetManager.GetDesc())
            return;

        GameObject obj = Instantiate(humanPrefeb);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, ry, 0);
        BaseHuman human = obj.AddComponent<SyncHuman>();
        human.desc = desc;
        otherHumans.Add(desc, human);
    }
}