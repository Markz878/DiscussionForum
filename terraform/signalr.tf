resource "azurerm_signalr_service" "signalr" {
  name                          = local.signalRName
  location                      = var.location
  resource_group_name           = var.resource_group_name
  service_mode                  = "Default"
  aad_auth_enabled              = true
  local_auth_enabled            = false

  sku {
    name     = "Free_F1"
    capacity = 1
  }

  cors {
    allowed_origins = ["*"]
  }
}

# resource "azurerm_signalr_service_network_acl" "signalr_network" {
#   signalr_service_id = azurerm_signalr_service.signalr.id
#   default_action     = "Deny"

#   public_network {
#     allowed_request_types = [
#       "ClientConnection",
#       "ServerConnection",
#       "RESTAPI",
#       "Trace"
#     ]
#   }
# }
