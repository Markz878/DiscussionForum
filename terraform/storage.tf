resource "azurerm_storage_account" "storage_account" {
  name                          = local.storageName
  location                      = var.location
  resource_group_name           = var.resource_group_name
  account_replication_type      = "LRS"
  account_tier                  = "Standard"
  public_network_access_enabled = true
  enable_https_traffic_only     = true
  account_kind                  = "StorageV2"
  network_rules {
    virtual_network_subnet_ids = [azurerm_subnet.default.id]
    default_action             = "Deny"
    ip_rules                   = ["91.153.83.107"]
    bypass                     = ["AzureServices"]
  }
}

resource "azurerm_storage_container" "attachments" {
  name                 = "files"
  storage_account_name = azurerm_storage_account.storage_account.name
  metadata = {
    cachecontrol : "public, max-age=31536000"
  }
}
