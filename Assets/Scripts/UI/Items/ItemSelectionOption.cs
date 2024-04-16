﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;


public class ItemSelectionOption : MonoBehaviour
{
    public IItemSelector item_selector;
    public Image image;
    public Text level, item_name, description;
    private ItemInfo info;

    public void init(IItemSelector item_selector, ItemInfo info, int target_level)
    {
        this.item_selector = item_selector;
        image.sprite = info.sprite;
        level.text = target_level + ".Lv";
        item_name.text = info.name;
        description.text = info.description;
        this.info = info;
    }

    public void on_click()
    {
        item_selector.on_selected(info);
    }
}