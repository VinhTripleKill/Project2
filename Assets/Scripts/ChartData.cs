using System;

[Serializable]
public class SlideNode
{
    public int lane;
    public float time;
}

[Serializable]
public class NoteData
{
    public int lane;
    public float hitTime;
    public float endTime; // HOLDNOTE

    public SlideNode[] slideNodes; // slide
}

[Serializable]
public class ChartData
{
    public NoteData[] notes;
}