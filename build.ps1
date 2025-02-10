param(
    [bool]
    $debug = 1
)

if ($debug){
    dotnet build
    Copy-Item .\bin\Debug\netstandard2.1\RiftAllseer.dll D:\SteamLibrary\steamapps\common\RiftOfTheNecroDancerOSTVolume1\BepInEx\plugins\RiftAllseer.dll
    Write-Output "Built and loaded debug"
} else {
    dotnet build -c Release
    Copy-Item .\bin\Release\netstandard2.1\RiftAllseer.dll D:\SteamLibrary\steamapps\common\RiftOfTheNecroDancerOSTVolume1\BepInEx\plugins\RiftAllseer.dll
    Write-Output "Built and loaded release"
}