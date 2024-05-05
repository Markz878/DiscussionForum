resource "azurerm_container_app" "webapp" {
  name                         = var.solution_name
  resource_group_name          = var.resource_group_name
  container_app_environment_id = azurerm_container_app_environment.acaenv.id
  revision_mode                = "Single"
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.webappIdentity.id
  }

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 8080
    exposed_port               = 80
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.webappIdentity.id]
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = var.solution_name
      image  = "${azurerm_container_registry.acr.login_server}/${var.solution_name}:${var.image_tag}"
      cpu    = 0.5
      memory = "1.0Gi"

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = azurerm_application_insights.appinsights.connection_string
      }
      env {
        name  = "ConnectionStrings__SqlServer"
        value = "Server=${azurerm_mssql_server.server.fully_qualified_domain_name};Initial Catalog=${local.databaseName};Authentication=Active Directory Managed Identity;User Id=${azurerm_user_assigned_identity.webappIdentity.client_id};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
      }
      env {
        name  = "FileStorageSettings__StorageUri"
        value = azurerm_storage_account.storage_account.primary_blob_endpoint
      }
      env {
        name  = "Azure__SignalR__ConnectionString"
        value = "Endpoint=https://${azurerm_signalr_service.signalr.hostname};AuthType=azure.msi;ClientId=${azurerm_user_assigned_identity.webappIdentity.client_id};Version=1.0;"
      }
      startup_probe {
        port      = 8080
        path      = "/health"
        transport = "HTTPS"
      }
    }
  }
}
