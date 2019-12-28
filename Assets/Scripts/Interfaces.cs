using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    void TakeDamage(int val);

    void GainHealth(int val);
}

public enum TurretType
{
    Normal, Multi_Attack, Grenadier
}

public enum Lane
{
    Left,Right,Top,Bottom
}
