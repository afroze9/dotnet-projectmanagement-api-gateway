$token = kubectl get secrets/consul-bootstrap-acl-token --template='{{.data.token | base64decode }}' --namespace consul
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("X-Consul-Token", "$token")

$rules = Get-Content "rules.hcl" -Raw
$policy_name = "kv-api-gateway"
$service_name = "api-gateway"

$body = @{
    Name = "$policy_name"
    Description = "Policy for api-gateway key prefix"
    Rules = $rules
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:8500/v1/acl/policy" `
    -Method PUT `
    -Headers $headers `
    -Body $body

$policy_id = $response.ID

echo "Policy Created"
echo "Name: $policy_name"
echo "Id: $policy_id"

$body = @{
    Description = "Token for $service_name service"
    Policies = @(
        @{
            Name = "$policy_name"
        }
    )
    ServiceIdentities = @(
        @{
            ServiceName = "$service_name"
        }
    )
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:8500/v1/acl/token" `
    -Method PUT `
    -Headers $headers `
    -Body $body

$secret_id = $response.SecretID

echo "Service Name: $service_name"
echo "Token: $secret_id"

$body = Get-Content -Raw -Path "app-config.json"

$response = Invoke-RestMethod `
    -Uri "http://localhost:8500/v1/kv/$service_name/app-config" `
    -Method PUT `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $body

Write-Host "Key updated successfully."

