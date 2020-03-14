using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using MidiJack;

public class TrumpetInput : MonoBehaviour
{
    public SendTestMIDIManager midiManager;

    SendTest sendTest;

    public KeyCode index = KeyCode.J;
    public KeyCode middle = KeyCode.K;
    public KeyCode ring = KeyCode.L;
    public KeyCode valve = KeyCode.D;
    public KeyCode up8 = KeyCode.W;
    public KeyCode down8 = KeyCode.S;
    public KeyCode mouth = KeyCode.Space;

    private uint id;
    private int note;
    private int octave = 5;


    [Header("Debug")]
    public bool isPlayingNote = false;

    KeyCode[] keys = new KeyCode[4];
    bool[] prevPressed = new bool[4];
    bool[] pressed = new bool[4];

    Dictionary<string, int> fingerings = new Dictionary<string, int>();

    bool[] c = new bool[] { false, false, false, false };
    private void Start()
    {
        keys = new KeyCode[] { index, middle, ring, valve };

        fingerings.Add(GetNoteString(new bool[] { false,  false,  false,  false }), 0);    // C
        fingerings.Add(GetNoteString(new bool[] { true,   true,   true,   false }), 1);    // C#7Db
        fingerings.Add(GetNoteString(new bool[] { true,   false,  true,   false }), 2);    // D
        fingerings.Add(GetNoteString(new bool[] { false,  true,   true,   false }), 3);    // D#/Eb
        fingerings.Add(GetNoteString(new bool[] { true,   true,   false,  false }), 4);    // E
        fingerings.Add(GetNoteString(new bool[] { true,   false,  false,  false }), 5);    // F
        fingerings.Add(GetNoteString(new bool[] { false,  true,   false,  false }), 6);    // F#/Gb
        fingerings.Add(GetNoteString(new bool[] { false,  false,  false,  true }), 7);     // G
        fingerings.Add(GetNoteString(new bool[] { false,  true,   true,   true }), 8);     // G#/Ab
        fingerings.Add(GetNoteString(new bool[] { true,   true,   false,  true }), 9);     // A
        fingerings.Add(GetNoteString(new bool[] { true,   false,  false,  true }), 10);    // A#/Bb
        fingerings.Add(GetNoteString(new bool[] { false,  true,   false,  true }), 11);    // B
    }

    void Update()
    {
        prevPressed = pressed;
        for(int i = 0; i < keys.Length; i++)
        {
            pressed[i] = Input.GetKeyDown(keys[i]) || Input.GetKey(keys[i]);
        }

        ProcessInput();
    }

    void ProcessInput()
    {
        int prevNote = note;
        note = GetNote();
        if(Input.GetKeyDown(mouth) && note != -1)
        {
            SendMIDINote();
            isPlayingNote = true;
        }

        if(isPlayingNote && prevNote != note && note != -1)
        {
            SendMIDINoteOff(prevNote);
            SendMIDINote();
        }

        if(Input.GetKeyUp(mouth) || note == -1)
        {
            isPlayingNote = false;
            SendMIDINoteOff();
        }
    }

    int GetNote()
    {
        Debug.Log("Pressed: " + pressed[0] + ", " + pressed[1] + ", " + pressed[2] + ", " + pressed[3]);
        int result;
        if (fingerings.TryGetValue(GetNoteString(pressed), out result))
        {
            return (octave * 12) + result;
        }
        return -1;
    }

    string GetNoteString(bool[] _p)
    {
        string result = string.Empty;
        foreach(var p in _p)
        {
            result += p ? "1" : "0";
        }
        return result;
    }

    public void SendMIDINote()
    {
        id = midiManager.MidiOutDevices[0].Id;
        //note = int.Parse("3c", NumberStyles.HexNumber);
        MidiMaster.SendNoteOn(id, MidiJack.MidiChannel.Ch1, note, 0.8f);
    }

    void SendMIDINoteOff()
    {
        MidiMaster.SendNoteOff(id, MidiJack.MidiChannel.Ch1, note, 0.8f);
    }

    void SendMIDINoteOff(int _note)
    {
        MidiMaster.SendNoteOff(id, MidiJack.MidiChannel.Ch1, _note, 0.8f);
    }
}

public class BoolArrayEqualityComparer : EqualityComparer<bool[]>
{
    public override bool Equals(bool[] x, bool[] y)
    {
        if (x.Length != y.Length)
        {
            return false;
        }
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode(bool[] obj)
    {
        return obj.GetHashCode();
    }
}
