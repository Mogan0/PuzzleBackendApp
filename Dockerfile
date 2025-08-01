
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app


COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "PuzzleRealtimeApp.dll"]