set USERNAME=yaroslavholota
set IMAGE=aurora
cd ..
cd ..
cd src
docker build -t %USERNAME%/%IMAGE%:latest .
cd ..