#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.PragmaticPlayTransferManualService/Tgs.GameRecord.PragmaticPlayTransferManualService.csproj", "Tgs.GameRecord.PragmaticPlayTransferManualService/"]
COPY ["Tgs.GameRecord.PragmaticPlayTransferService.Core/Tgs.GameRecord.PragmaticPlayTransferService.Core.csproj", "Tgs.GameRecord.PragmaticPlayTransferService.Core/"]
RUN dotnet restore "Tgs.GameRecord.PragmaticPlayTransferManualService/Tgs.GameRecord.PragmaticPlayTransferManualService.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.PragmaticPlayTransferManualService"
RUN dotnet build "Tgs.GameRecord.PragmaticPlayTransferManualService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameRecord.PragmaticPlayTransferManualService.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.PragmaticPlayTransferManualService.dll"]