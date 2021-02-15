using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LyricEditor2InputMenu : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    InputField inputField;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public string GetUserInput()
    {
        return inputField.text;
    }

    // Open the input panel to accept user input
    public void StartEdit(string startingText)
    {
        inputField.text = startingText;
        gameObject.SetActive(true);
    }

    public void StartEdit()
    {
        StartEdit("");
    }
}
