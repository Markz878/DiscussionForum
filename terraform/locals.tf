locals {
 containerRegistryName = "acr${var.solution_name}"
 loganalyticsName = "log-${var.solution_name}"
 appinsightsName = "ai-${var.solution_name}"
 vnetName = "vnet-${var.solution_name}"
 containerAppEnvironmentName = "cae-${var.solution_name}"
 storageName = "st${var.solution_name}"
 sqlServerName = "sql-${var.solution_name}"
 databaseName = "sqldb-${var.solution_name}"
 signalRName = "sigr-${var.solution_name}"
}