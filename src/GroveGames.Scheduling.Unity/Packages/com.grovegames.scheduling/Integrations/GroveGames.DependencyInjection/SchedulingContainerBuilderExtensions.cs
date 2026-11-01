using GroveGames.DependencyInjection;

namespace GroveGames.Scheduling.Unity
{
    public static class SchedulingContainerBuilderExtensions
    {
        public static IContainerBuilder AddScheduler(this IContainerBuilder builder)
        {
            builder.AddSingleton<Scheduler>();
            builder.AddSingleton<IScheduler>(resolver => resolver.Resolve<Scheduler>());
            return builder.AddSingleton<SchedulerUpdater>();
        }
    }
}
