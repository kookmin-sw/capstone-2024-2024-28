﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpPower;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private BoxCollider2D boxCollider;
    private float regen_time = 0.0f;
    public bool is_destroyed { get; private set; } = false;

    public int defenceAttack;

    private void Start() 
    {
        Locator.player = this;
        
        defenceAttack = 0;

        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerAttack.Init(this);
        playerController.Init(this);

        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public override void destroy()
    {
        if (is_destroyed)
            return;

        is_destroyed = true;
        Locator.event_manager.notify(new GameOverEvent());
    }
    
    public void MaxHpUp(int amount)
    {
        max_hp += amount;
        hp += amount;
        Locator.event_manager.notify(new OnHpChangeEvent { hp = this.hp });
    }

    public override void heal(int heal)
    {
        if (is_destroyed)
            return;

        base.heal(heal);
        Locator.event_manager.notify(new OnHpChangeEvent{hp = this.hp});
        Locator.damageManager.ShowHeal(transform, heal, Team.Player);
    }

    public override bool take_damage(int damage)
    {
        if (is_destroyed)
            return false;

        if(defenceAttack > 0)
        {
            defenceAttack -= 1;
            Locator.event_manager.notify(new OnGuardDamageEvent{});
            return false;
        }
        base.take_damage(damage);
        Locator.event_manager.notify(new OnHpChangeEvent{hp = this.hp});
        return true;
    }

    void FixedUpdate()
    {
        if (is_destroyed)
            return;

        regen_time += Time.deltaTime;
        if (regen_time > 1) {
            regen_time -= 1.0f;
            base.heal(5);
            Locator.event_manager.notify(new OnHpChangeEvent{hp = this.hp});
        }
    }

    public Rigidbody2D GetRigidbody()
    {
        return rigid;
    }
    public BoxCollider2D GetBoxCollider()
    {
        return boxCollider;
    }
    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
    public Animator GetAnimator()
    {
        return animator;
    }
    public void EventRegister<T>(Action<IEventParam> action)
    {
        Locator.event_manager.register<T>(action);
    }
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public float GetJumpPower()
    {
        return jumpPower;
    }
}
