TFM version: netstandard2.1
    netstandard.dll exists, file version 2.1.0.0

Unity version: 2021.3.43

`dotnet new bep6plugin_unity_mono -n MyFirstPlugin -T netstandard2.1 -U 2021.3.43`
`dotnet restore MyFirstPlugin`

build - `dotnet build`
move contents of 'bin/Debug' to plugins folder.


# ~~Unity Explorer~~ doesn't work:
https://github.com/sinai-dev/UnityExplorer
`unity explorer doesn't work w/ most recent version of bepinex`

# dnSpyEx
https://github.com/dnSpyEx/dnSpy


https://github.com/dnSpyEx/dnSpy-Unity-mono

patched sdlls
https://github.com/liesauer/Unity-debugging-dlls/releases
    - patched w/ unity-2019.1.8, might need to build patch
    - didn't work

    - need correct version...

# ILSPY (for code analysis)
https://github.com/icsharpcode/ILSpy
Assembly-CSharp.dll

# AssetStudio (deprecated)
AssetStudio2024 (need to compile...)

# AssetRipper
- nice, it works... kinda.

#
- examples: https://github.com/kobrakon/ClientModdingExamples/blob/main/C%23%20Mod%20Examples/UseBepInExConfiguration/ExampleController.cs

# Harmony Patching

```

RRTileView {
    private MaterialPropertyBlock _materialPropertyBlock;
}

using it in postfix -> add three ___

[HarmonyPostfix]
static void CreateTile(ref MaterialPropertyBlock ____materialPropertyBlock){
    Vector3 newPosition = _materialPropertyBlock.GetVector("_Position");
    newPosition.x += 300.0f;
    _materialPropertyBlock.SetVector("_Position", newPosition);
}


```

# thunking
- 
