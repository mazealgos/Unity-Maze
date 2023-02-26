using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageText : MonoBehaviour
{

    public GameObject textObject;
    Text text;

    public void clearText()
    {
        text.text = "";
        text.color = new Color(1f, 1f, 1f, 1f);
    }

    public void displayMessage(string message, Color color)
    {
        clearText();
        text.color = color;
        text.text = message;
    }

    public void displayMessage(string message)
    {
        text.color = new Color(1f, 1f, 1f, 1f);
        text.text = message;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        text = textObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
