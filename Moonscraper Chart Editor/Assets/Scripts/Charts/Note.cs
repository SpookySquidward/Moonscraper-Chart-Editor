﻿using UnityEngine;
using System;
using System.Collections.Generic;

public class Note : ChartObject 
{
    private readonly ID _classID = ID.Note;

    public override int classID { get { return (int)_classID; } }

    public uint sustain_length;
    public Fret_Type fret_type;

    public Flags flags;

    public Note previous;
    public Note next;

    NoteController _controller = null;
    new public NoteController controller {
        get { return _controller; }
        set { _controller = value; base.controller = value; }
    }

    public Note(Song song, Chart chart, uint _position, 
                Fret_Type _fret_type, 
                uint _sustain = 0, 
                Flags _flags = Flags.NONE) : base(song, chart, _position)
    {
        sustain_length = _sustain;
        flags = _flags;
        fret_type = _fret_type;

        previous = null;
        next = null;
    }

    public Note (Note note) : base(note.song, note.chart, note.position)
    {
        position = note.position;
        sustain_length = note.sustain_length;
        flags = note.flags;
        fret_type = note.fret_type;
    }

    public enum Fret_Type
    {
        // Assign to the sprite array position
        GREEN = 0, RED = 1, YELLOW = 2, BLUE = 3, ORANGE = 4, OPEN = 5
    }

    public enum Note_Type
    {
        STRUM, HOPO, TAP
    }

    public enum Special_Type
    {
        NONE, STAR_POW, BATTLE
    }

    [Flags]
    public enum Flags
    {
        NONE = 0,
        FORCED = 1,
        TAP = 2
    }

    public bool forced
    {
        get
        {
            return (flags & Flags.FORCED) == Flags.FORCED;
        }
        set
        {
            if (value)
                flags = flags | Flags.FORCED;
            else
                flags = flags & ~Flags.FORCED;
        }
    }

    public override string GetSaveString()
    {
        int fretNumber = (int)fret_type;

        if (fret_type == Fret_Type.OPEN)
            fretNumber = 7;

        return Globals.TABSPACE + position + " = N " + fretNumber + " " + sustain_length + Globals.LINE_ENDING;          // 48 = N 2 0
    }

    public string GetFlagsSaveString()
    {
        string saveString = string.Empty;

        if ((flags & Flags.FORCED) == Flags.FORCED)
            saveString += Globals.TABSPACE + position + " = N 5 0 " + Globals.LINE_ENDING;

        if ((flags & Flags.TAP) == Flags.TAP)
            saveString += Globals.TABSPACE + position + " = N 6 0 " + Globals.LINE_ENDING;

        return saveString;
    }
    
    protected override bool Equals(SongObject b)
    {
        if (b.GetType() == typeof(Note))
        {
            Note realB = b as Note;
            if (position == realB.position && fret_type == realB.fret_type)
                return true;
            else
                return false;
        }
        else
            return base.Equals(b);
    }

    protected override bool LessThan(SongObject b)
    {
        if (b.GetType() == typeof(Note))
        {
            Note realB = b as Note;
            if (position < b.position)
                return true;
            else if (position == b.position)
            {
                if (fret_type < realB.fret_type)
                    return true;
            }

            return false;
        }
        else
            return base.LessThan(b);
    }
    
    public static void groupAddFlags (Note[] notes, Flags flag)
    {
        for (int i = 0; i < notes.Length; ++i)
        {
            notes[i].flags = notes[i].flags | flag;
        }
    }

    public bool IsChord
    {
        get
        {
            if (previous != null && previous.position == position)
                return true;
            else if (next != null && next.position == position)
                return true;
            else
                return false;
        }
    }

    bool IsHopoUnforced
    {
        get
        {
            bool HOPO = false;

            if (!IsChord && previous != null)
            {
                // Need to consider whether the previous note was a chord, and if they are the same type of note
                if (previous.IsChord || (!previous.IsChord && fret_type != previous.fret_type))
                {
                    // Check distance from previous note 
                    int HOPODistance = (int)(65 * song.resolution / Globals.STANDARD_BEAT_RESOLUTION);

                    if (position - previous.position <= HOPODistance)
                        HOPO = true;
                }
            }

            return HOPO;
        }
    }

    public bool IsHopo
    {
        get
        {
            bool HOPO = IsHopoUnforced;

            // Check if forced
            if (forced)
                HOPO = !HOPO;

            return HOPO;
        }
    }

    public Note[] GetChord()
    {
        List<Note> chord = new List<Note>();
        chord.Add(this);

        Note previous = this.previous;
        while (previous != null && previous.position == this.position)
        {
            chord.Add(previous);
            previous = previous.previous;
        }

        Note next = this.next;
        while (next != null && next.position == this.position)
        {
            chord.Add(next);
            next = next.next;
        }

        return chord.ToArray();
    }

    public void applyFlagsToChord()
    {
        Note[] chordNotes = GetChord();

        foreach (Note chordNote in chordNotes)
        {
            chordNote.flags = flags;
        }
    }

    public void SetNoteType(Note_Type noteType)
    {
        flags &= ~Flags.TAP;
        switch (noteType)
        {
            case (Note_Type.STRUM):
                if (IsChord)
                    flags &= ~Flags.FORCED;
                else
                {
                    if (IsHopoUnforced)
                        flags |= Flags.FORCED;
                    else
                        flags &= ~Flags.FORCED;
                }

                break;

            case (Note_Type.HOPO):
                if (IsChord)
                    flags |= Flags.FORCED;
                else
                {
                    if (!IsHopoUnforced)
                        flags |= Flags.FORCED;
                    else
                        flags &= ~Flags.FORCED;
                }

                break;

            case (Note_Type.TAP):
                flags |= Flags.TAP;
                break;

            default:
                break;
        }

        applyFlagsToChord();

        ChartEditor.editOccurred = true;
    }
}
