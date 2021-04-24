FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["01_UserInterface（前端界面）/FSS.GrpcService/FSS.GrpcService.csproj", "FSS.GrpcService/"]
RUN dotnet restore "01_UserInterface（前端界面）/FSS.GrpcService/FSS.GrpcService.csproj"
COPY . .
WORKDIR "/src/FSS.GrpcService"
RUN dotnet build "FSS.GrpcService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FSS.GrpcService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FSS.GrpcService.dll"]
