::Go to root
cd ..
cd ..
cd /src
set USERNAME=yaroslavholota
set IMAGE=aurora
git pull
::Load current location
for /f %%A in ('cd') do set "pwd=%%A"
::TODO: Add auto-bump mechanism
::Get version
for /f %%B in ('type version') do set "version=%%B"
echo version: %version%
::Build container
cmd /c "cd scripts && cd Docker && build.cmd"
::Commit
git add -A
git commit -m "version %version%"
git tag -a "%version%" -m "version %version%"
::Tag
docker tag %USERNAME%/%IMAGE%:latest %USERNAME%/%IMAGE%:%version%
::Push
docker push %USERNAME%/%IMAGE%:latest
docker push %USERNAME%/%IMAGE%:%version%
::Go back
cd scripts/Docker