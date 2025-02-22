﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isEnabled;

    public bool AllowMovement;
    private bool stop;
    public PlayerController pc;
    public float detectDistance;
    public float shootDistance;
    public float speed;
    public bool chase;
    public LayerMask detectLayer;

    private Stats m_stats;
    private Camera m_cam;

    private Vector3 lastSeenSpot;

    public Slider healthBar;
    private Color m_barStartColor;

    public bool isMelee;
    public bool isRanged;
    public float escapeDistance;

    private Animator m_anim;

    public Weapon curWeapon;

    private SpriteRenderer sr;

    public float meleeDistance = 5;

    public bool HasGun
    {
        get
        {
            return GetComponent<Gun>() != null;
        }
    }

    private void Start()
    {
        pc = FindObjectOfType<PlayerController>();
        m_stats = GetComponent<Stats>();
        m_cam = FindObjectOfType<Camera>();
        if(healthBar)
            m_barStartColor = healthBar.colors.normalColor;
        m_anim = GetComponentInChildren<Animator>();

        sr = GetComponent<SpriteRenderer>();
        if (!sr)
            sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Update()
    {
        if(isEnabled && !GameManager.Instance.isGameFinished)
        {
            DoLogic();
            DoIdle();
            UpdateHealthBar();

            // flip sprite
            if(sr)
            {
                if (transform.eulerAngles.y > 180)
                {
                    sr.flipX = true;
                }
                else
                {
                    sr.flipX = false;
                }
            }
        }    
    }

    public void FixedUpdate()
    {
        if (!isEnabled)
            return;

        if(!pc)
        {
            pc = FindObjectOfType<PlayerController>();
        }

        //not sure if needed
        if (isMelee)
        {
            if (chase && Vector3.Distance(transform.position, lastSeenSpot) >= meleeDistance && !stop)
            {
                Move((lastSeenSpot - transform.position).normalized);

                if (m_anim != null)
                    m_anim.SetFloat("Speed", Mathf.Abs((lastSeenSpot - transform.position).normalized.magnitude));

                if (pc != null)
                    transform.LookAt(pc.transform.position);
            }
            else if (chase && Vector3.Distance(transform.position, lastSeenSpot) <= meleeDistance)
            {
                //chase = false;
                if (m_anim != null)
                    m_anim.SetFloat("Speed", 0);
            }
            else
            {
                //do idle or move untill wall or something
                if (m_anim != null)
                    m_anim.SetFloat("Speed", 0);
            }


            if(pc && Vector3.Distance(transform.position, pc.transform.position) <= meleeDistance)
            {
                if (curWeapon != null)
                {
                    if (curWeapon.GetType() == typeof(Melee))
                    {
                        Melee sword = (Melee)curWeapon;
                        if (sword.swingTimer <= 0)
                        {
                            stop = true;
                            sword.Swing();

                            if (m_anim != null)
                            {
                                m_anim.SetTrigger("Attack");
                                SoundManager.PlayASource("EnemyRoar");
                            }
                        }
                        else
                            stop = false;
                    }
                }
            }


        }
        //fix ranged
        else if (isRanged)
        {
            //run to shoot distance
            if (chase && Vector3.Distance(transform.position, pc.transform.position) >= detectDistance + 5)
                chase = false;
            else if (chase && Vector3.Distance(transform.position, lastSeenSpot) >= shootDistance)
            {
                if (pc != null)
                    transform.LookAt(pc.transform.position);

                if (GetComponent<Gun>().canShoot)
                    Move((lastSeenSpot - transform.position).normalized);

                if (m_anim != null)
                    m_anim.SetFloat("Speed", 1);
            }
            //run away from player
            else if (chase && Vector3.Distance(transform.position, lastSeenSpot) <= escapeDistance)
            {
                if (pc != null)
                    transform.LookAt(pc.transform.position);

                if (GetComponent<Gun>().canShoot)
                    Move((transform.position - lastSeenSpot).normalized);

                if (m_anim != null)
                    m_anim.SetFloat("Speed", 1);//Mathf.Abs((lastSeenSpot - transform.position).normalized.magnitude));

                //maybe shoot while running?
                //Shoot();
            }
            else if (chase && Vector3.Distance(transform.position, pc.transform.position) >= escapeDistance)
            {
                if (pc != null)
                    transform.LookAt(pc.transform.position);

                if (m_anim != null)
                    m_anim.SetFloat("Speed", 0);

                Shoot();
                //chase = false;
            }
            else
            {
                //do idle or move untill wall or something
                if (m_anim != null)
                    m_anim.SetFloat("Speed", 0);
            }
        }

        if(transform.position.y < -20)
        {
            Destroy(gameObject);
        }
            
    }

    public void DoIdle()
    {
        if (chase)
            return;

        if (waiting)
            return;

        transform.Rotate(transform.rotation.x, Random.Range(0, 360), transform.rotation.z);

        StartCoroutine(AIDelay(Random.Range(0, 5)));
    }

    private bool waiting = false;

    IEnumerator AIDelay(float delay)
    {
        waiting = true;
        yield return new WaitForSeconds(delay);
        waiting = false;
    }

    public void DoLogic()
    {
        if(pc != null)
            CheckForPlayer();
    }

    public void Shoot()
    {
        if(isRanged && HasGun)
        {
            if (GetComponent<Gun>().canShoot)
            {
                GetComponent<Gun>().Shoot(transform);

                if (GetComponentInChildren<Animator>())
                    GetComponentInChildren<Animator>().SetTrigger("Shoot");

                StartCoroutine(WaitAfterShot());

                SoundManager.PlayASource("Fire");
            }      
        }
    }

    private IEnumerator WaitAfterShot()
    {
        yield return new WaitForSeconds(.5f);
        /*if(!GetComponent<Stats>().isDead)
            GetComponentInChildren<Animator>().SetTrigger("Reload");*/
    }

    public void CheckForPlayer()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, (pc.transform.position - transform.position).normalized, out hit, 200, detectLayer);

        Debug.DrawLine(transform.position, hit.point, Color.red);

        Vector3 targetDir = pc.transform.position - transform.position;
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));

        if (hit.collider != null && hit.collider.gameObject != null 
            && hit.collider.gameObject == pc.gameObject && Vector3.Distance(transform.position, pc.transform.position) <= detectDistance
            && angleToPlayer >= -90 && angleToPlayer <= 90)
        {
            chase = true;
            lastSeenSpot = pc.transform.position;

            //play detect sound
            //add delay so it doesnt get spammed
            if(canYell)
            {
                canYell = false;
                StartCoroutine(WaitForAnnouncement());

                if(isRanged)
                {
                    SoundManager.PlayASource("EnemyShoot");
                }
                else if(isMelee)
                {
                    SoundManager.PlayASource("EnemyRoar");
                }
            }
        }
    }

    private bool canYell = true;

    private IEnumerator WaitForAnnouncement()
    {
        yield return new WaitForSeconds(Random.Range(4,7));
        canYell = true;
    }

    protected void Move(Vector3 moveVector)
    {
        if (AllowMovement)
        {
            transform.Translate(moveVector * Time.fixedDeltaTime * speed, Space.World);
        }
    }

    private void UpdateHealthBar()
    {
        if (!healthBar)
            return;


        if(pc && Vector3.Distance(transform.position, pc.transform.position) >= 12)
        {
            if (healthBar.gameObject.activeSelf)
                healthBar.gameObject.SetActive(false);
        }
        else
        {
            if(!healthBar.gameObject.activeSelf)
                healthBar.gameObject.SetActive(true);
        }

        healthBar.value = m_stats.health / m_stats.maxHealth;
        healthBar.transform.LookAt(healthBar.transform.position + m_cam.transform.rotation * Vector3.back,
                                       m_cam.transform.rotation * Vector3.down);
        /*float dist = Vector3.Distance(Camera.main.transform.position, healthBar.transform.position) * 0.025f;
        healthBar.transform.localScale = Vector3.one * dist;*/
    }

    public void FlashHealthBar()
    {
        if (!healthBar)
            return;

        Image im = healthBar.transform.GetChild(1).GetChild(0).GetComponent<Image>();
        StartCoroutine(FlashHealthBar(im, 0.1f));
    }

    private IEnumerator FlashHealthBar(Image im, float dur)
    {
        if (!healthBar.transform.parent.gameObject.activeSelf)
            healthBar.transform.parent.gameObject.SetActive(true);
        im.color = Color.white;
        yield return new WaitForSeconds(dur);
        im.color = m_barStartColor;
    }
}
