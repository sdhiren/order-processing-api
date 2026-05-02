# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution + csproj files first for optimal layer caching
COPY OrderProcessing.slnx .
COPY src/OrderProcessing.API/OrderProcessing.API.csproj                         src/OrderProcessing.API/
COPY src/OrderProcessing.Application/OrderProcessing.Application.csproj         src/OrderProcessing.Application/
COPY src/OrderProcessing.Domain/OrderProcessing.Domain.csproj                   src/OrderProcessing.Domain/
COPY src/OrderProcessing.Infrastructure/OrderProcessing.Infrastructure.csproj   src/OrderProcessing.Infrastructure/

RUN dotnet restore src/OrderProcessing.API/OrderProcessing.API.csproj

# Copy the rest and publish
COPY src/ ./src/
RUN dotnet publish src/OrderProcessing.API/OrderProcessing.API.csproj -c Release --output /app/publish --no-restore

# ── Runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderProcessing.API.dll"]