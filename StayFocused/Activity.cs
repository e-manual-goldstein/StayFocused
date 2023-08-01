namespace StayFocused
{
    public class Activity
    {
        public int ActivityScore { get; private set; }
        public string Description { get; set; }
        

        public void IncrementActivityScore()
        {
            ActivityScore++;
        }
    }
}
