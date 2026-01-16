#!/bin/bash

echo "🔧 Fix Build - Nettoyage complet"

# Fermer les processus dotnet
echo "Arrêt des processus dotnet..."
pkill -f "dotnet" 2>/dev/null || true

# Supprimer les dossiers bin et obj
echo "Suppression des dossiers bin et obj..."
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null

# Supprimer le cache Rider
echo "Suppression du cache Rider..."
rm -rf .idea

# Supprimer le cache Blazor.Bootstrap (potentiellement corrompu)
echo "Suppression du cache Blazor.Bootstrap..."
rm -rf ~/.nuget/packages/blazor.bootstrap

# Nettoyer tout le cache NuGet local
echo "Nettoyage du cache NuGet..."
dotnet nuget locals all --clear

# Restaurer les packages
echo "📦 Restauration des packages..."
dotnet restore

# Build
echo "🔨 Build..."
dotnet build

echo "✅ Terminé!"
