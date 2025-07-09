# Copy-Plugins.ps1
# CoffeeLake (C) 2025
#
# Updates plugins platform (SunFlower.Abstractions.dll)
# Copies SunFlower plugins into sources nested catalogs
# nested catalogs: 
#   GUI-less application root
#   GUI application root

Write-Host "--- Importing to Sources ---" -ForegroundColor Blue

# Define paths
$solutionRoot = "D:\GitHub\SunFlower"
$mainAppPath = "$solutionRoot\src\SunFlower.Windows\bin\Debug\net8.0"
$pluginsOutputPath = "$mainAppPath\Plugins"

if (Test-Path $pluginsOutputPath) {
    
}
else {
    New-Item -ItemType Directory -Path $pluginsOutputPath | Out-Null
}
# Updating plugins foundation
$abstractBase = "$mainAppPath\SunFlower.Abstractions.dll"
$abstractTo = "$mainAppPath\Plugins"

if (Test-Path $abstractBase) {
    Copy-Item -Path $abstractBase -Destination $abstractTo -Force
}
else {
    Write-Warning "abstractions not found: $abstractBase"
}

# Define all nested plugins
$pluginProjects = Get-ChildItem -Path "$solutionRoot\plugins" -Directory -Recurse -Depth 1

foreach ($project in $pluginProjects) {
    $projectName = $project.Name
    $dllPath = "$($project.FullName)\bin\Debug\net8.0\$projectName.dll"
    
    if (Test-Path $dllPath) {
        Write-Host "Copying $projectName.dll"
        Copy-Item -Path $dllPath -Destination $pluginsOutputPath -Force
        
        # <optional> Copying databases
        $pdbPath = "$($project.FullName)\bin\Debug\net8.0\$projectName.pdb"
        if (Test-Path $pdbPath) {
            Copy-Item -Path $pdbPath -Destination $pluginsOutputPath -Force
        }
    }
    else {
        Write-Warning "Plugin not found: $dllPath"
    }
}

# Checkout progress
$copiedFiles = Get-ChildItem $pluginsOutputPath
Write-Host "`nCopied plugins ($($copiedFiles.Count)):"
$copiedFiles | ForEach-Object { Write-Host " - $($_.Name)" }

Write-Host "`nPlugins copied successfully to: $pluginsOutputPath" -ForegroundColor Green