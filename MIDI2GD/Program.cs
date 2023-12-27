using System.Reflection;
using System.Resources;
using GeometryDashAPI.Data;
using GeometryDashAPI.Levels;
using GeometryDashAPI.Levels.GameObjects.Triggers;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

// Default settings
string configName = @"InstrumentMappings.txt";
int defaultInst = 14219;
float stopOffset = -4f;

// Store paths
string levelName;
string midiPath;

int[] LoadInstrumentMappings() // Load SFX IDs from text file
{
    if (File.Exists(configName))
    {
        // Read existing mappings
        string[] fileData = File.ReadAllLines(configName);
        List<int> sfxIDs = new List<int>();

        foreach (string sfxID in fileData)
        {
            if (!sfxID.Contains('#') && sfxID.Length > 0)
            {
                sfxIDs.Add(int.Parse(sfxID));
            }
        }
        for (int i = 0; i < 16 - sfxIDs.Count; i++)
        {
            sfxIDs.Add(defaultInst);
        }

        return sfxIDs.ToArray();
    } else
    {
        // No mappings file located, so load the default mappings from resources
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames()
        .Single(str => str.EndsWith(configName));

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            string result = reader.ReadToEnd();
            StreamWriter newMappingsFile = File.CreateText(configName);
            newMappingsFile.Write(result);
            newMappingsFile.Close();
        }
        return LoadInstrumentMappings();
    }
}

MidiFile AskForMIDI() // Get MIDI file path from user
{
    midiPath = Console.ReadLine().Replace("\"", string.Empty);
    MidiFile midiFile = new MidiFile();
    if (File.Exists(midiPath))
    {
        midiFile = MidiFile.Read(midiPath);
    }
    else
    {
        Console.WriteLine("Path does not exist, try again");
        AskForMIDI();
    }
    return midiFile;
}
Level AskForLevel(LocalLevels local) // Get Level name from user
{
    levelName = Console.ReadLine();
    Level level = new Level();
    try
    {
        level = local.GetLevel(levelName, revision: 0).LoadLevel();
    } catch
    {
        Console.WriteLine("Error loading level (does it exist?)");
        AskForLevel(local);
    }
    return level;
}

void ImportToLevel(MidiFile midiFile, Level level, LocalLevels local) // Import MIDI notes as SFX objects in Level
{
    IEnumerable<Note> notes = midiFile.GetNotes();
    int[] sfxIDs = LoadInstrumentMappings();
    foreach (Note note in notes)
    {
        TempoMap tempoMap = midiFile.GetTempoMap();
        MetricTimeSpan metricTime = note.TimeAs<MetricTimeSpan>(tempoMap);
        MetricTimeSpan metricLength = note.LengthAs<MetricTimeSpan>(tempoMap);
        float volume = note.Velocity / 128f;
        int pitch = Math.Min(12, Math.Max(-12, (note.NoteNumber - 60)));
        int speed = -Math.Abs((note.NoteNumber - 60) - pitch);

        Console.WriteLine(String.Format("Pitch: {0} Volume: {1} Speed: {2} Time: {3} Channel: {4} SFX ID: {5}", pitch, volume, speed, metricTime, note.Channel, sfxIDs[note.Channel]));

        level.AddBlock(new SfxTrigger()
        {
            PositionX = ToSecondsValue(metricTime) * 9.333f * 60,
            PositionY = (note.NoteNumber - 60) * 60,
            Pitch = pitch,
            Speed = speed,
            SongId = sfxIDs[note.Channel],
            Volume = volume,
            Groups = new int[] { note.Channel + 1 }
        }); ;
        level.AddBlock(new EditSfxTrigger()
        {
            PositionX = ((ToSecondsValue(metricTime) + ToSecondsValue(metricLength)) * 9.333f * 60) + stopOffset,
            PositionY = (note.NoteNumber - 60) * 60, 
            Groups = new int[] { note.Channel + 1 }, 
            GroupId = note.Channel + 1, 
            Stop = true
        });
    }
}

float ToSecondsValue(MetricTimeSpan metricTime) // Convert metric time to seconds only value
{
    return (metricTime.Hours * 3600) + (metricTime.Minutes * 60) + metricTime.Seconds + (((float)metricTime.Milliseconds) / 1000);
}

// Do the stuff
Console.WriteLine("Loading local levels...");
LocalLevels local = LocalLevels.LoadFile();

Console.WriteLine("Name of level (must be in your created levels):");
Level level = AskForLevel(local);

Console.WriteLine("Path to MIDI file:");
MidiFile midiFile = AskForMIDI();

Console.WriteLine("Working...");
ImportToLevel(midiFile, level, local);

Console.WriteLine("Saving level...");
local.GetLevel(levelName, revision: 0).SaveLevel(level);
local.Save();

Console.WriteLine("Done!");
// Done doing the stuff
