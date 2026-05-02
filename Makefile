.PHONY: help up down build logs ps \
        restore clean \
        test test-unit test-integration coverage \
        migrate db-only

# ──────────────────────────────────────────────────────────────────────────────
# Default target
# ──────────────────────────────────────────────────────────────────────────────
help:
	@echo ""
	@echo "Usage: make <target>"
	@echo ""
	@echo "  Docker / Stack"
	@echo "    up               Build images and start the API + PostgreSQL in Docker"
	@echo "    down             Stop and remove containers (data volume is preserved)"
	@echo "    down-volumes     Stop containers AND delete the postgres data volume"
	@echo "    build            Re-build the API Docker image"
	@echo "    logs             Tail logs for all services"
	@echo "    ps               Show running container status"
	@echo "    db-only          Start only the PostgreSQL container"
	@echo ""
	@echo "  Local development"
	@echo "    restore          Restore NuGet packages"
	@echo "    run              Run the API locally (requires a local/docker postgres)"
	@echo "    clean            Remove all bin/ and obj/ build artefacts"
	@echo ""
	@echo "  Database"
	@echo "    migrate          Apply EF Core migrations (against local postgres)"
	@echo ""
	@echo "  Tests"
	@echo "    test             Run all tests (unit + integration)"
	@echo "    test-unit        Run unit tests only"
	@echo "    test-integration Run integration tests only (requires Docker for Testcontainers)"
	@echo "    coverage         Run all tests and collect code coverage (Cobertura)"
	@echo ""

# ──────────────────────────────────────────────────────────────────────────────
# Docker / Stack
# ──────────────────────────────────────────────────────────────────────────────
up:
	docker-compose build
	docker-compose up -d

down:
	docker-compose down

down-volumes:
	docker-compose down -v

build:
	docker-compose build

logs:
	docker-compose logs -f

ps:
	docker-compose ps

db-only:
	docker-compose up -d db

# ──────────────────────────────────────────────────────────────────────────────
# Local development
# ──────────────────────────────────────────────────────────────────────────────
restore:
	dotnet restore

run:
	dotnet run --project src/OrderProcessing.API/OrderProcessing.API.csproj

clean:
	dotnet clean
	find . \( -name "bin" -o -name "obj" \) -type d -not -path "./.git/*" | xargs rm -rf

# ──────────────────────────────────────────────────────────────────────────────
# Database
# ──────────────────────────────────────────────────────────────────────────────
migrate:
	dotnet ef database update \
		--project src/OrderProcessing.Infrastructure \
		--startup-project src/OrderProcessing.API

# ──────────────────────────────────────────────────────────────────────────────
# Tests
# ──────────────────────────────────────────────────────────────────────────────
test:
	dotnet test --configuration Release --logger "console;verbosity=normal"

test-unit:
	dotnet test tests/OrderProcessing.UnitTests \
		--configuration Release \
		--logger "console;verbosity=normal"

test-integration:
	dotnet test tests/OrderProcessing.IntegrationTests \
		--configuration Release \
		--logger "console;verbosity=normal"

coverage:
	dotnet test --configuration Release \
		--collect:"XPlat Code Coverage" \
		--results-directory ./coverage \
		--logger "console;verbosity=normal"
	@echo ""
	@echo "Coverage reports written to ./coverage/"
