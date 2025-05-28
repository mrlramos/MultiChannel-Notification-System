# Container Group para Gateway API
resource "azurerm_container_group" "gateway" {
  name                = "${local.project_name}-${local.environment}-gateway"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  ip_address_type     = "Public"
  dns_name_label      = "${local.project_name}-${local.environment}-gateway-${random_string.suffix.result}"
  os_type             = "Linux"
  subnet_ids          = [azurerm_subnet.containers.id]

  container {
    name   = "gateway-api"
    image  = "${azurerm_container_registry.main.login_server}/gateway-api:latest"
    cpu    = var.container_cpu
    memory = var.container_memory

    ports {
      port     = 80
      protocol = "TCP"
    }

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      Services__NotificationAPI = "http://${azurerm_container_group.notification.fqdn}"
      Services__SubscriptionAPI = "http://${azurerm_container_group.subscription.fqdn}"
      APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.main.connection_string
    }
  }

  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }

  tags = local.common_tags

  depends_on = [
    azurerm_container_group.notification,
    azurerm_container_group.subscription
  ]
}

# Container Group para Notification API
resource "azurerm_container_group" "notification" {
  name                = "${local.project_name}-${local.environment}-notification"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  ip_address_type     = "Private"
  os_type             = "Linux"
  subnet_ids          = [azurerm_subnet.containers.id]

  container {
    name   = "notification-api"
    image  = "${azurerm_container_registry.main.login_server}/notification-api:latest"
    cpu    = var.container_cpu
    memory = var.container_memory

    ports {
      port     = 80
      protocol = "TCP"
    }

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.main.connection_string
    }

    secure_environment_variables = {
      ConnectionStrings__CosmosDB = azurerm_cosmosdb_account.main.primary_sql_connection_string
      ConnectionStrings__ServiceBus = azurerm_servicebus_namespace.main.default_primary_connection_string
    }
  }

  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }

  tags = local.common_tags
}

# Container Group para Subscription API
resource "azurerm_container_group" "subscription" {
  name                = "${local.project_name}-${local.environment}-subscription"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  ip_address_type     = "Private"
  os_type             = "Linux"
  subnet_ids          = [azurerm_subnet.containers.id]

  container {
    name   = "subscription-api"
    image  = "${azurerm_container_registry.main.login_server}/subscription-api:latest"
    cpu    = var.container_cpu
    memory = var.container_memory

    ports {
      port     = 80
      protocol = "TCP"
    }

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.main.connection_string
    }

    secure_environment_variables = {
      ConnectionStrings__CosmosDB = azurerm_cosmosdb_account.main.primary_sql_connection_string
    }
  }

  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }

  tags = local.common_tags
}

# Container Group para Processor Worker
resource "azurerm_container_group" "processor" {
  name                = "${local.project_name}-${local.environment}-processor"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  ip_address_type     = "Private"
  os_type             = "Linux"
  subnet_ids          = [azurerm_subnet.containers.id]

  container {
    name   = "processor-worker"
    image  = "${azurerm_container_registry.main.login_server}/processor-worker:latest"
    cpu    = var.container_cpu
    memory = var.container_memory

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.main.connection_string
    }

    secure_environment_variables = {
      ConnectionStrings__ServiceBus = azurerm_servicebus_namespace.main.default_primary_connection_string
    }
  }

  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }

  tags = local.common_tags
} 