using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util
{
    internal static class TaskTimeoutExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan timeoutDelay, string message)
        {
            var obj1 = (object) task;
            var taskArray = new[]
            {
                task,
                Task.Delay(timeoutDelay)
            };
            var obj = obj1;
            if (obj != await Task.WhenAny(taskArray))
            {
                throw new TimeoutException(message);
            }

            task.Wait();
        }

        public static async Task<T> WithTimeout<T>(
            this Task<T> task,
            TimeSpan timeoutDelay,
            string message)
        {
            var obj1 = task;
            var taskArray = new[]
            {
                task,
                Task.Delay(timeoutDelay)
            };
            var obj = (object)obj1;
            if (obj == await Task.WhenAny(taskArray)) return task.Result;
            throw new TimeoutException(message);
        }
    }
}