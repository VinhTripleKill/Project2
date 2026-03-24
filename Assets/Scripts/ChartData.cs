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
<<<<<<< Updated upstream
    public float endTime; // HOLDgcvyhgsvgvchg
=======
    public float endTime; // hold
>>>>>>> Stashed changes
    

    public SlideNode[] slideNodes; // slide
}

[Serializable]
public class ChartData
{
    public NoteData[] notes;
}