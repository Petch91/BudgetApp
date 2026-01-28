# =========================
# BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Solution
COPY BudgetApp.sln .

# Projets (cache Docker)
COPY Front_BudgetApp/Front_BudgetApp/Front_BudgetApp.csproj Front_BudgetApp/Front_BudgetApp/
COPY Application/*.csproj Application/
COPY BudgetApp.Shared/*.csproj BudgetApp.Shared/
COPY Entities/*.csproj Entities/

RUN dotnet restore Front_BudgetApp/Front_BudgetApp/Front_BudgetApp.csproj

# Reste du code
COPY . .

RUN dotnet publish Front_BudgetApp/Front_BudgetApp/Front_BudgetApp.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# RUNTIME
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5201

EXPOSE 5201

ENTRYPOINT ["dotnet", "Front_BudgetApp.dll"]
