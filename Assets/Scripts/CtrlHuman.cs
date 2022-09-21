using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);

                // Move
                var sendMsg = $"Move|{NetManager.GetDesc()},{hit.point.x},{hit.point.y},{hit.point.z}";
                NetManager.Send(sendMsg);
            }
        }
    }
}