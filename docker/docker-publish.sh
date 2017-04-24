#!/usr/bin/env bash

echo "Calls all deployment chain in a proper order"
docker/app-publish.sh
docker/docker-build.sh
