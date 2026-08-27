using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Chord
{
    public int[] pressIndices;
    public int barIndex;

    public Chord(int numStrings)
    {
        pressIndices = new int[numStrings];
        barIndex = 0;
    }

    public Chord(int[] pressIndices, int barIndex = 0)
    {
        this.pressIndices = pressIndices;
        this.barIndex = barIndex;
    }

    public int[] Notes
    {
        get
        {
            int[] notes = new int[pressIndices.Length];
            for (int i = 0; i < pressIndices.Length; i++)
            {
                notes[i] = Mathf.Max(barIndex, pressIndices[i]);
            }
            return notes;
        }
    }
}

public class GuitarController : MonoBehaviour
{
    public GameObject fretPrefab;
    public GameObject[] strings;
    public float[] frequencies;
    public KeyCode[] pluckKeys;
    [Range(0, 1)]
    public float[] maxVolumes;
    public float minVolume = 0.1f;
    public float speedVolumeFactor = 0.04f;
    public float stringLength = 10f;
    public float stringWidth = 0.05f;
    public float stringSpace = 0.3f;
    public int numFrets = 18;
    [Range(0, 1)]
    public float pluckPosition = 0.28f;

    public TextMeshProUGUI chordText;
    public GameObject fingerMarkPrefab;
    public GameObject muteMarkPrefab;
    public GameObject barMarkPrefab;
    List<GameObject> chordMarks = new List<GameObject>();

    [SerializeField]
    Chord[] chords;
    int currentChordIndex = 0;
    int prevStringIndex = 0;
    float prevMouseY = 0;

    public Chord CurrentChord => chords[currentChordIndex];

    // Start is called before the first frame update
    void Start()
    {
        // Initialize 9 empty chords for 1~9 keys
        chords = new Chord[9];
        for (int i = 0; i < 9; i++)
        {
            chords[i] = new Chord(strings.Length);
        }

        for (int i = 0; i < numFrets; i++)
        {
            Vector3 position = new Vector3(NotePosition(i), 0, 0);
            Instantiate(fretPrefab, position, Quaternion.identity, transform);
        }

        // Bridge
        Instantiate(fretPrefab, new Vector3(stringLength / 2, 0, 0), Quaternion.identity, transform);

        UpdateChord();
    }

    void Awake()
    {
        for (int i = 0; i < strings.Length; i++)
        {
            strings[i].transform.localPosition = new Vector3(0, StringY(i), 0);

            // Set visualizer parameters before Start() to make it work
            var visualizer = strings[i].GetComponent<StringVisualizer>();
            visualizer.length = stringLength;
            visualizer.width = stringWidth;

            strings[i].GetComponent<LineRenderer>().material.color = Color.white;
            strings[i].GetComponent<StringSynth>().Frequency = frequencies[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < chords.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)(i + 49)))
            {
                currentChordIndex = i;
                chordText.text = "Current chord: " + (i + 1).ToString();
                UpdateChord();
                break;
            }
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for (int i = 0; i < strings.Length; i++)
        {
            if (Input.GetKeyDown(pluckKeys[i]))
            {
                float relativeLength = Mathf.Pow(2f, -CurrentChord.Notes[i] / 12f);
                strings[i].GetComponent<StringSynth>().pluckPosition = pluckPosition / relativeLength;
                //strings[i].GetComponent<StringSynth>().volume = maxVolumes[i];
                strings[i].GetComponent<StringSynth>().Trigger();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            HandleDrag(mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CurrentChord.barIndex = Mathf.Clamp(CurrentChord.barIndex - 1, 0, numFrets - 1);
            UpdateChord();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CurrentChord.barIndex = Mathf.Clamp(CurrentChord.barIndex + 1, 0, numFrets - 1);
            UpdateChord();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            int stringIndex = MouseToStringIndex(mousePosition, false);
            if (stringIndex >= 0 && stringIndex < strings.Length)
            {
                if (CurrentChord.pressIndices[stringIndex] == -1)
                {
                    CurrentChord.pressIndices[stringIndex] = 0;
                }
                else
                {
                    CurrentChord.pressIndices[stringIndex] = -1;
                }
                UpdateChord();
            }
        }
    }

    void UpdateChord()
    {
        foreach (var mark in chordMarks)
        {
            Destroy(mark);
        }
        chordMarks.Clear();
        
        Vector3 position = new Vector3(NotePosition(CurrentChord.barIndex - 0.45f), 0, 0);
        chordMarks.Add(Instantiate(barMarkPrefab, position, Quaternion.identity, transform));

        for (int i = 0; i < strings.Length; i++)
        {
            if (CurrentChord.pressIndices[i] > 0)
            {
                position = new Vector3(NotePosition(CurrentChord.pressIndices[i] - 0.45f), StringY(i), 0);
                chordMarks.Add(Instantiate(fingerMarkPrefab, position, Quaternion.identity, transform));
            }
            if (CurrentChord.pressIndices[i] == -1)
            {
                position = new Vector3(NotePosition(0), StringY(i), 0);
                chordMarks.Add(Instantiate(muteMarkPrefab, position, Quaternion.identity, transform));
            }

            int note = CurrentChord.Notes[i];
            strings[i].GetComponent<StringSynth>().Frequency = frequencies[i] * Mathf.Pow(2f, note / 12f);
            strings[i].GetComponent<StringSynth>().isMuted = CurrentChord.pressIndices[i] == -1;

            position = new Vector3(NotePosition(CurrentChord.pressIndices[i]), StringY(i), 0);
            if (CurrentChord.pressIndices[i] < 0)
            {
                position = new Vector3(0, StringY(i), 0); // NotePosition is undefined if note == -1
            }
            strings[i].GetComponent<StringVisualizer>().stopPosition = position;
        }
    }

    bool IsInFingerboard(Vector3 mousePosition)
    {
        return mousePosition.x < NotePosition(numFrets - 1) && mousePosition.x > NotePosition(0)
            && mousePosition.y < stringSpace * strings.Length / 2
            && mousePosition.y > -stringSpace * strings.Length / 2;
    }

    void HandleClick(Vector3 mousePosition)
    {
        // Register press down
        prevStringIndex = MouseToStringIndex(mousePosition, true);
        prevMouseY = mousePosition.y;

        if (IsInFingerboard(mousePosition))
        {
            int stringIndex = MouseToStringIndex(mousePosition, false);
            int fretIndex = (int)(-Mathf.Log(0.5f - mousePosition.x / stringLength, 2) * 12) + 1;

            if (CurrentChord.pressIndices[stringIndex] == fretIndex)
            {
                CurrentChord.pressIndices[stringIndex] = 0;
            }
            else
            {
                CurrentChord.pressIndices[stringIndex] = fretIndex;
            }
            UpdateChord();
        }
    }

    void HandleDrag(Vector3 mousePosition)
    {
        int stringIndex = MouseToStringIndex(mousePosition, true);

        int startIndex = Mathf.Min(prevStringIndex, stringIndex);
        int endIndex = Mathf.Max(prevStringIndex, stringIndex);

        float relativeMouseX = (stringLength / 2 - mousePosition.x) / stringLength;
        //float mouseSpeed = Mathf.Abs(mousePosition.y - prevMouseY) / Time.deltaTime;

        for (int i = startIndex; i < endIndex; i++)
        {
            if (i >= 0 && i < strings.Length)
            {
                float relativeStringLength = Mathf.Pow(2f, -CurrentChord.Notes[i] / 12f);
                float pluckPosition = relativeMouseX / relativeStringLength;
                if (pluckPosition < 0 || pluckPosition > 1) continue;
                strings[i].GetComponent<StringSynth>().pluckPosition = pluckPosition;

                //float volume = mouseSpeed * speedVolumeFactor * maxVolumes[i];
                //volume = Mathf.Clamp(volume, minVolume, maxVolumes[i]) * Random.Range(0.9f, 1.1f);
                //strings[i].GetComponent<StringSynth>().volume = volume;

                strings[i].GetComponent<StringSynth>().Trigger();
            }
        }

        prevStringIndex = MouseToStringIndex(mousePosition, true);
        prevMouseY = mousePosition.y;
    }

    int MouseToStringIndex(Vector3 mousePosition, bool inBetween)
    {
        float stringIndex = mousePosition.y / stringSpace + strings.Length / 2;
        if (inBetween)
        {
            return Mathf.RoundToInt(stringIndex);
        }
        else
        {
            return Mathf.FloorToInt(stringIndex);
        }
    }

    public float NotePosition(float note)
    {
        float relativePosition = 1f - Mathf.Pow(2f, -note / 12f);
        return relativePosition * stringLength - stringLength / 2;
    }

    public float StringY(int index)
    {
        return index * stringSpace - stringSpace * (strings.Length - 1) / 2;
    }
}
