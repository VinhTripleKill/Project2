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
    public float endTime; // hold note

=======
    public float endTime; // HOLD
    
>>>>>>> Stashed changes
    public SlideNode[] slideNodes; // slide
}

[Serializable]
public class ChartData
{
    public NoteData[] notes;
}