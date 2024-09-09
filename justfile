set windows-powershell := true

build:
	cd mod_src; dotnet build

[windows]
assemble: build
	Get-ChildItem -Path "mod_src\\bin\\Debug\\netstandard2.1\\Xelf.FriendlyFaces.dll", "icon.png" | \
	Compress-Archive \
		-CompressionLevel "Optimal" \
		-DestinationPath "FriendlyFaces.zip" \
		-Update