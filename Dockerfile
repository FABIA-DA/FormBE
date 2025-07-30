FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG FRAMEWORK=net9.0
WORKDIR /src
COPY ["FormBE/FormBE.csproj", "FormBE/"]
COPY ["FormBE.Core/FormBE.Core.csproj", "FormBE.Core/"]
COPY ["FormBE.Persistence/FormBE.Persistence.csproj", "FormBE.Persistence/"]
COPY ["FormBE.Shared/FormBE.Shared.csproj", "FormBE.Shared/"]
RUN dotnet restore "FormBE/FormBE.csproj" -p:TargetFramework=$FRAMEWORK
COPY . .
WORKDIR "/src/FormBE"
RUN dotnet build "FormBE.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FormBE.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-x64 -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
ARG PORT=5200
WORKDIR /app
RUN mkdir Logs
EXPOSE $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FormBE.dll"]