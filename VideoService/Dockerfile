FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "VideoService.csproj"
RUN dotnet build "VideoService.csproj" -c Debug -o /app/build


FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY --from=build /app/build .

EXPOSE 8080

ENTRYPOINT ["dotnet", "VideoService.dll"]