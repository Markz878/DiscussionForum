resource "azurerm_application_insights" "appinsights" {
  name                = local.appinsightsName
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
  daily_data_cap_in_gb = 1
  local_authentication_disabled = true
  retention_in_days = 30
  workspace_id = azurerm_log_analytics_workspace.log_analytics.id
}