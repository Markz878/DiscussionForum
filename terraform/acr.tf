resource "azurerm_container_registry" "acr" {
  name                          = local.containerRegistryName
  location                      = var.location
  resource_group_name           = var.resource_group_name
  sku                           = "Basic"
  admin_enabled                 = false
  network_rule_bypass_option    = "AzureServices"
  public_network_access_enabled = true
}
