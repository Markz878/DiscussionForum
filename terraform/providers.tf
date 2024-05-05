data "azurerm_client_config" "current" {}

terraform {
  backend "azurerm" {
    use_oidc = true
  }
}

provider "azurerm" {
  use_oidc                   = true
  skip_provider_registration = true
  features {}
}
