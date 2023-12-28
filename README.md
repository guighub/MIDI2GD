# MIDI2GD
A sequenced MIDI importer for Geometry Dash
## How to use
- Create a new level at 4x speed with music disabled by an "Edit Song" trigger. 4x speed (the one that looks like this: >>>>) is the speed I chose for the music because it allows for better note timing, but I might add an option for other speeds eventually.
- Open MIDI2GD.exe found in the releases tab
- Type the name of the level that you want to import into
- Input the path to the MIDI file on your computer
- Close MIDI2GD when it says "Done!"
- Launch Geometry Dash
- Play your level!
## InstrumentMappings.txt
The instrument mappings file can be very useful for changing what SFX to use for each MIDI channel, each line (excluding ones with comments and blank lines) represents a MIDI channel, the number assigned to that line is the ID of the sound effect that will be used.\
You can find downloaded sound effects at this path: `C:/Users/USERNAME/AppData/Local/GeometryDash/`\
(the sound effects are formatted as `s_____.ogg`, but include only the number in the mappings file)\
*Note: the default SFX used if none are specified for a channel is* `14219`
## Thanks to
Folleach for creating [GeometryDashAPI](https://github.com/Folleach/GeometryDashAPI)\
melanchall for creating [DryWetMIDI](https://github.com/melanchall/drywetmidi)\
RobTop for not delaying 2.2 any further
