cd ../src
dotnet ef database update -p Aurora.Infrastructure/Aurora.Infrastructure.csproj -s Aurora.Presentation/Aurora.Presentation.csproj -c SearchContext
cd ../scripts