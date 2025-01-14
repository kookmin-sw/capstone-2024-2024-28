﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APlayerController : ActorController
{
    [SerializeField] private GameObject effect_shadow;

    private int dashMax = 1; 
    public int DashMax{
        get{return dashMax;}
        set{dashMax = value;}
    }
    private int dashCount = 1;
    private bool shadow;

    private float velocityY = 0.0f;

    public Vector2 getAimDir()
    {
        Vector2 vec = Vector2.zero;

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            vec.x = -1.0f;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            vec.x = 1.0f;
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            vec.y = 1.0f;
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            vec.y = -1.0f;
        }
        return vec.normalized;
    }
    public void Dash()
    {
        if(dashCount > 0)
        {
            dashCount--;
        }
        else
        {
            return;
        }
        Locator.event_manager.notify(new OnDashEvent());
        float speed = 40;
        Vector2 vec = getAimDir();
        physics.velocity = vec.normalized * speed;
        shadow = true;
        Invoke("shadow_off", 0.2f);
        Invoke("stop", 0.2f);
        //SendMessage("on_jump", current_jump_cnt, SendMessageOptions.DontRequireReceiver);
    }
    private void DownCheck()
    {
        if (physics.velocity.y < 0.0f)
        {
            SendMessage("on_fall",true, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            SendMessage("on_fall",false, SendMessageOptions.DontRequireReceiver);
        }
    }
    public void shadow_off()
    {
        shadow = false;
    }

    void on_damaged(int damage)
    {
        
    }
    void on_ground()
    {
        dashCount = dashMax;
        Locator.event_manager.notify(new OnGroundEvent(velocityY));
    }
    public void Update()
    {
        DownCheck();
        velocityY = GetComponent<PhysicsPlatformer>().velocity.y;
        if (shadow)
        {
            GameObject G = Instantiate(effect_shadow, transform.position, Quaternion.identity);
            G.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
            G.transform.localScale = transform.localScale;
        }
    }
}