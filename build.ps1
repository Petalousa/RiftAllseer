param(
    [bool]
    $debug = 1,
    $open_folder = 0
)

$project_guid = "com.petalousa.riftallseer"
$steamapps_folder = "D:\SteamLibrary\steamapps\common\RiftOfTheNecroDancerOSTVolume1\"
$bepinex_folder = Join-Path -Path $steamapps_folder -ChildPath "BepInEx\plugins"
$my_plugin_folder = Join-Path -Path $bepinex_folder -ChildPath $project_guid
# create a the project directory
if (-Not (Test-Path -PathType Container $my_plugin_folder)){
    New-Item -ItemType Directory -Path $my_plugin_folder
}

Copy-Item .\res $my_plugin_folder -Recurse

if ($debug){
    dotnet build
    Copy-Item .\bin\Debug\netstandard2.1\com.petalousa.riftallseer.dll $my_plugin_folder
    Write-Output "Built and loaded debug"
} else {
    dotnet build -c Release
    Copy-Item .\bin\Release\netstandard2.1\com.petalousa.riftallseer.dll $my_plugin_folder
    Write-Output "Built and loaded release"
}

if ($open_folder){
    explorer.exe $my_plugin_folder
}