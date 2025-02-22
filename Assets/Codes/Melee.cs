﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Weapon
{
    public float swingTimer;
    public float swingTimerMax;
    public bool canMelee;

    public float swingDuration;
    public float maxSwingDuration;
    public bool canDealDamage;
    public float meleeForce;

    // public SaveFile saveFile;
    // public GameManager gameManager;

    //public bool isEnemy;

    private void Update()
    {
        if(swingTimer > 0)
            swingTimer -= Time.deltaTime;
        if(swingTimer < 0)
        {
            swingTimer = 0;
            canMelee = true;
            this.gameObject.tag = "Untagged";
            hitList.Clear();
        }

        if (swingDuration > 0)
        {
            canDealDamage = true;
            swingDuration -= Time.deltaTime;
        }
            
        if(swingDuration < 0)
        {
            swingDuration = 0;
            canDealDamage = false;
        }
    }

    public void Swing()
    {
        this.gameObject.tag = "Destroy";
        canMelee = false;
        swingTimer = swingTimerMax;
        swingDuration = maxSwingDuration;
    }

    private List<GameObject> hitList = new List<GameObject>();

    public void ClearHitList()
    {
        this.gameObject.tag = "Untagged";
        canDealDamage = false;
        hitList.Clear();
    }

    // private List<>

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root != owner && other.gameObject.layer != 0)// && other.gameObject.layer != gameObject.layer)
        {
            Stats s = other.transform.root.GetComponent<Stats>();


            if(other.GetComponent<Fracture>() && owner.GetComponent<PlayerController>())
            {
                if (canDealDamage && !hitList.Contains(other.gameObject))
                {
                    other.GetComponentInParent<Building>().TakeDamage(20f);
                    hitList.Add(other.gameObject);
                }
            }

            if (s != null && canDealDamage && !hitList.Contains(s.GetComponent<GameObject>()))
            {
                s.TakeDmg(damage);
                hitList.Add(s.GetComponent<GameObject>());
            }

            Rigidbody r = other.GetComponent<Rigidbody>();
            if (r != null && (r.gameObject.layer == 8 || r.gameObject.layer == 7) && canDealDamage && !hitList.Contains(r.gameObject))
            {
                r.isKinematic = false;
                r.AddForceAtPosition(this.transform.forward * meleeForce, r.position - other.transform.position);
            }

                /*Rigidbody r = other.GetComponent<Rigidbody>();

                if (r != null && (r.gameObject.layer == 8 || r.gameObject.layer == 7))
                {
                    if (canDealDamage && !hitList.Contains(r.gameObject))
                    {
                        //TODO: Somekind of condtion for breaking

                        r.isKinematic = false;

                        hitList.Add(r.gameObject);
                        r.AddForceAtPosition(this.transform.forward * meleeForce, r.position - other.transform.position);
                    }
                }*/
            }
    }
}
