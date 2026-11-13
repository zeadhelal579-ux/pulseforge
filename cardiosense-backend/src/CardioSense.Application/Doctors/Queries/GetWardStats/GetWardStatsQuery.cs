// =============================================================================
// File           : GetWardStatsQuery.cs
// Layer          : Application / Doctors/Queries/GetWardStats
// Purpose        : Query for the doctor's ward statistics cards.
// Logic          : Serves GET /api/doctors/me/ward-stats; it has no parameters because the doctor is the
//                  authenticated caller.
// Implementation : Parameterless record implementing the Mediator request contract.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Doctors.Queries.GetWardStats;

/// <summary>Get the doctor's ward statistics.</summary>
public sealed record GetWardStatsQuery : IRequest<Result<WardStatsDto>>;
