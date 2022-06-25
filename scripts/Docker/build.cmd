set USERNAME=yaroslavholota
set IMAGE=aurora
cd src
docker build -t %USERNAME%/%IMAGE%:latest .
cd ..