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

        public LyricItem(Event lyricEvent)
        {
            this.lyricItemEvent = lyricEvent;
        }

        public void SetTime(uint tick)
        {
            List<SongEditCommand> commands = new List<SongEditCommand>();

            if (lyricItemEvent != null)
            {
                commands.Add(new SongEditDelete(lyricItemEvent));
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
    List<string> displaySyllables;

    public const string c_lyricPrefix = "lyric ";
    public const string c_phraseStartKeyword = "phrase_start";
    public const string c_phraseEndKeyword = "phrase_end";
    public bool phraseStartPlaced = false;
    public bool phraseEndPlaced = false;
    public bool allSyllablesPlaced = false;

    [UnityEngine.SerializeField]
    Text phraseText;

    // Start is called before the first frame update
    void Start()
    {

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
    }

    // Set the phrase_start event tick
    public void SetPhraseStart(uint tick)
    {
        lyricEvents.ElementAt(0).SetTime(tick);
    }

    // Set the phrase_end event tick
    public void SetPhraseEnd(uint tick)
    {
        lyricEvents.ElementAt(lyricEvents.Count).SetTime(tick);
    }

    // Create LyricItem events for each passed syllable and the phrase start
    // and phrase end positions. Must be called before using PlaceNextLyric(),
    // SetPhraseStart(), or SetPhraseEnd()!
    public void InitializeSyllables(List<string> syllables)
    {
        lyricEvents.Clear();

        lyricEvents.Add(new LyricItem(new Event(c_phraseStartKeyword, 0)));
        foreach (string syllable in syllables)
        {
            lyricEvents.Add(new LyricItem(new Event(syllable, 0)));
        }
        lyricEvents.Add(new LyricItem(new Event(c_phraseEndKeyword, 0)));

        generateDisplaySyllables(syllables);
    }

    // Create a list of syllables with added spaces that can be easily displayed
    // to the phraseText component.
    void generateDisplaySyllables(List<string> syllables)
    {
        for (int i = 0; i < syllables.Count; i++)
        {
            string currentSyllable = syllables.ElementAt(i);

            if (!currentSyllable.EndsWith("-"))
                currentSyllable += " ";

            displaySyllables.Add(currentSyllable);
        }
    }
}
