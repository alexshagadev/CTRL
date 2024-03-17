using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugJump : MonoBehaviour
{
    Material playerMat;
    Color playerColor;

    private void Start()
    {
        // Get a reference to the player mat and extract its color.
        playerMat = GetComponent<SpriteRenderer>().material;
        playerColor = playerMat.color;
    }

    void Update()
    {
        // If the player presses space at any time, switch the color of their material to red for a brief moment.
        // This will make it clear when the player is trying to jump in the demonstration video.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(FlashRed());
        }
    }

    // This coroutine will create a slight delay before we switch the material color back to the original.
    IEnumerator FlashRed()
    {
        playerMat.color = Color.red;
        yield return new WaitForSeconds(0.2f); // Wait for 0.2 seconds
        playerMat.color = playerColor; // Revert to the original color
    }
}