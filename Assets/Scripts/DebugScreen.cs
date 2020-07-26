using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Player player;
    Text text;

    float frameRate;
    float timer;


    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();
        text = GetComponent<Text>();
    }

    void Update()
    {
        string debugText = "Minecraft Unity Edition!";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n";
        debugText += "XYZ: (" + Mathf.FloorToInt(world.player.transform.position.x) + "," + Mathf.FloorToInt(world.player.transform.position.y) + "," + Mathf.FloorToInt(world.player.transform.position.z) + ")";
        debugText += "\n";
        debugText += "Chunk: (" + world.playerCurrChunkCoord.x + "," + world.playerCurrChunkCoord.z + ")";
        debugText += "\n";
        debugText += "(" + player.mouseHorizontal + "," + player.mouseHorizontal + ")";

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
