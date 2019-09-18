﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  public static bool IsDed;
  private static Vector2 checkPoint;
  public static Vector3 position => SM.player.transform.position;

  [Header("Move params")]
	[Range(0.5f, 10f)]
	public float speed = 1;
	[Range(0.5f, 5f)]
	public float jumpSpeed = 1;

  [Header("Weapons")]
  public bool isMeele;
  public WeaponTrigger meeleTrigger;
  public float meeleCooldown = 1;
  public bool isRange;

  private int _currentWeapon;
  internal int currentWeapon
  {
    set
    {
      if (_currentWeapon == value)
        return;

      _currentWeapon = value;
      animator.SetInteger("Weapon", _currentWeapon);
      animator.SetTrigger("SwitchWeapon");

    }
  }

  [Header("Components")]
  public Animator animator;
	public ContactFilter2D groundFilter;

	private Rigidbody2D body;

  public static bool IsPlayer(Collider2D collider)
	{
		return collider.GetComponent<Player>() != null;
	}

	private void Awake()
	{
		SM.player = this;
		body = GetComponent<Rigidbody2D>();
		if (!body)
		{
			Debug.LogWarning("No rigidbody for player");
		}
    checkPoint = transform.position;

  }

	private void Start()
	{
    //Time.timeScale = 0.1f;\
    meeleTrigger.coolDown = meeleCooldown;
  }

	private void OnEnable()
	{
		body.isKinematic = false;
	}

	private void OnDisable()
	{
		body.isKinematic = true;
	}

	private void Update()
	{
    if (IsDed)
    {
      if (SM.keyJump)
      {
        Respawn();
      }
      return;
    }

    if (coolDownHp > 0)
      coolDownHp -= Time.deltaTime;

    //SM.SetHp(Time.deltaTime / 5f);

    if (SM.dialogOpened)
      return;

		if (SM.keyJump)
		{
			if (!isGrounded)
				return;

			body.AddForce(new Vector2(0, 5) * jumpSpeed, ForceMode2D.Impulse);
			_isGrounded = false;
      animator.SetTrigger("Jump");
			return;
		}

    if (isMeele && SM.keyWeapon1)
    {

      currentWeapon = _currentWeapon == 1 ? 0 : 1;
      return;
    }
    if (isRange && SM.keyWeapon2)
    {
      currentWeapon = _currentWeapon == 2 ? 0 : 2;
      return;
    }

    if (_currentWeapon > 0 && SM.keyAttack)
    {
      if (_currentWeapon == 1)
      {
        if (meeleTrigger.attacking)
          return;

        meeleTrigger.coolDown = meeleCooldown;
        meeleTrigger.attacking = true;
      }

      animator.SetTrigger("Attack");
      return;
    }

	}

	private void FixedUpdate()
	{
    if (IsDed)
      return;


    groundChecked = false;
		Vector2 move = body.velocity;
		move.x = SM.keyMove * Time.deltaTime * speed * 100f;

    animator.SetBool("Falling", move.y < -0.1f);
    animator.SetBool("Grounded", isGrounded);

    if (Mathf.Abs(move.x) < 0.1f)
    {
      animator.SetFloat("Speed",0);
      move.x = 0;
    }
    else
    {
      bool nRight = move.x < 0;
      bool isRight = animator.GetBool("Right");
      if (nRight != isRight) {

        animator.SetBool("Right", nRight);
        animator.SetTrigger("ChangeDirection");
      }

      animator.SetFloat("Speed", 1);
    }

    body.velocity = move;
  }


  bool groundChecked;
	bool _isGrounded;
	bool isGrounded => groundChecked ? _isGrounded : GroundCheck();

	bool GroundCheck()
	{
		groundChecked = true;
		_isGrounded = body.IsTouching(groundFilter);
		return _isGrounded;
	}
  private bool GetGroundCollider(out Collider2D collider)
  {
    collider = null;
    if (!isGrounded)
      return false;

    Collider2D[] colliders = new Collider2D[1];
    if (body.GetContacts(groundFilter, colliders) < 1)
      return false;

    //Debug.Log("Ground - " + colliders[0].name);
    collider = colliders[0];
    return true;
  }
  public void Ded()
  {
    if (IsDed)
      return;

    IsDed = true;
    body.isKinematic = true;
    body.velocity = Vector2.zero;
    animator.SetTrigger("Ded");

    Debug.Log("Player is ded");
  }
  public void SetSpawn(Vector2 pos)
  {
    if (!IsDed)
      return;

    IsDed = false;
    transform.position = pos;
    body.isKinematic = false;
    body.velocity = Vector2.zero;
    animator.SetTrigger("Respawn");
    animator.SetFloat("Speed", 0);

    Debug.Log("Player is respawn");
  }
  public static void Respawn()
  {
    SM.player.SetSpawn(checkPoint);
  }
  public static void Save()
  {
    Debug.Log("Save position");
    checkPoint = SM.player.transform.position;
  }

  private float coolDownHp = 0;
  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (coolDownHp > 0)
      return;

    Enemy enemy = collision.GetComponentInParent<Enemy>();
    if (!enemy)
      return;

    Debug.Log(collision.name);
    SM.SetHp(enemy.attackDamage / 100f);
    coolDownHp = 1;
    
  }
}