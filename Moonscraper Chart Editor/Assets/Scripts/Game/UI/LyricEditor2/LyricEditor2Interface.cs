using System.Collections.Generic;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;
using System.Text.RegularExpressions;
using System.Linq;

public class LyricEditor2Interface : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    LyricEditor2PhraseController phraseTemplate;
    [UnityEngine.SerializeField]
    LyricEditor2InputMenu inputMenu;

    List<string> testLyrics = new List<string>();
    List<LyricEditor2PhraseController> currentPhrases = new List<LyricEditor2PhraseController>();
    LyricEditor2PhraseController currentPhrase;
    uint lastSafeTick = 0;

    // Start is called before the first frame update
    void Start()
    {
        phraseTemplate.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Parse a string into a double string array (phrases of syllables) to be
    // given as Phrase Controller input. Does not have an implemented time-out
    // period in case of excessively long strings to be parsed.
    List<List<string>> ParseLyrics(string inputString)
    {
        // Start by splitting the string into phrases
        char[] newlineCharacters = {'\n', '\r'};
        string[] tempPhrases = inputString.Split(newlineCharacters, System.StringSplitOptions.RemoveEmptyEntries);

        // Prepare the regex engine to parse each phrase
        List<List<string>> parsedLyrics = new List<List<string>>();
        // [^-\s]+      matches one or more characters in a syllable, excluding
        //                  spaces and dashes
        // (-\s?|\s?)   matches a dash, or whitespace if no dash is found
        string regexPattern = @"[^-\s]+(-|\s?)";
        Regex rx = new Regex(regexPattern);

        foreach (string basePhrase in tempPhrases)
        {
            // Match each phrase
            MatchCollection matches = rx.Matches(basePhrase);
            // Convert the MatchCollection into a List and append that to
            // parsedLyrics
            List<string> matchesList = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                matchesList.Add(matches[i].ToString());
            }
            parsedLyrics.Add(matchesList);
        }

        return parsedLyrics;
    }

    // Grab the user's lyric input and use it to construct new phrase objects
    public void SubmitLyrics()
    {
        string userInput = inputMenu.GetUserInput();
        List<List<string>> parsedLyrics = ParseLyrics(userInput);
        for (int i = 0; i < parsedLyrics.Count; i++)
        {
            List<string> lyricPhraseStrings = parsedLyrics.ElementAt(i);
            LyricEditor2PhraseController phraseController = UnityEngine.GameObject.Instantiate(phraseTemplate, phraseTemplate.transform.parent).GetComponent<LyricEditor2PhraseController>();
            phraseController.gameObject.SetActive(true);
            phraseController.InitializeSyllables(lyricPhraseStrings);
            currentPhrases.Add(phraseController);
        }
        // If one or more phrases were created, set currentPhrase to the first
        // one
        if (currentPhrases.Count > 0)
        {
            currentPhrase = currentPhrases.ElementAt(0);
        }
    }

    // Get rid of any phrases in currentPhrases
    void ClearCurrentPhrases()
    {
        foreach(LyricEditor2PhraseController phrase in currentPhrases)
        {
            phrase.ClearFromEditor();
            UnityEngine.Object.Destroy(phrase.gameObject);
        }
        currentPhrases.Clear();
        currentPhrase = null;
    }

    // Open the lyric input menu
    public void ActivateLyricInputMenu()
    {
        inputMenu.StartEdit("1-2-3-4-5-6-7-8-9-0");
    }

    // Called when the "Place Lyric" button is pressed down; starts placing the
    // next-up lyric.
    public void OnPlaceButtonPressed()
    {
        uint currentTick = ChartEditor.Instance.currentTickPos;
        if (!currentPhrase.allSyllablesPlaced)
        {
            currentPhrase.PlaceNextLyric(currentTick);
        }
    }

    // Called when the "Place Lyric" button is released; stops placing down the
    // previous lyric
    public void OnPlaceButtonReleased()
    {
        uint currentTick = ChartEditor.Instance.currentTickPos;
        currentPhrase.StopPlacingLyric(currentTick);
        if (currentPhrase.allSyllablesPlaced)
        {
            lastSafeTick = currentPhrase.phraseEndTick;
            // MoveToNextPhrase();
        }
    }
}
