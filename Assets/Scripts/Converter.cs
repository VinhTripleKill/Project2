using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChartConverter : MonoBehaviour
{
#if UNITY_EDITOR

    [ContextMenu("Convert SM To JSON")]
    void ConvertSM()
    {
        string path = EditorUtility.OpenFilePanel("Select .sm file", "", "sm");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);

        float bpm = 120f;
        float offset = 0f;

        foreach (var line in lines)
        {
            if (line.StartsWith("#BPMS"))
            {
                var match = Regex.Match(line, @"=(\d+(\.\d+)?)");
                if (match.Success)
                    bpm = float.Parse(match.Groups[1].Value);
            }

            if (line.StartsWith("#OFFSET"))
            {
                var match = Regex.Match(line, @"-?\d+(\.\d+)?");
                if (match.Success)
                    offset = float.Parse(match.Value);
            }
        }

        List<List<string>> measures = new();
        List<string> current = new();
        bool reading = false;

        foreach (var line in lines)
        {
            string l = line.Trim();

            if (Regex.IsMatch(l, "^[0-3]{4}$"))
            {
                reading = true;
                current.Add(l);
                continue;
            }

            if (reading && l == ",")
            {
                measures.Add(new List<string>(current));
                current.Clear();
            }
        }

        float beatDuration = 60f / bpm;

        List<NoteData> notes = new();
        Dictionary<int, NoteData> holdStarts = new();

        for (int m = 0; m < measures.Count; m++)
        {
            var measure = measures[m];
            int lineCount = measure.Count;

            for (int i = 0; i < lineCount; i++)
            {
                string row = measure[i];

                float beat = m * 4f + i * (4f / lineCount);
                float time = beat * beatDuration - offset;

                for (int lane = 0; lane < 4; lane++)
                {
                    char c = row[lane];

                    if (c == '1')
                    {
                        notes.Add(new NoteData
                        {
                            lane = lane,
                            hitTime = time,
                            endTime = time
                        });
                    }

                    if (c == '2')
                    {
                        holdStarts[lane] = new NoteData
                        {
                            lane = lane,
                            hitTime = time
                        };
                    }

                    if (c == '3' && holdStarts.ContainsKey(lane))
                    {
                        var n = holdStarts[lane];
                        n.endTime = time;
                        notes.Add(n);
                        holdStarts.Remove(lane);
                    }
                }
            }
        }

        SaveJson(notes);
    }

    [ContextMenu("Convert OSU To JSON")]
    void ConvertOSU()
    {
        string path = EditorUtility.OpenFilePanel("Select .osu file", "", "osu");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        List<NoteData> notes = ParseOsu(lines);

        SaveJson(notes);
    }

    [ContextMenu("Convert OSZ To JSON")]
    void ConvertOSZ()
    {
        string path = EditorUtility.OpenFilePanel("Select .osz file", "", "osz");
        if (string.IsNullOrEmpty(path)) return;

        using ZipArchive archive = ZipFile.OpenRead(path);

        foreach (var entry in archive.Entries)
        {
            if (!entry.FullName.EndsWith(".osu"))
                continue;

            using StreamReader reader = new StreamReader(entry.Open());
            string text = reader.ReadToEnd();

            string[] lines = text.Split('\n');

            List<NoteData> notes = ParseOsu(lines);

            if (notes.Count > 0)
            {
                SaveJson(notes);
                return;
            }
        }

        Debug.LogError("No .osu file found inside OSZ");
    }

    List<NoteData> ParseOsu(string[] lines)
    {
        bool reading = false;
        List<NoteData> notes = new();

        foreach (var line in lines)
        {
            if (line.StartsWith("[HitObjects]"))
            {
                reading = true;
                continue;
            }

            if (!reading) continue;
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            int x = int.Parse(parts[0]);
            int timeMs = int.Parse(parts[2]);
            int type = int.Parse(parts[3]);

            float time = timeMs / 1000f;

            int lane = Mathf.Clamp(x * 4 / 512, 0, 3);

            bool isHold = (type & 128) != 0;

            if (!isHold)
            {
                notes.Add(new NoteData
                {
                    lane = lane,
                    hitTime = time,
                    endTime = time
                });
            }
            else
            {
                string[] holdData = parts[5].Split(':');
                float endTime = int.Parse(holdData[0]) / 1000f;

                notes.Add(new NoteData
                {
                    lane = lane,
                    hitTime = time,
                    endTime = endTime
                });
            }
        }

        return notes;
    }

    void SaveJson(List<NoteData> notes)
    {
        notes = notes.OrderBy(n => n.hitTime).ToList();

        ChartData chart = new ChartData();
        chart.notes = notes.ToArray();

        string json = JsonUtility.ToJson(chart, true);

        string savePath = EditorUtility.SaveFilePanel(
            "Save JSON",
            "",
            "chart",
            "json"
        );

        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllText(savePath, json);
            Debug.Log("Convert Done!");
        }
    }

#endif
}