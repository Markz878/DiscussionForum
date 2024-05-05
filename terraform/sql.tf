resource "azurerm_mssql_server" "server" {
  name                                 = local.sqlServerName
  resource_group_name                  = var.resource_group_name
  location                             = var.location
  version                              = "12.0"
  public_network_access_enabled        = true
  outbound_network_restriction_enabled = true

  azuread_administrator {
    azuread_authentication_only = true
    login_username              = "DiscussionForumSQLAdmins"
    object_id                   = "71cd5d5a-94e5-4464-9944-86efd18c1c45"
    tenant_id                   = "0424f4a0-f409-4185-9d9d-76ce62c20e4d"
  }
}

resource "azurerm_mssql_database" "database" {
  name                        = local.databaseName
  server_id                   = azurerm_mssql_server.server.id
  collation                   = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb                 = 1
  sku_name                    = "Basic"
  geo_backup_enabled          = false
  zone_redundant              = false
}
