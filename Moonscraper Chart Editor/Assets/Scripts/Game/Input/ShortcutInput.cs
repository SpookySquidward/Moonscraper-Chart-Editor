﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSE;
using MSE.Input;

public enum Shortcut
{
    ActionHistoryRedo,
    ActionHistoryUndo,

    AddSongObject,

    BpmIncrease,
    BpmDecrease,

    ChordSelect,

    ClipboardCopy,
    ClipboardCut,
    ClipboardPaste,

    Delete, 

    FileLoad,
    FileNew,
    FileSave,
    FileSaveAs,

    MoveStepPositive,
    MoveStepNegative,
    MoveMeasurePositive,
    MoveMeasureNegative,

    NoteSetNatural,
    NoteSetStrum,
    NoteSetHopo,
    NoteSetTap,
    
    PlayPause,

    SelectAll,
    SelectAllSection,
    StepDecrease,
    StepIncrease,

    SectionJumpPositive,
    SectionJumpNegative,
    SectionJumpMouseScroll,

    ToggleBpmAnchor,
    ToggleClap,
    ToggleExtendedSustains,
    ToggleMetronome,
    ToggleMouseMode,
    ToggleNoteForced,
    ToggleNoteTap,
    ToggleViewMode, 
    
    ToolNoteBurst,
    ToolNoteHold,
    ToolSelectCursor,
    ToolSelectEraser,
    ToolSelectNote,
    ToolSelectStarpower,
    ToolSelectBpm,
    ToolSelectTimeSignature,
    ToolSelectSection,
    ToolSelectEvent,

    ToolNoteLane1,
    ToolNoteLane2,
    ToolNoteLane3,
    ToolNoteLane4,
    ToolNoteLane5,
    ToolNoteLane6,
    ToolNoteLaneOpen,

    CloseMenu,
}

public static class ShortcutInput
{
    public static class Category
    {
        // static int to make int conversion way easier. The lack of implicit enum->int conversion is annoying as hell.
        public enum CategoryType
        {
            Global,
            KeyboardMode,
            ToolNote,
        }

        public static InteractionMatrix interactionMatrix = new InteractionMatrix(3);

        static Category()
        {
            interactionMatrix.SetInteractableAll((int)CategoryType.Global);

            interactionMatrix.SetInteractable((int)CategoryType.KeyboardMode, (int)CategoryType.KeyboardMode);

            interactionMatrix.SetInteractable((int)CategoryType.ToolNote, (int)CategoryType.ToolNote);
        }
    }

    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const int kCategoryDefault = (int)Category.CategoryType.Global;

    static readonly InputAction.Properties kDefaultProperties = new InputAction.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    public class ShortcutActionContainer : IEnumerable<InputAction>
    {
        [SerializeField]
        List<InputAction.SaveData> saveData = new List<InputAction.SaveData>();    // Safer save data format, to handle cases where the Shortcut enum list may get updated or values are shifted around
        EnumLookupTable<Shortcut, InputAction> actionConfigCleanLookup;

        public ShortcutActionContainer()
        {
            InputManager inputManager = InputManager.Instance;

            actionConfigCleanLookup = new EnumLookupTable<Shortcut, InputAction>();

            for (int i = 0; i < actionConfigCleanLookup.Count; ++i)
            {
                Shortcut scEnum = (Shortcut)i;
                InputAction.Properties properties;
                if (!inputManager.inputPropertiesConfig.TryGetPropertiesConfig(scEnum, out properties))
                {
                    properties = kDefaultProperties;
                }

                if (string.IsNullOrEmpty(properties.displayName))
                {
                    properties.displayName = scEnum.ToString();
                }

                actionConfigCleanLookup[scEnum] = new InputAction(properties);
            }
        }

        public void Insert(Shortcut key, InputAction entry)
        {
            actionConfigCleanLookup[key] = entry;
        }

        public InputAction GetActionConfig(Shortcut key)
        {
            return actionConfigCleanLookup[key];
        }

        public void LoadFromSaveData(ShortcutActionContainer that)
        {
            saveData = that.saveData;
            foreach (var data in saveData)
            {
                Shortcut enumVal;
                if (System.Enum.TryParse(data.action, out enumVal))
                {
                    actionConfigCleanLookup[enumVal].LoadFrom(data);
                    // Add more maps as needed
                }
                else
                {
                    Debug.LogError("Unable to parse " + data.action + " as an input action");
                }
            }
        }

        public void UpdateSaveData()
        {
            saveData.Clear();
            for (int i = 0; i < actionConfigCleanLookup.Count; ++i)
            {
                Shortcut sc = (Shortcut)i;

                if (!actionConfigCleanLookup[sc].properties.rebindable)
                    continue;

                var newItem = new InputAction.SaveData();
                actionConfigCleanLookup[sc].SaveTo(sc.ToString(), newItem);

                saveData.Add(newItem);
            }
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            foreach (var val in actionConfigCleanLookup)
            {
                yield return val;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    static ShortcutActionContainer primaryInputs { get { return GameSettings.controls; } } 

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static bool modifierInput { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }
    public static bool secondaryInput { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
    public static bool alternativeInput { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }

    public static bool GetInputDown(Shortcut key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputDown(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInputUp(Shortcut key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInput(Shortcut key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInput(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetGroupInputDown(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (GetInputDown(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInputUp(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (GetInputUp(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInput(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (GetInput(key))
                return true;
        }

        return false;
    }
}
