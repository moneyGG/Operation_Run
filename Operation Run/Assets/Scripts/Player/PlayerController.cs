using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    /*TODO
     * Clean up weapon script/interaction
     * try to move weapon specific logic out of UseWeapon and into their own scripts
     */
    [Header("----- Componets -----")]
    [SerializeField] CharacterController controller;
    /// <summary>
    /// to shoot from the weapon model 
    /// </summary>
    public Transform shootPointVisual;
    /// <summary>
    /// to shoot from the center of reticle/camera
    /// </summary>
    public Transform shootPointCenter;

    


    [Header("----- Player Stats -----")]
    [Range(0, 100)] [SerializeField] float walkSpeed;
    [Range(10, 35)] [SerializeField] float gravity;
    [Range(5, 15)] [SerializeField] float jumpSpeed;
    [Range(1, 3)] [SerializeField] int jumpsMax;
    public List<Keys> keyList = new List<Keys>();
    int jumpsCur;
    Vector3 move;
    Vector3 playerVelocity;

    int hpMax;
    [Range(0,100)] [SerializeField] int hp;

    [Header("----- Weapon Stats -----")]
    [SerializeField] float wUseTime;
    [SerializeField] float wRange;
    [SerializeField] int wDamage;
    [SerializeField] MeshFilter wModel;
    [SerializeField] MeshRenderer wMaterial;
    [SerializeField] Weapon weapon;

    bool isUsingWeapon;

    // Start is called before the first frame update
    void Start()
    {
        hpMax = hp;
        UpdateHealthUI();
        if(GameManager.instance.playerSpawnPosition != null) // stops game from breaking if no spawn point set. Helps with testing.
        {
            SpawnPlayer();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isPaused)
        {
            Movement();
            Weapons();
        }
      
    }

    private void Weapons()
    {
        if (Input.GetButton("Fire1") & !isUsingWeapon)
        {
            StartCoroutine(UseWeapon());
        }
    }

    void Movement()
    {
        //gravity and jumping
        if(controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
            jumpsCur = 0;
        }
        else
        {
            playerVelocity.y -= gravity * Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump") && jumpsCur < jumpsMax)
        {
            playerVelocity.y = jumpSpeed;
            ++jumpsCur;
        }

        //player movement input
        move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        move = move.normalized;

        //move controller
        controller.Move(move * Time.deltaTime * walkSpeed);
        controller.Move(playerVelocity * Time.deltaTime); // this needs to come after movement or it causes issues.
    }

    IEnumerator UseWeapon()
    {
        isUsingWeapon = true;
        if (weapon != null)
        {
            GameObject bulletClone = Instantiate(weapon.bullet, shootPointCenter.position, weapon.bullet.transform.rotation);
            if (bulletClone.GetComponent<Rigidbody>() != null)
            {
                //GameObject visualClone = Instantiate(weapon.bullet, shootPointVisual.position, weapon.bullet.transform.rotation);
                //visualClone.GetComponent<Collider>().enabled = false;
                //visualClone.GetComponent<Rigidbody>().velocity = (shootPointCenter.position - shootPointVisual.position).normalized * weapon.bulletSpeed;
                //Destroy(visualClone,0.1f);
                bulletClone.GetComponent<Rigidbody>().velocity = Camera.main.transform.forward * weapon.bulletSpeed;
            }else if (bulletClone.GetComponent<LineRenderer>())
            {

            }
            yield return new WaitForSeconds(wUseTime);
        }
        isUsingWeapon = false;
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        UpdateHealthUI();
        StartCoroutine(GameManager.instance.PlayerHitFlash());

        if (hp <= 0)
        {
            GameManager.instance.PlayerDead();
        }
    }

    public void SpawnPlayer()
    {
        hp = hpMax;
        UpdateHealthUI();
        controller.enabled = false;
        transform.position = GameManager.instance.playerSpawnPosition.transform.position;
        controller.enabled = true;
    }

    public void UpdateHealthUI()
    {
        GameManager.instance.playerHealthBar.fillAmount = (float)hp / (float)hpMax;
    }

    void Teleport(Vector3 pos)
    {
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
    }
    public void keyPickup(Keys key)
    {
        keyList.Add(key);
    }

    

    public void ChangeWeapon(Weapon weap)
    {
        weapon = weap;
        wDamage = weap.damage;
        wRange = weap.range;
        wUseTime = weap.useTime;

        wModel.sharedMesh = weap.model.GetComponent<MeshFilter>().sharedMesh;
        wMaterial.sharedMaterial = weap.model.GetComponent<MeshRenderer>().sharedMaterial;
    }

}
