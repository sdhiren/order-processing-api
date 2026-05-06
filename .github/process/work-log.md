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

---

## [2026-05-06] — Architecture Tests

### Requirement
Add a dedicated architecture test project to enforce Clean Architecture structural rules (layer dependencies, namespace placement, naming conventions, technology constraints).

### Assumptions documented
- Uses `NetArchTest.Rules` as the architecture-testing library (idiomatic .NET choice)
- References all four source assemblies; no runtime/integration setup needed
- Tests cover the full layer dependency graph, naming conventions, namespace placement, and forbidden technology leakage

### TDD cycle
**Red → Green**: Architecture tests by nature are green-from-first-run when the codebase is already compliant; the value is in protecting regressions going forward
**Refactor**: N/A — pure constraint tests

### Tests written (24 total)

**LayerDependencyTests (7)**
- `Domain_Should_Not_DependOn_Application`
- `Domain_Should_Not_DependOn_Infrastructure`
- `Domain_Should_Not_DependOn_Api`
- `Application_Should_Not_DependOn_Infrastructure`
- `Application_Should_Not_DependOn_Api`
- `Infrastructure_Should_Not_DependOn_Api`
- `Controllers_Should_Reside_Only_In_Api_Layer`
- `Controllers_Should_Not_Reference_Repositories_Directly`

**NamingConventionTests (5)**
- `Interfaces_Should_Start_With_I`
- `Controllers_Should_End_With_Controller`
- `Service_Implementations_Should_End_With_Service`
- `Repository_Implementations_Should_End_With_Repository`
- `Exceptions_Should_End_With_Exception`
- `Validators_Should_End_With_Validator`

**LayerStructureTests (12)**
- `Domain_Entities_Should_Reside_In_Entities_Namespace`
- `Domain_Exceptions_Should_Reside_In_Exceptions_Namespace`
- `Domain_Should_Not_Reference_EntityFrameworkCore`
- `Domain_Should_Not_Reference_MicrosoftExtensionsDependencyInjection`
- `Application_Service_Implementations_Should_Reside_In_Services_Namespace`
- `Application_Interfaces_Should_Reside_In_Interfaces_Namespace`
- `Application_Should_Not_Reference_EntityFrameworkCore`
- `Infrastructure_Repositories_Should_Reside_In_Repositories_Namespace`
- `Api_Controllers_Should_Reside_In_Controllers_Namespace`
- `Api_Controllers_Should_Be_Decorated_With_ApiController_Attribute`

### Files changed / created
- `tests/OrderProcessing.ArchitectureTests/OrderProcessing.ArchitectureTests.csproj`
- `tests/OrderProcessing.ArchitectureTests/ArchitectureTestBase.cs`
- `tests/OrderProcessing.ArchitectureTests/LayerDependencyTests.cs`
- `tests/OrderProcessing.ArchitectureTests/NamingConventionTests.cs`
- `tests/OrderProcessing.ArchitectureTests/LayerStructureTests.cs`
- `OrderProcessing.slnx` — added new project to `/tests/` solution folder

### Result
24/24 architecture tests passing. Zero regressions on existing test suites.
