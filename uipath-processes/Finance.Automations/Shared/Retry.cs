using System;
using UiPath.CodedWorkflows;

namespace Finance.Automations._00_Shared
{
    public class Retry : CodedWorkflow
    {
        [Workflow]
        public void Execute(int count = 3, int timeoutInt = 5, Action action = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action), "Action cannot be null");
                
            var attempt = 0;
            Exception lastException = null;
            
            while(attempt < count){
                try{
                    action.Invoke();
                    return; // Success - exit the retry loop
                }
                catch(Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    
                    Log($"Attempt {attempt} failed: {ex.Message}", LogLevel.Warn);
                    
                    // If this was the last attempt, don't wait
                    if(attempt >= count)
                        break;
                        
                    // Wait before next retry
                    if(timeoutInt > 0)
                    {
                        Log($"Waiting {timeoutInt} seconds before retry...", LogLevel.Info);
                        System.Threading.Thread.Sleep(timeoutInt * 1000);
                    }
                }
            }
            
            // If we get here, all retries failed
            throw new Exception($"Action failed after {count} attempts. Last error: {lastException?.Message}", lastException);
        }
    }
}