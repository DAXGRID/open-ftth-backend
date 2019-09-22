FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["OpenFTTH.GraphQLService/OpenFTTH.GraphQLService.csproj", "OpenFTTH.GraphQLService/"]
RUN dotnet restore "OpenFTTH.GraphQLService/OpenFTTH.GraphQLService.csproj"
COPY . .
WORKDIR "/src/OpenFTTH.GraphQLService"
RUN dotnet build "OpenFTTH.GraphQLService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "OpenFTTH.GraphQLService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OpenFTTH.GraphQLService.dll"]