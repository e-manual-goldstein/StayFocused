namespace StayFocused
{
    public class Activity
    {
        public int ActivityScore { get; private set; }
        public string Description { get; }
        public Activity(string descr)
        {            
            Description = descr;
        }

        public void IncrementActivityScore()
        {
            ActivityScore++;
        }
    }
}
