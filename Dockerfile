FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Filo.Api/Filo.Api.csproj", "src/Filo.Api/"]
COPY ["src/Filo.Application/Filo.Application.csproj", "src/Filo.Application/"]
COPY ["src/Filo.Domain/Filo.Domain.csproj", "src/Filo.Domain/"]
COPY ["src/Filo.Common/Filo.Common.csproj", "src/Filo.Common/"]
COPY ["src/Filo.Infrastructure/Filo.Infrastructure.csproj", "src/Filo.Infrastructure/"]
RUN dotnet restore "src/Filo.Api/Filo.Api.csproj"
COPY . .
WORKDIR "/src/src/Filo.Api"
RUN dotnet build "Filo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Filo.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Filo.Api.dll"]
