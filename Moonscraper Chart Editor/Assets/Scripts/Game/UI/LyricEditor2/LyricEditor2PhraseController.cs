using System.Collections.Generic;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;
using System.Linq;

public class LyricEditor2PhraseController : UnityEngine.MonoBehaviour
{
    class LyricItem
    {
        public Event lyricItemEvent;
        public string text = "";
        public bool hasBeenPlaced = false;


        public LyricItem(string text)
        {
            this.text = text;
        }

        public LyricItem(Event lyricEvent)
        {
            this.lyricItemEvent = lyricEvent;

            List<SongEditCommand> commands = new List<SongEditCommand>();
            commands.Add(new SongEditAdd(this.lyricItemEvent));
            BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
            ChartEditor.Instance.commandStack.Push(batchedCommands);
        }

        public void SetTime(uint tick)
        {
            List<SongEditCommand> commands = new List<SongEditCommand>();

            if (lyricItemEvent != null)
            {
                commands.Add(new SongEditDelete(this.lyricItemEvent));
            }

            Event newLyric = new Event(this.text, tick);
            commands.Add(new SongEditAdd(newLyric));

            BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
            ChartEditor.Instance.commandStack.Push(batchedCommands);

            this.lyricItemEvent = newLyric;
            this.hasBeenPlaced = true;
        }
    }

    List<LyricItem> lyricEvents = new List<LyricItem>();
    int lyricEventsIndex = 1;
    List<string> displaySyllables = new List<string>();

    public const string c_lyricPrefix = "lyric ";
    public const string c_phraseStartKeyword = "phrase_start";
    public const string c_phraseEndKeyword = "phrase_end";
    public bool phraseStartPlaced = false;
    public bool phraseEndPlaced = false;
    public bool allSyllablesPlaced = false;
    public bool isCurrentlyPlacingLyric = false;
    public uint phraseEndTick;

    [UnityEngine.SerializeField]
    Text phraseText;
    [UnityEngine.SerializeField]
    UnityEngine.Color defaultColor;
    [UnityEngine.SerializeField]
    UnityEngine.Color unfocusedColor;
    [UnityEngine.SerializeField]
    UnityEngine.Color selectionColor;

    string defaultColorString, unfocusedColorString, selectionColorString;

    // Start is called before the first frame update
    void Start()
    {
        defaultColorString = ColorToString(defaultColor);
        unfocusedColorString = ColorToString(unfocusedColor);
        selectionColorString = ColorToString(selectionColor);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Set the tick of the next syllable in lyricEvents. Will not set the phrase
    // start or phrase end events.
    public void PlaceNextLyric(uint tick)
    {
        if (lyricEventsIndex < lyricEvents.Count - 1)
        {
            lyricEvents.ElementAt(lyricEventsIndex).SetTime(tick);
            lyricEventsIndex += 1;

            if (lyricEventsIndex == lyricEvents.Count - 1)
                allSyllablesPlaced = true;
        }
        UpdateDisplayedText();
    }

    // Set the phrase_start event tick
    public void SetPhraseStart(uint tick)
    {
        lyricEvents.ElementAt(0).SetTime(tick);
        phraseStartPlaced = true;
    }

    // Set the phrase_end event tick
    public void SetPhraseEnd(uint tick)
    {
        lyricEvents.ElementAt(lyricEvents.Count - 1).SetTime(tick);
        phraseEndPlaced = true;
        phraseEndTick = tick;
    }

    // Create LyricItem events for each passed syllable and the phrase start
    // and phrase end positions. Must be called before using PlaceNextLyric(),
    // SetPhraseStart(), or SetPhraseEnd()!
    public void InitializeSyllables(List<string> syllables)
    {
        lyricEvents.Clear();

        lyricEvents.Add(new LyricItem(c_phraseStartKeyword));
        foreach (string syllable in syllables)
        {
            lyricEvents.Add(new LyricItem(syllable));
        }
        lyricEvents.Add(new LyricItem(c_phraseEndKeyword));

        GenerateDisplaySyllables(syllables);
        UpdateDisplayedText();
    }

    // Create a list of syllables with added spaces that can be easily displayed
    // to the phraseText component.
    void GenerateDisplaySyllables(List<string> syllables)
    {
        for (int i = 0; i < syllables.Count; i++)
        {
            string currentSyllable = syllables.ElementAt(i);

            // if (!currentSyllable.EndsWith("-"))
            //     currentSyllable += " ";

            displaySyllables.Add(currentSyllable);
        }
    }

    // Converts a Color object into a string with format "#RRGGBBAA", which is
    // recognized for Rich Text.
    string ColorToString(UnityEngine.Color color)
    {
        string formattedValue = "#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
        return formattedValue;
    }

    // Adds Rich Text color formatting to a given string. If the string is
    // null or empty, this method instead passes the string without
    // modification.
    string AddColorTag(string targetString, string colorKey)
    {
        if (!string.IsNullOrEmpty(targetString))
        {
            string tempString = "<color=" + colorKey + ">";
            tempString += targetString;
            tempString += "</color>";
            return tempString;
        }
        return targetString;
    }

    // Uses the current lyricEventsIndex value to color the lyrics in, then
    // pushes those lyrics to the phraseText component
    public void UpdateDisplayedText()
    {
        int currentSyllableIndex = lyricEventsIndex - 1;
        string runningStringPre = "";

        if (isCurrentlyPlacingLyric && currentSyllableIndex > 0)
            currentSyllableIndex -= 1;

        for (int i = 0; i < currentSyllableIndex; i++)
        {
            runningStringPre += displaySyllables.ElementAt(i);
        }
        runningStringPre = AddColorTag(runningStringPre, unfocusedColorString);

        string runningStringCurrent = "";
        if (isCurrentlyPlacingLyric)
        {
            runningStringCurrent = AddColorTag(displaySyllables.ElementAt(currentSyllableIndex), selectionColorString);
            currentSyllableIndex += 1;
        }

        string runningStringPost = "";
        for (int i = currentSyllableIndex; i < displaySyllables.Count; i++)
        {
            runningStringPost += displaySyllables.ElementAt(i);
        }
        runningStringPost = AddColorTag(runningStringPost, defaultColorString);

        string formattedPhrase = runningStringPre + runningStringCurrent + runningStringPost;

        phraseText.text = formattedPhrase;
    }
}
