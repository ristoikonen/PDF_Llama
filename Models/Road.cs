using System;


namespace Agent_Ollama.Models;

public class Road
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Direction { get; set; }
    //public AustralianState State { get; set; }
    //public DateTime StartTimeMorning { get; set; }
    //public DateTime EndTimeMorning { get; set; }
    //public DateTime StartTimeEvening { get; set; }
    //public DateTime EndTimeEvening { get; set; }

    public override string ToString()
    {
        return Name + "  " + Description + " " + Direction;
            //+ " from " + 
            //StartTimeMorning.ToShortTimeString() + 
            //" to " + 
            //EndTimeMorning.ToShortTimeString() + 
            //" from " + 
            //StartTimeEvening.ToShortTimeString() + 
            //" to " + 
            //EndTimeEvening.ToShortTimeString();
    }
}

public enum AustralianState
{
    Act, NSW, NT, QLD, SA, TAS, VIC, WA
}
