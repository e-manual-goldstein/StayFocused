using System;

namespace StayFocused
{
    public class Program
    {
        static void Main(string[] args)
        {
            var taskRunner = new TaskRunner();

            // Begin the timed tasks
            taskRunner.Begin();

            // Your other asynchronous tasks can be done here or called as methods
            // For example, you can call your asynchronous methods like:
            // Task.Run(() => YourOtherAsyncTask(taskRunner));
            // Task.Run(() => AnotherAsyncTask(taskRunner));

            // Keep the main thread alive until the user presses any key to exit
            Console.WriteLine("StayFocused is running. Press any key to stop...");
            Console.ReadKey();

            // Stop the timed tasks when the user wants to exit
            taskRunner.End();
        }
    }
}
