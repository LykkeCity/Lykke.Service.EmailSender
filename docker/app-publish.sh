#!/usr/bin/env bash

echo "Building binary deliverables..."
dotnet restore Lykke.Service.EmailSender.sln

dotnet publish src/Lykke.Service.EmailSender -o ../../app/Lykke.Service.EmailSender
echo "Building binary deliverables complete"
