FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY src/Certus.Domain/Certus.Domain.csproj src/Certus.Domain/
COPY src/Certus.Application/Certus.Application.csproj src/Certus.Application/
COPY src/Certus.Infrastructure/Certus.Infrastructure.csproj src/Certus.Infrastructure/
COPY src/Certus.Dashboard/Certus.Dashboard.csproj src/Certus.Dashboard/
RUN dotnet restore src/Certus.Dashboard/Certus.Dashboard.csproj

# Copy everything else and build
COPY src/ src/
RUN dotnet publish src/Certus.Dashboard/Certus.Dashboard.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Certus.Dashboard.dll"]
