window.budgetApp = window.budgetApp || {};

// Déclenche le téléchargement d'un fichier texte (CSV) côté navigateur.
// Le préfixe ﻿ (BOM UTF-8) garantit l'ouverture correcte des accents dans Excel.
window.budgetApp.downloadFile = function (filename, content, mime) {
    const blob = new Blob(['﻿' + content], { type: mime || 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
