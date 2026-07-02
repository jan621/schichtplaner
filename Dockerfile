FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish planer/planer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

# Railway (and most PaaS) inject the port to listen on via $PORT.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet planer.dll"]
