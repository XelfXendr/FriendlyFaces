set windows-powershell := true

[windows]
copy-assets:
	New-Item -Force -Path "{{justfile_directory()}}\\mod_src" -Name "assets" -ItemType "directory"
	cp "{{justfile_directory()}}\\unity_project\\AssetBundles\\StandaloneWindows\\friendlyfaces" "{{justfile_directory()}}\\mod_src\\assets"
[windows]
build: copy-assets
	cd "{{justfile_directory()}}\\mod_src"; dotnet build

[windows]
assemble: build
	Get-ChildItem -Path "{{justfile_directory()}}\\mod_src\\bin\\Debug\\netstandard2.1\\Xelf.FriendlyFaces.dll", "{{justfile_directory()}}\\package_contents\\*", "{{justfile_directory()}}\\README.md", "{{justfile_directory()}}\\CHANGELOG.md" | \
	Compress-Archive \
		-CompressionLevel "Optimal" \
		-DestinationPath "{{justfile_directory()}}\\FriendlyFaces.zip" \
		-Update