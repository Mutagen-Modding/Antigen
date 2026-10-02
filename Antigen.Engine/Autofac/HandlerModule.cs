using Autofac;
using Antigen.Reporting.Drops;
using Antigen.SDK.Drops;

namespace Antigen.Autofac;

class HandlerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Last registered runs first
        builder.RegisterType<PassToHandlerReportDropbox>().AsImplementedInterfaces();
        builder.RegisterDecorator<EditorIdEnricher, IReportDropbox>();
        builder.RegisterDecorator<MinimumSeverityFilter, IReportDropbox>();
        builder.RegisterDecorator<SeverityAdjuster, IReportDropbox>();
        builder.RegisterDecorator<DisallowedParametersChecker, IReportDropbox>();
        builder.RegisterDecorator<FilterBlacklistedReports, IReportDropbox>();
    }
}
