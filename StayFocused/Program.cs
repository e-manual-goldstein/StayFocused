using System;
using System.Threading.Tasks;

namespace StayFocused
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the continuous action loop
            Task.Run(async () =>
            {
                while (true)
                {
                    // Perform the timed action (in this case, "Stay Focused" function)
                    StayFocused();

                    // Wait for 5 seconds
                    await Task.Delay(5000); // 5000 milliseconds = 5 seconds
                }
            });

            // Your other asynchronous tasks can be done here or called as methods
            // For example, you can call your asynchronous methods like:
            // Task.Run(YourOtherAsyncTask);
            // Task.Run(AnotherAsyncTask);

            // Keep the main thread alive
            Console.WriteLine("StayFocused is running. Press any key to exit...");
            Console.ReadKey();
        }

        static void StayFocused()
        {
            Console.WriteLine("Stay Focused!");
        }

        // Your other asynchronous methods can be defined here
        // For example:
        // static async Task YourOtherAsyncTask()
        // {
        //     // Your asynchronous code here
        // }

        // static async Task AnotherAsyncTask()
        // {
        //     // Your asynchronous code here
        // }
    }
}
