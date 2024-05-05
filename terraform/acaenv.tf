resource "azurerm_container_app_environment" "acaenv" {
  name                       = local.containerAppEnvironmentName
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics.id
  infrastructure_subnet_id   = azurerm_subnet.default.id
}
