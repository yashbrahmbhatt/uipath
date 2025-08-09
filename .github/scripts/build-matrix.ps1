# Build matrix script for changed projects
param(
    [string]$BeforeCommit = $env:GITHUB_EVENT_BEFORE,
    [string]$AfterCommit = $env:GITHUB_SHA
)

function Get-ChangedFiles {
    try {
        if ($BeforeCommit -and $AfterCommit) {
            return git diff --name-only $BeforeCommit $AfterCommit
        } else {
            return git diff --name-only HEAD~1 HEAD
        }
    } catch {
        return git diff --name-only HEAD~1 HEAD
    }
}

function Get-ProjectsToBuild {
    param([array]$ChangedFiles, [object]$MonoConfig)
    
    $projectsTouild = @{}
    
    # Find directly changed projects
    foreach ($project in $MonoConfig.projects) {
        if (-not $project.build) { continue }
        
        $projectPath = $project.path.Replace('/', '\')
        foreach ($file in $ChangedFiles) {
            if ($file.Replace('/', '\') -like "$projectPath*") {
                $projectsTouild[$project.id] = $project
                break
            }
        }
    }
    
    # Add transitive dependencies
    $added = $true
    while ($added) {
        $added = $false
        foreach ($project in $MonoConfig.projects) {
            if ($projectsTouild.ContainsKey($project.id) -or -not $project.build) { continue }
            
            if ($project.PSObject.Properties['dependsOn']) {
                foreach ($dep in $project.dependsOn) {
                    if ($projectsTouild.ContainsKey($dep)) {
                        $projectsTouild[$project.id] = $project
                        $added = $true
                        break
                    }
                }
            }
        }
    }
    
    return $projectsTouild.Values
}

function Get-TopologicallySorted {
    param([array]$Projects)
    
    $visited = @{}
    $result = @()
    
    function Visit($project) {
        if ($visited[$project.id] -eq 'temp') { throw "Cycle detected at $($project.id)" }
        if ($visited[$project.id]) { return }
        
        $visited[$project.id] = 'temp'
        
        if ($project.PSObject.Properties['dependsOn']) {
            foreach ($depId in $project.dependsOn) {
                $dep = $Projects | Where-Object { $_.id -eq $depId }
                if ($dep) { Visit $dep }
            }
        }
        
        $visited[$project.id] = 'perm'
        $result += $project
    }
    
    foreach ($project in $Projects) {
        Visit $project
    }
    
    return $result
}

# Main execution
$changedFiles = Get-ChangedFiles
$monoConfig = Get-Content "mono.json" | ConvertFrom-Json
$projectsToBuild = Get-ProjectsTouild -ChangedFiles $changedFiles -MonoConfig $monoConfig
$sortedProjects = Get-TopologicallySorted -Projects $projectsToBuild

if ($sortedProjects.Count -eq 0) {
    $matrix = '{"include":[]}'
} else {
    $includes = $sortedProjects | ForEach-Object {
        [PSCustomObject]@{
            id = $_.id
            path = $_.path
            type = $_.type
            deploySteps = $_.deploySteps
            dependsOn = $_.dependsOn
        }
    }
    $matrix = @{ include = $includes } | ConvertTo-Json -Depth 5 -Compress
}

Write-Host "Matrix: $matrix"
"matrix=$matrix" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
