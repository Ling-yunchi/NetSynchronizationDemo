using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    protected bool _isMoving = false;
    private Vector3 _targetPosition;
    private float _speed = 1.0f;
    private Animator _animator;
    public string desc = "Base Human";
    public int hp = 100;

    public void MoveTo(Vector3 pos)
    {
        _targetPosition = pos;
        _isMoving = true;
        _animator.SetBool("isMoving", true);
    }

    public void MoveUpdate()
    {
        if (_isMoving == false)
            return;

        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos,_targetPosition,_speed * Time.deltaTime);
        transform.LookAt(_targetPosition);
        if(Vector3.Distance(pos,_targetPosition) < 0.1f)
        {
            _isMoving = false;
            _animator.SetBool("isMoving", false);
        }
    }

    protected void Start()
    {
        _animator = GetComponent<Animator>();
    }

    protected void Update()
    {
        MoveUpdate();
    }
}
