# An interactive guitar simulator and synthesizer, made with Unity

<img src="readme_assets/screenshot.png"/>

It looks like a guitar and you can play it like a real guitar.

## Controls

- Hold down the mouse and drag through the strings to strum it.
- Press the keys at the top letter row (TYUIOP) to pluck the corresponding strings.
- Click on the fretboard to edit the current chord. Press 1-9 keys to switch chords. You can store at most 9 chords.
- Press the left/right arrow keys to move the capo/bar
- Press M while the cursor is on a string to mute the string

## How it works

The sound is not pre-recorded, it's generated on the fly in realtime, and the timbre changes continuously based on your plucking position.

The vibration of strings and the sound can be decomposed into sine waves with integer multiple frequencies, which is called **harmonic series** or overtone series. The amplitude of each frequency can be determined by the plucking position. The algorithm adds up every sine wave to get the waveform of the final sound. This method is called additive synthesis.

This project is inspired by this [amazing article](https://euphonics.org/5-3-synthesising-plucked-strings/), and the details of the method is described in it. I haven't implemented guitar body resonance yet, so the sound is not very realistic.

## Future plans
- Incorporate guitar body resonance
- Synthesize more instruments!
    - Wind instruments, e.g. flute or saxophone
    - Bowed string instruments, e.g. violin
    - Vibraphone
