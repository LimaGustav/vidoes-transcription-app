FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "VideoProcessorWorker.csproj"
RUN dotnet build "VideoProcessorWorker.csproj" -c Debug -o /app/build


FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY --from=build /app/build .

ENTRYPOINT ["dotnet", "VideoProcessorWorker.dll"]