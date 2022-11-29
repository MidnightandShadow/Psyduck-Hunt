using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class LevelManager : MonoBehaviour
{
    private int amountOfPokemon;
    private int amountOfPokemonCaught;

    public float currentTime;
    public float endTime;

    private GUIManager GUI;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        amountOfPokemon = GameObject.FindGameObjectsWithTag("Pokemon").Length;
        endTime = 180f;

        GUI = GameObject.Find("GUI").GetComponent<GUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
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
            GUI.reportToPlayer("Success!", amountOfPokemon + " remaining", 2.0f);
        }
        else
        {
            GUI.reportToPlayer("A Psyduck escaped!", amountOfPokemon + " remaining", 2.0f);
        }
        
        if (amountOfPokemon <= 0)
        {
            endGame("before they all escaped!");
        }
    }

    private void endGame(string reason)
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        GUI.reportToPlayer("Well done!", "You have captured "
                                         + amountOfPokemonCaught 
                                         + (amountOfPokemonCaught == 1 ? " Psyduck " : " Psyducks "), reason);
    }
}
