using System.Activities;
using UiPath.Robot.Activities.Api;

namespace Yash.Frameworks.Activities.ViewModels.Helpers
{
    public static class ActivityContextExtensions
    {
        public static IExecutorRuntime GetExecutorRuntime(this ActivityContext context) => context.GetExtension<IExecutorRuntime>();
    }
}
