using System.Collections.Generic;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class LyricEditor2PhraseController : UnityEngine.MonoBehaviour
{
    class LyricItem
    {
        public LyricEditorItemInterface lyricItemInterface;
        public Event lyricItemEvent;
        public string text = "";

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
        }
    }

    List<LyricItem> syllables = new List<LyricItem>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
