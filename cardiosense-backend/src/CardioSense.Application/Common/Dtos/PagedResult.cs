// =============================================================================
// File           : PagedResult.cs
// Layer          : Application / Common/Dtos
// Purpose        : Generic page of items (PaginatedResponse<T> in the frontend contract).
// Logic          : TotalCount is the number of items after filtering and before paging; TotalPages is derived
//                  from it.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire).
// =============================================================================
namespace CardioSense.Application.Common.Dtos;

/// <summary>A page of results.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
