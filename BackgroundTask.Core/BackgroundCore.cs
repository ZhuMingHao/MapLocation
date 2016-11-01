using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BackgroundTask.Core
{
    public sealed class BackgroundCore : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
          
            taskInstance.GetDeferral().Complete();
           
        }
    }
}
