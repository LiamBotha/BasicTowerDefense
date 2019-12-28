using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerModel
{
    public int numUpgrades = 0;

    private int[] enemySumTime = new int[7]; // melee, ranged, commander, scout, techy, vanguard, wizard 
    private int[] numEnemyKilled = new int[7];
    public int[] numTurretsBuilt = new int[3]; // standard, multi, grenade

    private Dictionary<int, int> enemyTimeSum = new Dictionary<int, int>();

    public int hardestEnemy;
    public int easiestEnemy;

    private int difficulty = 1; // Max must be 4, Min must be 1

    public int GetDifficulty()
    {
        return difficulty;
    }

    public void SetDifficulty(int value)
    {
        if(value > 4)
        {
            value = 4;
        }
        else if(value < 1)
        {
            value = 1;
        }

        difficulty = value;
    }

    // TODO - Turrets build
    // TODO - Easiest enemy
    // TODO - Hardest enemy
    // TODO - Other statistics

    public PlayerModel()
    {
        enemyTimeSum.Add(0, 0);
        enemyTimeSum.Add(1, 0);
        enemyTimeSum.Add(2, 0);
        enemyTimeSum.Add(3, 0);
        enemyTimeSum.Add(4, 0);
        enemyTimeSum.Add(5, 0);
        enemyTimeSum.Add(6, 0);
    }

    public int GetRange(int numTypes = 4)
    {
        int min = 0 + (GetDifficulty() - 1);
        int max = (GetDifficulty() - 1) + numTypes;

        Debug.Log("Minimum: " + min + ", Maximum: " + max);

        int val = Random.Range(min , max);

        return enemyTimeSum.ElementAt(val).Key;
    }

    public void AddTime(BaseUnit enemy) //TODO replace with values from event listener
    {
        int enemyVal = 0;

        if(enemy.GetType() == typeof(MeleeEnemy))
        {
            enemyVal = 0;
        }
        else if(enemy.GetType() == typeof(RangedEnemy))
        {
            enemyVal = 1;
        }
        else if (enemy.GetType() == typeof(Commander))
        {
            enemyVal = 2;
        }
        else if (enemy.GetType() == typeof(Scout))
        {
            enemyVal = 3;
        }
        else if (enemy.GetType() == typeof(Techy))
        {
            enemyVal = 4;
        }
        else if (enemy.GetType() == typeof(Vanguard))
        {
            enemyVal = 5;
        }
        else if (enemy.GetType() == typeof(Wizard))
        {
            enemyVal = 6;
        }

        enemySumTime[enemyVal] += (int)enemy.timePassed; // array -> remove
        enemyTimeSum[enemyVal] += (int)enemy.timePassed; // dictionary
        numEnemyKilled[enemyVal]++;  
    }

    public float GetAvgTime(int enemyVal)
    {
        return enemyTimeSum[enemyVal] / numEnemyKilled[enemyVal];
    }

    private void SetBestAndWorstEnemy()
    {
        float minTime = float.MaxValue;
        float maxTime = float.MinValue;
        int minEnemy = 0;
        int maxEnemy = 0;

        for(int i = 0; i < enemySumTime.Length;++i)
        {
            float val = GetAvgTime(i);

            if(minTime > val)
            {
                minTime = val;
                minEnemy = i;
            }
            else if(maxTime < val)
            {
                maxTime = val;
                maxEnemy = i;
            }
        }

        easiestEnemy = minEnemy;
        hardestEnemy = maxEnemy;
    } // gets avg time and uses it to set easy and hard enemy

    private void SortTime()
    {
        var items = from pair in enemyTimeSum
                    orderby pair.Value ascending
                    select pair;

        enemyTimeSum = (Dictionary<int,int>)items;
    }
}
