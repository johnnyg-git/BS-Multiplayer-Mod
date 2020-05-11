using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer_Mod
{
	public class ThreadManager
	{
		// Actions to execute on the main thread
		private static readonly List<Action> executeOnMainThread = new List<Action>();
		private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
		private static bool actionToExecuteOnMainThread = false;

		/// <summary>
		/// Execute an action on next UpdateMain
		/// </summary>
		/// <param name="_action">The action to execute</param>
		public static void ExecuteOnMainThread(Action _action)
		{
			// If action is empty then return
			if (_action == null) return;

			List<Action> obj = executeOnMainThread;
			lock (obj)
			{
				// Add action to list
				executeOnMainThread.Add(_action);
				// Show there is an action to execute
				actionToExecuteOnMainThread = true;
			}
		}

		/// <summary>
		/// Ran every frame
		/// Will execute actions
		/// </summary>
		public static void UpdateMain()
		{
			// If there is an action to execute
			if (actionToExecuteOnMainThread)
			{
				// Clean up from last time
				executeCopiedOnMainThread.Clear();
				List<Action> obj = executeOnMainThread;
				lock (obj)
				{
					// Add actions to copy
					executeCopiedOnMainThread.AddRange(executeOnMainThread);
					// Clear non copy actions
					executeOnMainThread.Clear();
					// Show there is no more actions to execute
					actionToExecuteOnMainThread = false;
				}
				// Execute all actions
				foreach (Action action in executeCopiedOnMainThread)
				{
					action();
				}
			}
		}
	}
}
