#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FileSystemWatcher/FileSystemWatcher.csproj", "FileSystemWatcher/"]
RUN dotnet restore "FileSystemWatcher/FileSystemWatcher.csproj"
COPY . .
WORKDIR "/src/FileSystemWatcher"
RUN dotnet build "FileSystemWatcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileSystemWatcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileSystemWatcher.dll"]