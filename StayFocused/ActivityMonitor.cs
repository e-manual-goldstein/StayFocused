using Newtonsoft.Json;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using StayFocused.Api;
using StayFocused.Activities;
using System.Collections.Generic;
using StayFocused.Activities.Handlers;

namespace StayFocused
{
    public class ActivityMonitor : IActivityMonitor
    {
        //static BasicActivityHandler _basicActivityHandler = new BasicActivityHandler();
        //static InActivity _inactive = new InActivity();
        SFDbContext _sFDbContext;
        TaskRunner _activityTask;
        //TaskRunner _persistenceTask;
        bool _stationLocked;

        private static ConcurrentDictionary<string, Activity> _activities = new();
        // Dictionary to store activities and their scores
        private Dictionary<string, IActivityHandler> _handlers = new Dictionary<string, IActivityHandler>();

        public ActivityMonitor(SFDbContext sFDbContext) 
        {
            _sFDbContext = sFDbContext;
            _activityTask = new TaskRunner(StayFocused, Constants.MonitoringIntervalMilliseconds);
            //_persistenceTask = new TaskRunner(SaveActivitiesToFile, Constants.PersistenceIntervalMilliseconds);
        }

        public void AddCustomHandler(string processName, IActivityHandler activityHandler)
        {
            if (_handlers.ContainsKey(processName))
            {
                throw new NotImplementedException();
            }
            _handlers[processName] = activityHandler;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Lock();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Unlock();
            }
        }

        public void Begin()
        {
            //InitialiseActivities(SaveFilePath);
            ActivateSessionSwitchHandler();

            //_persistenceTask.Begin();
            _activityTask.Begin();
        }

        private void ActivateSessionSwitchHandler()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;            
        }

        #region Stay Focused

        private void StayFocused()
        {
            
            var activity = GetActivity();

            _sFDbContext.Add(activity.CreateNewEntry());
            _sFDbContext.SaveChanges();

            activity.IncrementActivityScore();

            Console.WriteLine($"{activity.ProcessName} - Score: {activity.ActivityScore}");
            
        }

        private Activity GetActivity()
        {
            var hWnd = WinApi.GetForegroundWindow(); 
            var activeProcess = GetWindowProcessName(hWnd);
            var windowTitle = WinApi.GetWindowTitle(hWnd);
            if (_handlers.ContainsKey(activeProcess))
            {

            }
            return _activities.GetOrAdd(ActivityName(activeProcess, windowTitle), (key) => new Activity()
            {
                ProcessName = activeProcess,
                WindowTitle = windowTitle,
            });
        }

        private string ActivityName(string processName, string windowTitle)
        {
            return $"{processName};{windowTitle}";
        }

        public static string GetActiveWindowTitleAndProcessName()
        {
            IntPtr hWnd = WinApi.GetForegroundWindow();
            
            return $"Active Window Title: {WinApi.GetWindowTitle(hWnd)} Process Name: {GetWindowProcessName(hWnd)}";
        }

        private static string GetWindowProcessName(IntPtr hWnd)
        {
            uint processId;
            WinApi.GetWindowThreadProcessId(hWnd, out processId);

            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }


        #endregion

        internal void Lock()
        {
            _stationLocked = true;
        }

        internal void Unlock()
        {
            _stationLocked = false;
        }
    }
}
