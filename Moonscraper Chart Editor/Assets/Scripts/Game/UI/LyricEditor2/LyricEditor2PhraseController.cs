using System.Collections.Generic;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;
using System.Linq;

public class LyricEditor2PhraseController : UnityEngine.MonoBehaviour
{
    class LyricItem
    {
        public LyricEditorItemInterface lyricItemInterface;
        public Event lyricItemEvent;
        public string text = "";
        public bool hasBeenPlaced = false;

        public LyricItem(LyricEditorItemInterface lyricItemInterface, Event lyricEvent)
        {
            this.lyricItemInterface = lyricItemInterface;
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
    int lyricEventsIndex = 0;
    public const string c_lyricPrefix = "lyric ";
    public const string c_phraseStartKeyword = "phrase_start";
    public const string c_phraseEndKeyword = "phrase_end";
    public bool phraseStartPlaced = false;
    public bool phraseEndPlaced = false;
    public bool allSyllablesPlaced = false;

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
}
