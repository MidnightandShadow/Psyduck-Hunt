using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Insert character controller")]
    private CharacterController controller;
    
    [FormerlySerializedAs("camera")]
    [SerializeField]
    [Tooltip("Insert main camera")]
    private Camera mainCamera;
    
    [SerializeField]
    [Tooltip("Insert animator controller")]
    private Animator playerAnimator;
    
    [SerializeField]
    [Tooltip("Insert pokeball prefab")]
    private GameObject pokeballPF;
    
    [SerializeField]
    [Tooltip("Insert pokeball bone Transform")]
    private Transform pokeballBone;

    [SerializeField]
    private float speed = 3f;

    [SerializeField]
    private float runSpeed = 7f;

    [SerializeField] 
    private float throwStrength = 4f;

    private Vector3 velocity;
    private readonly float gravity = -9.82f;
    private readonly float jumpHeight = 15f;
    private readonly float groundCastDistance = 0.10f;
    private bool grounded;
    private bool throwing;
    private GameObject instantiatedPokeball;
    
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;

    private AudioSource playerAS1;

    private Terrain terrain;
    private static readonly int IsThrowing = Animator.StringToHash("isThrowing");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");

    // Start is called before the first frame update
    void Start()
    {
        playerAS1 = GetComponent<AudioSource>();
        playerAS1.volume = 0.35f;
        playerAS1.spatialBlend = 1f;
        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
    }

    // Update is called once per frame
    void Update()
    {
        // Grab transforms
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;

        grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCastDistance);

        if (Time.timeScale > 0)
        {
            // wasd/left right up down ground movement
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 movement = (playerTransform.right * x) + (playerTransform.forward * z);

            // throwing
            if (Input.GetButtonDown("Fire1") && grounded && !throwing)
            {
                throwing = true;
                SpawnPokeballToBone();
                playerAnimator.SetBool(IsThrowing, true);
            }

            if (!throwing)
            {
                // detect if running
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    playerAS1.volume = 0.45f;
                    controller.Move(movement * (runSpeed * Time.deltaTime));
                }
                else
                {
                    playerAS1.volume = 0.35f;
                    controller.Move(movement * (speed * Time.deltaTime));
                }

                // gravity and jumping
                velocity.y += gravity * Time.deltaTime;

                if (Input.GetButtonDown("Jump") && this.grounded)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight);
                }

                controller.Move(velocity * Time.deltaTime);
                playerAnimator.SetBool(IsJumping, !grounded);
            }

            // detect if at least walking
            playerAnimator.SetBool(IsWalking, movement.magnitude > 0);

            if (!throwing)
            {
                playerAnimator.SetBool(IsRunning, Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0);
            }

            // Rotate player alongside camera
            playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
        }
    }

    public void ThrowEnded()
    {
        throwing = false;
        playerAnimator.SetBool(IsThrowing, false);
    }

    private void SpawnPokeballToBone()
    {
        if (instantiatedPokeball == null)
        {
            instantiatedPokeball = Instantiate(pokeballPF, pokeballBone, false);
        }
    }

    public void ReleasePokeball()
    {
        if (instantiatedPokeball != null)
        {
            instantiatedPokeball.transform.parent = null;
            instantiatedPokeball.GetComponent<SphereCollider>().enabled = true;
            instantiatedPokeball.GetComponent<Rigidbody>().useGravity = true;
            
            Transform cameraTransform = mainCamera.transform;
            Vector3 throwAdjustment = new Vector3(0f, 0.5f, 0f);
            Vector3 throwVector = (cameraTransform.forward + throwAdjustment) * throwStrength;
            
            instantiatedPokeball.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);

            instantiatedPokeball = null;
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private float[] GetTextureMix(Vector3 playerPosition)
    {
        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        // Position of player in relation to terrain alpha map
        int mapPositionX =
            Mathf.RoundToInt( (playerPosition.x - terrainPosition.x) / terrainData.size.x
                              * terrainData.alphamapWidth);
        
        int mapPositionZ =
            Mathf.RoundToInt( (playerPosition.z - terrainPosition.z) / terrainData.size.z
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

    private string FootStepLayerName(Vector3 playerPosition)
    {
        float[] cellMix = GetTextureMix(playerPosition);
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
        playerAS1.clip = grassSounds[Random.Range(0, grassSounds.Length)];

        if (FootStepLayerName(transform.position).Equals("TL_Sand"))
        {
            playerAS1.clip = sandSounds[Random.Range(0, sandSounds.Length)];
        }
        
        // randomize pitch within a reasonable amount
        int randomPitch = Random.Range(0, 10);
        
        switch (randomPitch)
        {
            case 0:
                playerAS1.pitch = 0.85f;
                break;
            case 1:
                playerAS1.pitch = 0.88f;
                break;
            case 2:
                playerAS1.pitch = 0.9f;
                break;
            case 3:
                playerAS1.pitch = 0.93f;
                break;
            case 4:
                playerAS1.pitch = 0.95f;
                break;
            case 5:
                playerAS1.pitch = 1.03f;
                break;
            case 6:
                playerAS1.pitch = 1.05f;
                break;
            case 7:
                playerAS1.pitch = 1.08f;
                break;
            case 8:
                playerAS1.pitch = 1.12f;
                break;
            case 9:
                playerAS1.pitch = 1.15f;
                break;
            default:
                playerAS1.pitch = 1;
                break;
        }
        
        playerAS1.Play();
    }
}