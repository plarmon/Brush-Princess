using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trick
{
    private string trickName;
    private float time;
    private float points;
    private int animHash;

    #region Constructors
    public Trick()
    {
        new Trick("empty", 0, 0);
    }

    public Trick(string name, float time, float points)
    {
        SetName(name);
        SetTime(time);
        SetPoints(points);
    }

    public Trick(string name, float time, float points, int animHash)
    {
        SetName(name);
        SetTime(time);
        SetPoints(points);
        SetAnimHash(animHash);
    }

    public Trick(string name, float time, float points, string animHash)
    {
        SetName(name);
        SetTime(time);
        SetPoints(points);
        SetAnimHash(animHash);
    }
    #endregion

    #region Getters/Setters
    public void SetName(string name)
    {
        trickName = name;
    }

    public string GetName()
    {
        return trickName;
    }

    public void SetTime(float time)
    {
        this.time = time;
    }

    public float GetTime()
    {
        return time;
    }

    public void SetPoints(float points)
    {
        this.points = points;
    }

    public float GetPoints()
    {
        return points;
    }

    public void SetAnimHash(int animHash)
    {
        this.animHash = animHash;
    }

    public void SetAnimHash(string animString)
    {
        animHash = Animator.StringToHash(animString);
    }

    public int GetAnimHash()
    {
        return animHash;
    }
    #endregion

}
