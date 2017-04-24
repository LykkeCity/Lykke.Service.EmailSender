#!/usr/bin/env bash

echo "Building docker deliverables..."

(
    cd app/Lykke.Service.EmailSender
    docker rmi lykkex/lykke-service-emailsender:dev
    docker build -t lykkex/lykke-service-emailsender:dev .
    docker push lykkex/lykke-service-emailsender:dev
)

echo "Building docker deliverables complete"
