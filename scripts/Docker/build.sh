set -ex

# docker hub username
USERNAME=yaroslavholota
# image name
IMAGE=aurora
docker build -t $USERNAME/$IMAGE:latest .