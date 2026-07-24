using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Filo.Common.Telemetry;

public static class ApplicationTelemetry
{
    public const string ServiceName = "FiloApi";

    // ActivitySource is used for Tracing (Span / End-to-end request flow tracking)
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    // Meter is used for Metrics (Counters, Gauges, Histograms)
    public static readonly Meter Meter = new(ServiceName);

    // Custom Counter Metrics
    public static readonly Counter<long> VehiclesCreatedCounter = Meter.CreateCounter<long>(
        "filo.vehicles.created", 
        "count", 
        "The number of vehicles created");

    public static readonly Counter<long> PersonsCreatedCounter = Meter.CreateCounter<long>(
        "filo.persons.created", 
        "count", 
        "The number of persons created");

    public static readonly Counter<long> OutboxMessagesProcessedCounter = Meter.CreateCounter<long>(
        "filo.outbox.processed", 
        "count", 
        "The number of outbox messages successfully processed");

    public static readonly Counter<long> OutboxMessagesFailedCounter = Meter.CreateCounter<long>(
        "filo.outbox.failed", 
        "count", 
        "The number of outbox messages that failed processing");
}
