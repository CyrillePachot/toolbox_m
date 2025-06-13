# Toolbox

Application WPF de comparaison de fichiers JSON, avec export des différences vers Excel.

## Prérequis

- Windows 10 ou supérieur
- Aucune installation de .NET requise si tu utilises la version "self-contained" générée lors de la publication.
- Sinon, .NET 8 Runtime doit être installé : [Télécharger .NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Utilisation

- Ouvre l’application via le menu Démarrer ou le raccourci sur le bureau.
- Clique sur "Ouvrir" pour sélectionner des fichiers JSON à comparer.
- Clique sur "Comparer" pour afficher les différences.
- Utilise "Exporter" pour sauvegarder les résultats dans un fichier Excel.
- Utilise "Fermer" pour retirer un fichier de la liste.

---

## Installation

1. **Télécharge le fichier `setup.exe`**  
   Récupère l’installeur généré via Inno Setup (fourni dans le dossier `Output` ou à l’emplacement choisi lors de la compilation du script Inno Setup).

2. **Lance l’installeur**  
   Double-clique sur `setup.exe` et suis les instructions à l’écran pour installer l’application Toolbox.

3. **Raccourcis**  
   À la fin de l’installation, un raccourci sera créé dans le menu Démarrer (et sur le bureau si tu as coché l’option).

## Désinstallation

- Va dans **Panneau de configuration > Programmes et fonctionnalités**.
- Sélectionne "Toolbox" puis clique sur "Désinstaller".

---

## Creation d'un setup

### 1. Générer un dossier de publication

1. Clique droit sur ton projet dans l’Explorateur de solutions > **Publier**.
2. Choisis **Dossier** comme cible.
3. Clique sur **Créer un profil**.
4. Dans les paramètres avancés, tu peux choisir :
   - **Fichier unique** (Single file)
   - **Autoporté** (Self-contained) si tu veux inclure le runtime .NET (ton appli marchera même si .NET n’est pas installé sur la machine cible)
5. Clique sur **Publier**.

Le dossier généré contiendra tous les fichiers nécessaires pour lancer ton appli (dont le `.exe`).

---

### 2. Créer un installeur (`setup.exe`)

#### Option A : Inno Setup (simple et gratuit)

1. Télécharge [Inno Setup](https://jrsoftware.org/isinfo.php) et installe-le.
2. Lance l’assistant (**Inno Script Wizard**).
3. Indique le dossier de publication généré à l’étape 1 comme source.
4. Suis les étapes pour configurer le nom, les raccourcis, etc.
5. Compile le script pour obtenir un `setup.exe`.

#### Option B : Visual Studio Installer Projects

1. Installe l’extension **Visual Studio Installer Projects** depuis le Marketplace Visual Studio.
2. Ajoute un nouveau projet de type **Setup Project** à ta solution.
3. Ajoute la sortie de ton projet WPF dans le setup.
4. Configure les raccourcis, l’icône, etc.
5. Compile le projet setup pour obtenir un `setup.exe`.

### 3. Utiliser Inno Setup Script Wizard

1. Ouvre **Inno Setup Compiler**.
2. Clique sur **File > New** puis choisis **Create a new script file using the Script Wizard**.

### 4. Suis les étapes du Wizard

- **Application Information**
  - Name: `Toolbox`
  - Version: `1.0`
  - Publisher: (ton nom ou société)
  - Application Website: (optionnel)
- **Application Folder**
  - Par défaut: `{pf}\Toolbox` (laisse par défaut ou adapte)
- **Application Files**
  - Clique sur **Add Folder...**
  - Sélectionne le dossier de publication (ex: `bin\Release\net8.0-windows\publish\`)
  - Coche **Include all files and subfolders**
- **Application Shortcuts**
  - Coche **Create a shortcut in the Start Menu**
  - Pour le raccourci, cible ton `.exe` (ex: `toolbox.exe`)
  - (Optionnel) Coche **Create a shortcut on the Desktop**
- **Application Documentation**
  - Ajoute un fichier README ou licence si tu veux (optionnel)
- **Setup Languages**
  - Choisis les langues souhaitées (français, anglais...)
- **Compiler Output**
  - Laisse par défaut ou choisis un dossier de sortie pour le setup

### 5. Compile

- Clique sur **Finish** à la fin du wizard.
- Le script s’ouvre dans Inno Setup : clique sur **Compile**.

### 6. Résultat

- Tu obtiens un `setup.exe` prêt à distribuer dans le dossier de sortie choisi.

© 2025 Toolbox
