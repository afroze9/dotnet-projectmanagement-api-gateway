param($tokenResult)

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

$body = Get-Content -Raw -Path "app-config.json" | ConvertFrom-Json
$body.Consul.Token = $secret_id
$body.TelemetrySettings.Endpoint = "http://jaeger:4317"
$body | ConvertTo-Json -Depth 10 | Set-Content -Path "app-config.json"

$body = Get-Content -Raw -Path "app-config.json"

$response = Invoke-RestMethod `
    -Uri "http://localhost:8500/v1/kv/$service_name/app-config" `
    -Method PUT `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $body

# Add token to ocelot.global.json
$ocelot_global_content = Get-Content -Raw "..\Ocelot\ocelot.global.json" | ConvertFrom-Json
$ocelot_global_content.GlobalConfiguration.ServiceDiscoveryProvider.Host = "consul-server1"
$ocelot_global_content.GlobalConfiguration.ServiceDiscoveryProvider.Token = $secret_id
$ocelot_global_content | ConvertTo-Json -Depth 10 | Set-Content "..\Ocelot\ocelot.global.json"

# Add token to appsettings.json file
$appsettings_content = Get-Content -Raw "..\appsettings.json" | ConvertFrom-Json
$appsettings_content.ConsulKV.Url = "http://consul-server1:8500"
$appsettings_content.ConsulKV.Token = $secret_id
$appsettings_content | ConvertTo-Json -Depth 10 | Set-Content "..\appsettings.json"

Write-Host "Key updated successfully."

Set-Variable -Name $tokenResult -Value $secret_id -Scope 1
