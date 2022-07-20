set USERNAME=yaroslavholota
set IMAGE=aurora
cd ..
cd ..
cd src
docker build -t %USERNAME%/%IMAGE%:latest -f Dockerfile .
docker build -t %USERNAME%/%IMAGE%-arm64:latest --platform linux/arm64/v8 -f Dockerfile-arm64 .
cd ..