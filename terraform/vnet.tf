resource "azurerm_virtual_network" "vnet" {
  name                = local.vnetName
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = ["10.5.0.0/16"]
}

resource "azurerm_subnet" "default" {
  name                 = "default"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  private_link_service_network_policies_enabled = true
  private_endpoint_network_policies_enabled = true
  address_prefixes     = ["10.5.0.0/23"]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.Sql"]
}
