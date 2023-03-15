Param (
    [Parameter(Mandatory = $true)]
    [ValidateNotNull()]
    [ValidateNotNullOrEmpty()]
    [ValidateSet('production', 'development', 'staging')]
    [string]
    $Environment

    [Switch]
    $Raw
)

$toBuildProjects = New-Object System.Collections.ArrayList
    $files = Get-ChildItem -Recurse -Include "*.csproj" `
    | Select-Object @{Name = "FullName"; Expression = { Join-Path -Path $_.Directory -ChildPath $_.Name } } `
    | Where-Object { $_.FullName - 'Flyingdarts-*'}