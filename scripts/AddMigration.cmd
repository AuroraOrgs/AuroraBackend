cd ../src
dotnet ef migrations add %1 -p Aurora.Infrastructure/Aurora.Infrastructure.csproj -s Aurora.Presentation/Aurora.Presentation.csproj -c SearchContext
cd ../scripts