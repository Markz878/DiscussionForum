resource "azurerm_user_assigned_identity" "webappIdentity" {
  location            = var.location
  name                = "${var.solution_name}-identity"
  resource_group_name = var.resource_group_name
}

resource "azurerm_role_assignment" "acr_pull_role" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.webappIdentity.principal_id
}

resource "azurerm_role_assignment" "appinsights_write_role" {
  scope                = azurerm_application_insights.appinsights.id
  role_definition_name = "Monitoring Metrics Publisher"
  principal_id         = azurerm_user_assigned_identity.webappIdentity.principal_id
}

resource "azurerm_role_assignment" "signalr_write_role" {
  scope                = azurerm_signalr_service.signalr.id
  role_definition_name = "SignalR App Server"
  principal_id         = azurerm_user_assigned_identity.webappIdentity.principal_id
}

resource "azurerm_role_assignment" "storage_contributor_role" {
  scope                = azurerm_storage_account.storage_account.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.webappIdentity.principal_id
}
