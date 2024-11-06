# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$rootFolder = Join-Path $packFolder "../"

# List of solutions
$solutions = (
    "basics"
)

# List of projects
$projects = (
    "src/Ty.Module",
    "src/Ty.UIBase",
    "src/Ty.WPFBase",
    "src/Ty.Configs",
    "src/Ty.AvaloniaBase",
    "tools/Ty.SourceGenerator"
)
