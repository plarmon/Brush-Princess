using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrindPoint : MonoBehaviour
{
    [SerializeField] private bool effect;
    [SerializeField] public GameObject grindEffect;

    private string name = "";
    private Transform point;
    private GrindPoint prevPoint;
    private GrindPoint nextPoint;
    private float randomBalanceModif;

    private void Start()
    {
        randomBalanceModif = Random.value * 2f - 1f;
        SetPoint(transform);
        SetName(gameObject.name);
    }

    public void SetPoint(Transform t) { point = t; }
    public void SetPrevPoint(GrindPoint gp) { prevPoint = gp; }
    public void SetNextPoint(GrindPoint gp) { nextPoint = gp; }
    public void SetName(string n) { name = n; }

    public Transform GetPoint() { return point; }
    public GrindPoint GetPrevPoint() { return prevPoint; }
    public GrindPoint GetNextPoint() { return nextPoint; }
    public string GetName() { return name; }
    public bool GetEffect() { return effect; }
    public float GetBalanceModif() { return randomBalanceModif; }

    public void PlayEffect()
    {
        grindEffect.GetComponent<ParticleSystem>().Play();
    }

    public void PauseEffect()
    {
        grindEffect.GetComponent<ParticleSystem>().Stop();
    }
}
