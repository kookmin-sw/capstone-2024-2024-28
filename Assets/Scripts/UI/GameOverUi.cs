﻿using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
public class GameOverUi : MonoBehaviour
{
    public UnityEngine.UI.Image image1;
    public UnityEngine.UI.Image image2;
    public TextMeshProUGUI textMeshPro;
    public SoundFade bgm;

    void Awake()
    {
        Locator.event_manager.register<GameOverEvent>(on_game_over);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameOverStart();
        }
    }
    public void GameOverStart()
    {
        StartCoroutine(ChangeAlphaOverTime());
    }

    private void on_game_over(IEventParam param)
    {
        Locator.player.GetComponent<PhysicsPlatformer>().enabled = false;
        Locator.player.GetComponent<PlayerAnimator>().enabled = false;
        Locator.player.GetComponent<PlayerController>().enabled = false;
        Locator.player.GetComponent<APlayerController>().enabled = false;
        Locator.player.GetComponent<PlayerAttack>().enabled = false;
        var player_animator = Locator.player.GetComponent<Animator>();
        player_animator.Rebind();
        // foreach (var parameter in player_animator.parameters)
        // {
        //     if (parameter.type
        // }
        Locator.player.GetComponent<Animator>().SetBool("isDeath", true);
        StartCoroutine(ChangeAlphaOverTime());
    }

    IEnumerator ChangeAlphaOverTime()
    {
        bgm.FadeOut();
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            Color image1Color = image1.color;
            image1Color.a = Mathf.Lerp(0f, 0.6f, elapsedTime / 2f);
            image1.color = image1Color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        
        elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            Color image2Color = image2.color;
            image2Color.a = Mathf.Lerp(0f, 0.8f, elapsedTime / 2f);
            image2.color = image2Color;

            Color textMeshProColor = textMeshPro.color;
            textMeshProColor.a = Mathf.Lerp(0f, 1f, elapsedTime / 2f);
            textMeshPro.color = textMeshProColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(FadeManager.Instance.LoadDiffScene("Main"));
    }
}