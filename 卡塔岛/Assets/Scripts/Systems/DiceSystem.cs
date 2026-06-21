using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceResult
{
    public int diceA;
    public int diceB;
    public int total;

    public DiceResult(int diceA, int diceB)
    {
        this.diceA = diceA;
        this.diceB = diceB;
        total = diceA + diceB;
    }
}

public class DiceSystem : MonoBehaviour
{
    public DiceResult RollTwoDice()
    {
        int diceA = Random.Range(1, 7);
        int diceB = Random.Range(1, 7);

        return new DiceResult(diceA, diceB);
    }
}
