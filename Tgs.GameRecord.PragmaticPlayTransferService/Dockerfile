#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.PragmaticPlayTransferService/Tgs.GameRecord.PragmaticPlayTransferService.csproj", "Tgs.GameRecord.PragmaticPlayTransferService/"]
COPY ["Tgs.GameRecord.PragmaticPlayTransferService.Core/Tgs.GameRecord.PragmaticPlayTransferService.Core.csproj", "Tgs.GameRecord.PragmaticPlayTransferService.Core/"]
RUN dotnet restore "Tgs.GameRecord.PragmaticPlayTransferService/Tgs.GameRecord.PragmaticPlayTransferService.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.PragmaticPlayTransferService"
RUN dotnet build "Tgs.GameRecord.PragmaticPlayTransferService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameRecord.PragmaticPlayTransferService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.PragmaticPlayTransferService.dll"]