FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . /build
WORKDIR /build

RUN dotnet workload restore
RUN dotnet publish ./src/owoBot.App.Bot -c Release -o /artifacts -p:ContinuousIntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

COPY --from=build /artifacts /app

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV OTEL_SERVICE_NAME=owobot

EXPOSE 80

ENTRYPOINT ["dotnet", "owoBot.App.Bot.dll"]
