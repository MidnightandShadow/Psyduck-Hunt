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
    private static readonly int Saunter = Animator.StringToHash("Saunter");
    private static readonly int Flee = Animator.StringToHash("Flee");
    private static readonly int Dig = Animator.StringToHash("Dig");

    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        
        trevor = GameObject.Find("Trevor");

        walkSpeed = GetComponent<NavMeshAgent>().speed;

        pokemonAnimator = GetComponent<Animator>();
        
        SwitchToState(State.Chill);
        UpdatePokemonAnimator(false,false,false);

        pokemonAS1 = GetComponents<AudioSource>()[0];
        pokemonAS2 = GetComponents<AudioSource>()[1];

        pokemonAS1.volume = 1f;
        pokemonAS1.spatialBlend = 1f;
        pokemonAS1.maxDistance = 5f;
        
        pokemonAS2.volume = 0.35f;
        pokemonAS2.spatialBlend = 1f;
        pokemonAS2.maxDistance = 5f;

        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();

        silence = true;
        Invoke(nameof(resetSilence), Random.Range(3f, 8f));

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
                    
                    Invoke(nameof(SwitchToSaunter), Random.Range(5.0f, 6.0f));
                    UpdatePokemonAnimator(false, false, false);
                    
                    GetComponent<NavMeshAgent>().speed = 0.0f;
                    
                    transitionActive = false;
                }
                
                if ( InView(trevor, viewAngle, viewDistance) )
                {
                    // to leave time for surprise animation
                    UpdatePokemonAnimator(false, true, false);
                    Invoke(nameof(SwitchToFlee), 1.6f);
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
                    UpdatePokemonAnimator(false,false,false);
                }
                
                if ( InView(trevor, viewAngle, viewDistance) )
                {
                    // to leave time for surprise animation
                    UpdatePokemonAnimator(false, true, false);
                    Invoke(nameof(SwitchToFlee), 1.6f);
                }
                break;
            
            case State.Flee:
                playSound(State.Flee);
                if (transitionActive)
                {
                    CancelInvoke(nameof(SwitchToSaunter));
                    CancelInvoke(nameof(SwitchToFlee));
                    
                    Invoke(nameof(CheckForDig), 10f);

                    currentDestination = ValidDestination(true);
                    GetComponent<NavMeshAgent>().destination = currentDestination;

                    GetComponent<NavMeshAgent>().speed = runSpeed;
                    
                    transitionActive = false;
                }

                if ( (transform.position - currentDestination).magnitude < 2.5f)
                {
                    CancelInvoke(nameof(CheckForDig));
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

    public void SwitchToFleeSurprise()
    {
        UpdatePokemonAnimator(false,true,false);
        Invoke(nameof(SwitchToFlee), 1.2f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(SwitchToSaunter));
        CancelInvoke(nameof(SwitchToFlee));
        CancelInvoke(nameof(CheckForDig));
        
        // SwitchToState(State.Flee);
    }

    private void CheckForDig()
    {
        if ((transform.position - trevor.transform.position).magnitude > 28f)
        {
            SwitchToState(State.Chill);
            UpdatePokemonAnimator(false,false,false);
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
        pokemonAnimator.SetBool(Saunter, saunter);
        pokemonAnimator.SetBool(Flee, flee);
        pokemonAnimator.SetBool(Dig, dig);
    }
    
    void playSound(State currentStateParam)
    {
        if (currentStateParam.Equals(State.Chill) || currentStateParam.Equals(State.Saunter))
        {
            // for fleeing (but I didn't really like the looping, so it's just one time)
            // pokemonAS1.loop = false;
            if (!silence && Random.Range(0,10) == 1)
            {
                pokemonAS1.clip = psySounds[Random.Range(0,psySounds.Length)];
                pokemonAS1.Play();
                silence = true;
                Invoke(nameof(resetSilence), Random.Range(3f,8f));
            }
        }

        if (currentStateParam.Equals(State.Flee) && transitionActive)
        {
            pokemonAS1.clip = panicSounds[Random.Range(0, panicSounds.Length)];
            // pokemonAS1.loop = true;
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
            var position = trevor.transform.position;
            x = position.x - boundaries[0, 0] >= boundaries[0, 1] - position.x ? boundaries[0, 0] : boundaries[0, 1];

            z = trevor.transform.position.z - boundaries[1, 0] >= boundaries[1, 1] - position.z ? boundaries[1, 0] : boundaries[1, 1];
        }
        
        Vector3 destination = new Vector3(x, Terrain.activeTerrain
            .SampleHeight(new Vector3(x, 0.0f, z)), z);

        return destination;
    }

    private bool InView(GameObject target, float viewingAngle, float viewingDistance)
    {
        var transform1 = transform;
        var position = target.transform.position;
        
        float dotProduct = Vector3.Dot(transform1.forward,
            Vector3.Normalize(position - transform1.position));

        float view = 1.0f - viewingAngle;

        float distance = (transform.position - position).magnitude;

        if (dotProduct >= view && distance <= viewingDistance)
        {
            return true;
        }

        return false;
    }

    private float[] GetTextureMix(Vector3 pokemonPosition)
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

    private string FootStepLayerName(Vector3 pokemonPosition)
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
        
        // randomize pitch within a reasonable amount
        int randomPitch = Random.Range(0, 10);
        
        switch (randomPitch)
        {
            case 0:
                pokemonAS2.pitch = 0.85f;
                break;
            case 1:
                pokemonAS2.pitch = 0.88f;
                break;
            case 2:
                pokemonAS2.pitch = 0.9f;
                break;
            case 3:
                pokemonAS2.pitch = 0.93f;
                break;
            case 4:
                pokemonAS2.pitch = 0.95f;
                break;
            case 5:
                pokemonAS2.pitch = 1.03f;
                break;
            case 6:
                pokemonAS2.pitch = 1.05f;
                break;
            case 7:
                pokemonAS2.pitch = 1.08f;
                break;
            case 8:
                pokemonAS2.pitch = 1.12f;
                break;
            case 9:
                pokemonAS2.pitch = 1.15f;
                break;
            default:
                pokemonAS2.pitch = 1;
                break;
        }


        pokemonAS2.Play();
    }
}
