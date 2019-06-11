FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY UnicornStore/UnicornStore.csproj UnicornStore/
RUN dotnet restore UnicornStore/UnicornStore.csproj
COPY . .
WORKDIR /src/UnicornStore
RUN dotnet build UnicornStore.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish UnicornStore.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "UnicornStore.dll"]
HEALTHCHECK --interval=30s --timeout=5s --retries=5 --start-period=30s CMD wget --quiet --tries=1 --spider http://localhost/health || exit 1