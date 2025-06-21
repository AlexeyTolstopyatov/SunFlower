# Copy-Plugins.ps1
# CoffeeLake (C) 2025
#
# Copies SunFlower plugins into .../test/.../bin/Debug/net8.0/Plugins
# 

# Define paths
$solutionRoot = "D:\GitHub\SunFlower"
$mainAppPath = "$solutionRoot\src\SunFlower\bin\Debug\net8.0"
$pluginsOutputPath = "$mainAppPath\Plugins"

# Cleaning target directory
Write-Host "Cleaning plugins directory: $pluginsOutputPath"
if (Test-Path $pluginsOutputPath) {
    Remove-Item "$pluginsOutputPath\*" -Recurse -Force
}
else {
    New-Item -ItemType Directory -Path $pluginsOutputPath | Out-Null
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