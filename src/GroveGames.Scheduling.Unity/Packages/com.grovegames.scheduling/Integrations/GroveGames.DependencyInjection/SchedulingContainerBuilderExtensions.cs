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

        public static IContainerBuilder AddConcurrentScheduler(this IContainerBuilder builder)
        {
            builder.AddSingleton<ConcurrentScheduler>();
            builder.AddSingleton<IScheduler>(resolver => resolver.Resolve<ConcurrentScheduler>());
            return builder.AddSingleton<ConcurrentSchedulerUpdater>();
        }
    }
}
