using System;
using UnityEngine;

[Serializable]
public class ShipConfig
{
    public string shipId;
    public string name;
    public string spriteResourcePath;
    public Sprite shipSprite;
    public float moveSpeed = 9.5f;
    public float fireRate = 0.22f;
    public int startingBombs = 2;
    public bool startWithShield;
    public string description;
    public Color accentColor = Color.cyan;
}