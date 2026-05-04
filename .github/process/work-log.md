# Work Log

<!-- Append a new entry after every completed implementation phase -->

## [2026-05-04] — GET /api/orders: integer status → string status

### Requirement
Change `GET /api/orders?status=` to accept string status names (e.g. `PENDING`, `Processing`) instead of integer enum values.

### Assumptions documented
- Case-insensitive string matching (PENDING = pending = Pending)
- Integer values (e.g. `?status=0`) are explicitly rejected with 400
- Unknown strings (e.g. `?status=UNKNOWN`) return 400 with valid values listed in ProblemDetails

### TDD cycle
**Red**: Added `GetAllOrders_FilteredByIntegerStatus_ShouldReturn400` — confirmed failure (200 returned, expected 400)
**Green**: Changed `[FromQuery] OrderStatus? status` → `[FromQuery] string? status`; replaced `Enum.TryParse` (which accepted integers) with `Enum.GetNames` name-match to enforce string-only contract
**Refactor**: Updated XML summary with valid values and added `[ProducesResponseType<ProblemDetails>(400)]`

### Tests written
- `GetAllOrders_FilteredByUppercaseStatus_ShouldReturn200WithMatchingOrders`
- `GetAllOrders_FilteredByLowercaseStatus_ShouldReturn200WithMatchingOrders`
- `GetAllOrders_FilteredByMixedCaseStatus_ShouldReturn200WithMatchingOrders`
- `GetAllOrders_FilteredByUnknownStatus_ShouldReturn400`
- `GetAllOrders_FilteredByIntegerStatus_ShouldReturn400` ← the Red test

### Files changed
- `src/OrderProcessing.API/Controllers/OrdersController.cs`
- `tests/OrderProcessing.IntegrationTests/Controllers/OrdersControllerTests.cs`

### Result
19/19 integration tests passing. Zero regressions.
