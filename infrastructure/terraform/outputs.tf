output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "container_registry_login_server" {
  description = "Login server URL for the container registry"
  value       = azurerm_container_registry.main.login_server
}

output "container_registry_admin_username" {
  description = "Admin username for the container registry"
  value       = azurerm_container_registry.main.admin_username
  sensitive   = true
}

output "container_registry_admin_password" {
  description = "Admin password for the container registry"
  value       = azurerm_container_registry.main.admin_password
  sensitive   = true
}

output "gateway_api_url" {
  description = "Public URL for the Gateway API"
  value       = "http://${azurerm_container_group.gateway.fqdn}"
}

output "gateway_api_fqdn" {
  description = "FQDN for the Gateway API"
  value       = azurerm_container_group.gateway.fqdn
}

output "cosmosdb_endpoint" {
  description = "Cosmos DB endpoint"
  value       = azurerm_cosmosdb_account.main.endpoint
}

output "cosmosdb_primary_key" {
  description = "Cosmos DB primary key"
  value       = azurerm_cosmosdb_account.main.primary_key
  sensitive   = true
}

output "cosmosdb_connection_string" {
  description = "Cosmos DB connection string"
  value       = azurerm_cosmosdb_account.main.primary_sql_connection_string
  sensitive   = true
}

output "servicebus_namespace" {
  description = "Service Bus namespace"
  value       = azurerm_servicebus_namespace.main.name
}

output "servicebus_connection_string" {
  description = "Service Bus connection string"
  value       = azurerm_servicebus_namespace.main.default_primary_connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "log_analytics_workspace_id" {
  description = "Log Analytics workspace ID"
  value       = azurerm_log_analytics_workspace.main.workspace_id
}

output "virtual_network_id" {
  description = "Virtual network ID"
  value       = azurerm_virtual_network.main.id
}

output "container_subnet_id" {
  description = "Container subnet ID"
  value       = azurerm_subnet.containers.id
}

output "deployment_summary" {
  description = "Summary of deployed resources"
  value = {
    environment           = var.environment
    location             = var.location
    resource_group       = azurerm_resource_group.main.name
    gateway_url          = "http://${azurerm_container_group.gateway.fqdn}"
    container_registry   = azurerm_container_registry.main.login_server
    cosmos_db           = azurerm_cosmosdb_account.main.name
    service_bus         = azurerm_servicebus_namespace.main.name
    application_insights = azurerm_application_insights.main.name
  }
} 