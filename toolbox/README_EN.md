# Toolbox

WPF application for comparing JSON files, with export of differences to Excel.

## Prerequisites

- Windows 10 or higher
- No .NET installation required if you use the "self-contained" version generated during publishing.
- Otherwise, .NET 8 Runtime must be installed: [Download .NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Usage

- Open the application via the Start menu or the desktop shortcut.
- Click "Open" to select JSON files to compare.
- Click "Compare" to display the differences.
- Use "Export" to save the results to an Excel file.
- Use "Close" to remove a file from the list.

---

## Installation

1. **Download the `setup.exe` file**  
   Get the installer generated via Inno Setup (provided in the `Output` folder or at the location chosen during the Inno Setup script compilation).

2. **Run the installer**  
   Double-click on `setup.exe` and follow the on-screen instructions to install the Toolbox application.

3. **Shortcuts**  
   At the end of the installation, a shortcut will be created in the Start menu (and on the desktop if you selected the option).

## Uninstallation

- Go to **Control Panel > Programs and Features**.
- Select "Toolbox" and click "Uninstall".

---

## Creating a setup

### 1. Generate a publish folder

1. Right-click your project in the Solution Explorer > **Publish**.
2. Choose **Folder** as the target.
3. Click **Create Profile**.
4. In advanced settings, you can choose:
   - **Single file**
   - **Self-contained** if you want to include the .NET runtime (your app will work even if .NET is not installed on the target machine)
5. Click **Publish**.

The generated folder will contain all the files needed to run your app (including the `.exe`).

---

### 2. Create an installer (`setup.exe`)

#### Option A: Inno Setup (simple and free)

1. Download [Inno Setup](https://jrsoftware.org/isinfo.php) and install it.
2. Launch the **Inno Script Wizard**.
3. Set the publish folder generated in step 1 as the source.
4. Follow the steps to configure the name, shortcuts, etc.
5. Compile the script to get a `setup.exe`.

#### Option B: Visual Studio Installer Projects

1. Install the **Visual Studio Installer Projects** extension from the Visual Studio Marketplace.
2. Add a new **Setup Project** to your solution.
3. Add the output of your WPF project to the setup.
4. Configure shortcuts, icon, etc.
5. Build the setup project to get a `setup.exe`.

### 3. Use Inno Setup Script Wizard

1. Open **Inno Setup Compiler**.
2. Click **File > New** and choose **Create a new script file using the Script Wizard**.

### 4. Follow the Wizard steps

- **Application Information**
  - Name: `Toolbox`
  - Version: `1.0`
  - Publisher: (your name or company)
  - Application Website: (optional)
- **Application Folder**
  - Default: `{pf}\Toolbox` (leave as is or adjust)
- **Application Files**
  - Click **Add Folder...**
  - Select the publish folder (e.g., `bin\Release\net8.0-windows\publish\`)
  - Check **Include all files and subfolders**
- **Application Shortcuts**
  - Check **Create a shortcut in the Start Menu**
  - For the shortcut, target your `.exe` (e.g., `toolbox.exe`)
  - (Optional) Check **Create a shortcut on the Desktop**
- **Application Documentation**
  - Add a README or license file if you want (optional)
- **Setup Languages**
  - Choose the desired languages (French, English, etc.)
- **Compiler Output**
  - Leave as default or choose an output folder for the setup

### 5. Compile

- Click **Finish** at the end of the wizard.
- The script opens in Inno Setup: click **Compile**.

### 6. Result

- You will get a `setup.exe` ready to distribute in the chosen output folder.

© 2025 Toolbox
