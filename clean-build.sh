#!/bin/bash

echo "🧹 Nettoyage des fichiers temporaires..."

# Arrêter tout processus dotnet qui pourrait bloquer
echo "Arrêt des processus dotnet..."
pkill -f "dotnet.*BudgetApp" || true

# Nettoyer les répertoires bin et obj
echo "Suppression des dossiers bin et obj..."
find . -type d -name "bin" -o -name "obj" | while read dir; do
    rm -rf "$dir"
    echo "  ✓ Supprimé: $dir"
done

# Nettoyer le cache NuGet local du projet
echo "Nettoyage du cache..."
dotnet clean --verbosity quiet

# Restaurer les packages
echo "📦 Restauration des packages NuGet..."
dotnet restore

# Rebuild complet
echo "🔨 Build complet..."
dotnet build

echo "✅ Nettoyage et build terminés!"
