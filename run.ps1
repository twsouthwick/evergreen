$r = $PSScriptRoot

pushd src/operator
dotnet publish "./EvergreenOperator.csproj" -c RELEASE -o $r/publish /p:UseAppHost=false

popd

# Check and update deployment.yaml to ensure imagePullPolicy is set to IfNotPresent
$deploymentPath = "$r/k8s/deployment.yaml"
$deploymentContent = Get-Content $deploymentPath -Raw

if ($deploymentContent -notmatch "imagePullPolicy:\s*IfNotPresent") {
    Write-Host "Adding imagePullPolicy: IfNotPresent to deployment.yaml"
    
    # Replace the image line with image + imagePullPolicy
    $updatedContent = $deploymentContent -replace "(\s+image:\s+operator\s*\n)", "`$1        imagePullPolicy: IfNotPresent`n"
    Set-Content $deploymentPath $updatedContent -NoNewline
} else {
    Write-Host "imagePullPolicy: IfNotPresent already present in deployment.yaml"
}

pushd $r
docker build -t evergreen-operator .
popd

kubectl apply -k $r/k8s
kubectl apply -f $r/src/operator/TestEvergreen.yml