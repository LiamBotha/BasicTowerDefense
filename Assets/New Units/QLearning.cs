using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning
{
    const float EPSILON = 0.000001f;

    //States
    double[] states; //may remove

    //Actions
    Dictionary<string, int> actions = new Dictionary<string, int>()
    {
        {"Continue",0},
        {"Teleport",1}
    };

    //Current State
    float currentState;
    int action;

    //QMatrix
    float[,] qMatrix = new float[5000,3];
    float learningRate = 0.5f;
    float discountFactor = 0.5f;

    //Rewards - Time, TurrDamage, BaseDamage

    

    //Train()

    public void Train() // Basic aad some more to it later for more complex uses
    {
        currentState = 0.1f;

        var maxQ = float.MinValue;

        foreach(var action in actions)
        {
            var Q = GetQuality(currentState,action.Value);

            if (Q > maxQ)
            {
                maxQ = Q;
            }
        }

        var oldQ = GetQuality(currentState,action);

        var stateReward = 0;

        var newQ = ((1 - learningRate) * oldQ) + (learningRate * (stateReward + (discountFactor * maxQ)));

        SetQuality(action,currentState, newQ);
    }

    public void UpdateValues(bool hasTeleported, float time, int damage)
    {
        int reward = (int)(damage - time); // Add points for heath later
        int actionIndex = 0;

        var maxQ = float.MinValue;

        foreach (var action in actions)
        {
            var Q = GetQuality(time,action.Value);

            if (Q > maxQ)
            {
                maxQ = Q;
            }
        }

        var oldQ = float.MinValue;

        var stateReward = reward;

        var newQ = ((1 - learningRate) * oldQ) + (learningRate * (stateReward + (discountFactor * maxQ)));

        if (hasTeleported)
        {
            actionIndex = 0;
            oldQ = GetQuality(time, 0);
            
        }
        else
        {
            actionIndex = 1;
            oldQ = GetQuality(time, 1);
        }

        SetQuality(actionIndex, time, newQ);
    }

    //SetQuality
    public void SetQuality(int action,float time, float value)
    {
        int stateIndex = (int)(currentState * 10);
        int actionIndex = action;

        qMatrix[stateIndex, actionIndex] = value;
    }

    //Get Quality
    public float GetQuality(float time,int action)
    {
        int stateIndex = (int)(time * 10);
        int actionIndex = action;

        return qMatrix[stateIndex, actionIndex];
    }

    public int GetNextAction(float time)
    {
        int stateIndex = (int)currentState * 10;
        int bestAction = 0;
        var maxQ = float.MinValue;

        foreach(var action in actions)// Add randomization
        {
            var bestQ = GetQuality(time, action.Value);

            if(bestQ > maxQ)
            {
                maxQ = bestQ;
                bestAction = action.Value;
            }
        }

        return bestAction;
    }

}
