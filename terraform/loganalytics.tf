resource "azurerm_log_analytics_workspace" "log_analytics" {
  name                          = local.loganalyticsName
  location                      = var.location
  resource_group_name           = var.resource_group_name
  sku                           = "PerGB2018"
  daily_quota_gb                = 1
  retention_in_days             = 30
  local_authentication_disabled = true
}
