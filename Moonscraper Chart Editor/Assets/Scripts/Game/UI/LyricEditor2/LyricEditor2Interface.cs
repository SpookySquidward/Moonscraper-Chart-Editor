using System.Collections.Generic;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;
using System.Text.RegularExpressions;
using System.Linq;

public class LyricEditor2Interface : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    LyricEditor2PhraseController phraseTemplate;

    // Start is called before the first frame update
    void Start()
    {
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
}
