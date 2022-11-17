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
    
    private LevelManager levelManager;

    [SerializeField] private AudioClip[] psySounds;
    [SerializeField] private AudioClip[] panicSounds;
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;

    private AudioSource pokemonAS1;
    private AudioSource pokemonAS2;

    private bool silence;
    
    private Terrain terrain;

    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        
        trevor = GameObject.Find("Trevor");

        walkSpeed = GetComponent<NavMeshAgent>().speed;

        pokemonAnimator = GetComponent<Animator>();
        
        SwitchToState(State.Chill);

        pokemonAS1 = GetComponents<AudioSource>()[0];
        pokemonAS2 = GetComponents<AudioSource>()[1];

        pokemonAS1.volume = 1f;
        pokemonAS1.spatialBlend = 1f;
        pokemonAS1.maxDistance = 5f;
        
        pokemonAS2.volume = 0.25f;
        pokemonAS2.spatialBlend = 1f;
        pokemonAS2.maxDistance = 5f;

        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();

        silence = true;
        Invoke("resetSilence", Random.Range(3f, 8f));

        FootStepLayerName(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Chill:
                playSound(State.Chill);
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
                playSound(State.Saunter);
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
        levelManager.removePokemon(gameObject, false);
    }

    private void UpdatePokemonAnimator(bool saunter, bool flee, bool dig)
    {
        pokemonAnimator.SetBool("Saunter", saunter);
        pokemonAnimator.SetBool("Flee", flee);
        pokemonAnimator.SetBool("Dig", dig);
    }
    
    void playSound(State currentState)
    {
        if (currentState.Equals(State.Chill) || currentState.Equals(State.Saunter))
        {
            // for fleeing
            pokemonAS1.loop = false;
            if (!silence && Random.Range(0,10) == 1)
            {
                pokemonAS1.clip = psySounds[Random.Range(0,psySounds.Length)];
                pokemonAS1.Play();
                silence = true;
                Invoke("resetSilence", Random.Range(3f,8f));
            }
        }

        if (currentState.Equals(State.Flee) && transitionActive)
        {
            pokemonAS1.clip = panicSounds[Random.Range(0, panicSounds.Length)];
            pokemonAS1.loop = true;
            pokemonAS1.Play();
        }
    }

    void resetSilence()
    {
        silence = false;
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

    public float[] GetTextureMix(Vector3 pokemonPosition)
    {
        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        // Position of pokemon in relation to terrain alpha map
        int mapPositionX =
            Mathf.RoundToInt( (pokemonPosition.x - terrainPosition.x) / terrainData.size.x
                              * terrainData.alphamapWidth);
        
        int mapPositionZ =
            Mathf.RoundToInt( (pokemonPosition.z - terrainPosition.z) / terrainData.size.z
                              * terrainData.alphamapHeight);

        // 3D: format: x, z, percentage of the terrain layers (grass vs sand) used
        float[,,] splatMapData = terrainData.GetAlphamaps(mapPositionX, mapPositionZ, 1, 1);

        // Extract all the values into the cell mix
        float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];
        
        // Converting 3D array to 1D array
        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatMapData[0, 0, i];
        }

        return cellMix;
    }

    public string FootStepLayerName(Vector3 pokemonPosition)
    {
        float[] cellMix = GetTextureMix(pokemonPosition);
        float strongestTexture = 0f;
        int maxIndex = 0;

        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestTexture)
            {
                strongestTexture = cellMix[i];
                maxIndex = i;
            }
        }

        return terrain.terrainData.terrainLayers[maxIndex].name;
    }

    public void footStep()
    {
        pokemonAS2.clip = grassSounds[Random.Range(0, grassSounds.Length)];

        if (FootStepLayerName(transform.position).Equals("TL_Sand"))
        {
            pokemonAS2.clip = sandSounds[Random.Range(0, sandSounds.Length)];
        }
        pokemonAS2.Play();
    }
}
