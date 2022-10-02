using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PokemonController : MonoBehaviour
{
    private enum State
    {
        Chill, Saunter, Flee, Dig
    }

    [SerializeField] private State currentState;
    private bool transitionActive;
    
    private GameObject trevor;

    [SerializeField] private Vector3 currentDestination;

    [SerializeField] private float runSpeed;

    [SerializeField] private float walkSpeed;

    // for when Psyduck should want to start fleeing
    private float viewAngle = 0.25f;
    private float viewDistance = 5f;

    private Animator pokemonAnimator;
    
    // Start is called before the first frame update
    void Start()
    {
        trevor = GameObject.Find("Trevor");

        walkSpeed = GetComponent<NavMeshAgent>().speed;

        pokemonAnimator = GetComponent<Animator>();
        
        SwitchToState(State.Chill);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Chill:
                if (transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    
                    Invoke("SwitchToSaunter", Random.Range(5.0f, 6.0f));
                    UpdatePokemonAnimator(false, false, false);
                    
                    GetComponent<NavMeshAgent>().speed = 0.0f;
                    
                    transitionActive = false;
                }
                
                if ( InView(trevor, viewAngle, viewDistance) )
                {
                    // to leave time for surprise animation
                    UpdatePokemonAnimator(false, true, false);
                    Invoke("SwitchToFlee", 1.6f);
                }
                
                break;
            
            case State.Saunter:
                if (transitionActive)
                {
                    currentDestination = ValidDestination(false);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    
                    UpdatePokemonAnimator(true, false, false);
                    
                    GetComponent<NavMeshAgent>().speed = walkSpeed;
                    
                    transitionActive = false;
                }

                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    SwitchToState(State.Chill);
                }
                
                if ( InView(trevor, viewAngle, viewDistance) )
                {
                    // to leave time for surprise animation
                    UpdatePokemonAnimator(false, true, false);
                    Invoke("SwitchToFlee", 1.6f);
                }
                
                break;
            
            case State.Flee:
                if (transitionActive)
                {
                    CancelInvoke("SwitchToSaunter");
                    CancelInvoke("SwitchToFlee");
                    
                    Invoke("CheckForDig", 10f);

                    currentDestination = ValidDestination(true);
                    GetComponent<NavMeshAgent>().destination = currentDestination;

                    GetComponent<NavMeshAgent>().speed = runSpeed;
                    
                    transitionActive = false;
                }

                if ( (transform.position - currentDestination).magnitude < 2.5f)
                {
                    CancelInvoke("CheckForDig");
                    CheckForDig();
                }
                
                break;
            
            case State.Dig:
                if (transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().speed = 0f;

                    UpdatePokemonAnimator(false, false, true);
                    
                    transitionActive = false;
                }
                
                break;
        }
    }

    private void SwitchToState(State newState)
    {
        transitionActive = true;
        currentState = newState;
    }
    
    private void SwitchToSaunter()
    {
        SwitchToState(State.Saunter);
    }

    private void SwitchToFlee()
    {
        SwitchToState(State.Flee);
    }

    private void OnDisable()
    {
        CancelInvoke("SwitchToSaunter");
        CancelInvoke("SwitchToFlee");
        CancelInvoke("CheckForDig");
        
        SwitchToState(State.Flee);
    }

    private void CheckForDig()
    {
        if ((transform.position - trevor.transform.position).magnitude > 28f)
        {
            SwitchToState(State.Chill);
        } else
        {
            SwitchToState(State.Dig);
        }
    }

    public void DigCompleted()
    {
        Destroy(gameObject);
    }

    private void UpdatePokemonAnimator(bool saunter, bool flee, bool dig)
    {
        pokemonAnimator.SetBool("Saunter", saunter);
        pokemonAnimator.SetBool("Flee", flee);
        pokemonAnimator.SetBool("Dig", dig);
    }

    private Vector3 ValidDestination(bool avoidTrevor)
    {
        float[,] boundaries = { { -60f, 65f }, { -60f, 55f } };
        
        float x = Random.Range(boundaries[0,0], boundaries[0, 1]);
        float z = Random.Range(boundaries[1,0], boundaries[1, 1]);

        if (avoidTrevor)
        {
            if (trevor.transform.position.x - boundaries[0, 0] >= boundaries[0, 1] - trevor.transform.position.x)
            {
                x = boundaries[0, 0];
            }
            else
            {
                x = boundaries[0, 1];
            }
            
            if (trevor.transform.position.z - boundaries[1, 0] >= boundaries[1, 1] - trevor.transform.position.z)
            {
                z = boundaries[1, 0];
            }
            else
            {
                z = boundaries[1, 1];
            }
        }
        
        Vector3 destination = new Vector3(x, Terrain.activeTerrain
            .SampleHeight(new Vector3(x, 0.0f, z)), z);

        return destination;
    }

    private bool InView(GameObject target, float viewingAngle, float viewingDistance)
    {
        float dotProduct = Vector3.Dot(transform.forward,
            Vector3.Normalize(target.transform.position - transform.position));

        float view = 1.0f - viewingAngle;

        float distance = (transform.position - target.transform.position).magnitude;

        if (dotProduct >= view && distance <= viewingDistance)
        {
            return true;
        }

        return false;
    }
}
