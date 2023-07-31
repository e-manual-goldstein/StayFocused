namespace StayFocused
{
    public class Activity
    {
        public int ActivityScore { get; private set; }

        public Activity()
        {            
        }

        public void IncrementActivityScore()
        {
            ActivityScore++;
        }
    }
}
