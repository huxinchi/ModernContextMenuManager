@echo off
dotnet publish .\ModernContextMenuManager.csproj -c Release -r win-x64 -o bin/output/x64
dotnet publish .\ModernContextMenuManager.csproj -c Release -r win-x86 -o bin/output/x86
dotnet publish .\ModernContextMenuManager.csproj -c Release -r win-arm64 -o bin/output/arm64

del /q bin\output\x64\ModernContextMenuManager.pdb
del /q bin\output\x86\ModernContextMenuManager.pdb
del /q bin\output\arm64\ModernContextMenuManager.pdb