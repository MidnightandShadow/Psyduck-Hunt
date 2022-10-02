using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PokeballController : MonoBehaviour
{
    [SerializeField]
    private Animator pokeballAnimator;

    [SerializeField] 
    private ParticleSystem pokeflashPF;
    
    
    private GameObject pokemon;
    private GameObject terrain;
    private int animationStage;
    private bool didOnce;
    private Transform trevor;
    private bool escaped;
    private bool checkForEscape = true;
    
    // Start is called before the first frame update
    void Start()
    {
        pokeballAnimator.speed = 0;
        trevor = GameObject.Find("Trevor").transform.Find("CameraFocus");
    }

    // "Update but for physics"
    private void FixedUpdate()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (pokemon == null) return;
        
        switch (animationStage)
        {
            case 0: // Apply upwards force from collision with pokemon
                rigidbody.AddForce(Vector3.up * 3, ForceMode.Impulse);
                animationStage++;
                break;
                
            case 1: // check for when pokeball starts falling back down
                if (rigidbody.velocity.y < 0)
                {
                    animationStage++;
                }
                break;
                
            case 2: // stop pokeball in mid-air, rotate towards pokemon, open, spawn particle on pokemon,
                // remove pokemon
                rigidbody.isKinematic = true; // its physics stop, hangs in mid-air

                Vector3 pokemonTransformPosition = pokemon.transform.position;
                        
                Quaternion rotationTowardsPokemon =
                    Quaternion.LookRotation(pokemonTransformPosition - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    rotationTowardsPokemon, Time.fixedDeltaTime * 3); // rotate towards

                pokeballAnimator.speed = 4; // speed up when opening (handled by animator)

                if (!didOnce)
                {
                    // spawn particle on pokemon
                    Instantiate(pokeflashPF, pokemonTransformPosition, quaternion.identity);
                    didOnce = true;
                }
                        
                // remove pokemon (deactived instead of destroyed in case it escapes later)
                pokemon.SetActive(false);

                if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                    pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Open"))
                {
                    animationStage++;
                }
                        
                break;
                
            case 3: // Close pokeball
                pokeballAnimator.SetInteger("State", 1);
                    
                if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                    pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Close"))
                {
                    animationStage++;
                    terrain = null;
                }
                break;
                
            case 4: // rotate towards player and fall to the ground
                transform.LookAt(trevor, Vector3.up); // rotate towards player

                rigidbody.isKinematic = false; // fall to the ground

                if (terrain != null)
                {
                    animationStage++;
                }
                break;
                
            case 5: // remove physics and do wiggle animation
                rigidbody.isKinematic = true;
                    
                pokeballAnimator.SetInteger("State", 2);
                pokeballAnimator.speed = 1.5f;

                float r = Random.Range(1, 11);

                if (checkForEscape)
                {
                    if (r.Equals(1))
                    {
                        escaped = true;
                        pokeballAnimator.speed = 0;
                        didOnce = false;
                        animationStage++;
                    }

                    StartCoroutine(WaitForCheck(1));
                    checkForEscape = false;
                    
                }
                    
                if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3.0f &&
                    pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Wiggle"))
                {
                    pokeballAnimator.speed = 0;
                    didOnce = false;
                    animationStage++;
                }
                break;
                
            case 6: // capture pokemon or it escapes
                Vector3 pokemonTransformPosition2 = pokemon.transform.position;

                if (escaped)
                {
                    if (!didOnce)
                    {
                        // spawn particle on pokemon
                        Instantiate(pokeflashPF, pokemonTransformPosition2, quaternion.identity);
                        didOnce = true;
                    }
                    Destroy(gameObject);
                    pokemon.SetActive(true);
                }
                else
                {
                    Destroy(pokemon);
                }
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidingWith = collision.gameObject;
        if (collidingWith.CompareTag("Pokemon") && pokemon == null)
        {
            pokemon = collidingWith;
        }

        if (collidingWith.name.Equals("Terrain"))
        {
            terrain = collidingWith;
        }
    }

    IEnumerator WaitForCheck(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        checkForEscape = true;
    }
}
