FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Aurora.Presentation/Aurora.Presentation.csproj", "Aurora.Presentation/"]
COPY ["Aurora.Infrastructure/Aurora.Infrastructure.csproj", "Aurora.Infrastructure/"]
COPY ["Aurora.Application/Aurora.Application.csproj", "Aurora.Application/"]
COPY ["Aurora.Shared/Aurora.Shared.csproj", "Aurora.Shared/"]
RUN dotnet restore "Aurora.Presentation/Aurora.Presentation.csproj"
COPY . .
WORKDIR "/src/Aurora.Presentation"
RUN dotnet build "Aurora.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aurora.Presentation.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aurora.Presentation.dll"]