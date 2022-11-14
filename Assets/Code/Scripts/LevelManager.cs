using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int amountOfPokemon;
    private int amountOfStartingPokemon;
    private int amountOfPokemonCaught;

    public float currentTime;
    public float endTime;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        amountOfPokemon = GameObject.FindGameObjectsWithTag("Pokemon").Length;
        amountOfStartingPokemon = amountOfPokemon;
        endTime = 15f;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        Debug.Log(currentTime);
        if (currentTime >= endTime)
        {
            endGame("before time ran out!");
        }
    }

    public void removePokemon(GameObject pokemon, bool captured)
    {
        Destroy(pokemon);
        amountOfPokemon--;
        
        if (captured)
        {
            amountOfPokemonCaught++;
            Debug.Log("Success! You have captured a Psyduck!");
        }
        else
        {
            Debug.Log("Whoops! A Psyduck escaped!");
        }
        
        if (amountOfPokemon <= 0)
        {
            endGame("before they all escaped!");
        }
    }

    public void endGame(string reason)
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Well done! You have captured " + amountOfPokemonCaught
                                                  + (amountOfPokemonCaught == 1 ? " Psyduck " : " Psyducks ") + reason);
    }
}
