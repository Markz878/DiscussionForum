param location string = resourceGroup().location
@minLength(5)
param solutionName string

var containerRegistryName = 'acr${solutionName}'
var loganalyticsName = 'log-${solutionName}'
var appinsightsName = 'ai-${solutionName}'
var vnetName = 'vnet-${solutionName}'
var containerAppEnvironmentName = 'cae-${solutionName}'
var storageName = 'st${solutionName}'
var sqlServerName = 'sql-${solutionName}'
var databaseName = 'sqldb-${solutionName}'
var signalRName = 'sigr-${solutionName}'
var sqlPrivateEndpointName = 'pe-sql-${solutionName}'
var sqlPrivateEndpointDnsZoneName = 'privatelink${environment().suffixes.sqlServerHostname}'
var stPrivateEndpointName = 'pe-st-${solutionName}'
var stPrivateEndpointDnsZoneName = 'privatelink.blob.${environment().suffixes.storage}'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
  }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: loganalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: json('0.1')
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appinsightsName
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    DisableLocalAuth: true
    RetentionInDays: 30
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'default'
        properties: {
          addressPrefix: '10.0.0.0/23'
        }
      }
    ]
  }
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-09-01' existing = {
  parent: vnet
  name: 'default'
}

resource containerappEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvironmentName
  location: location
  properties: {
    vnetConfiguration: {
      internal: false
      infrastructureSubnetId: subnet.id
    }
    infrastructureResourceGroup: '${resourceGroup().name }-managed'
    zoneRedundant: false
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_ZRS'
  }
  properties: {
    allowSharedKeyAccess: false
    publicNetworkAccess: 'Disabled'
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Deny'
    }
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    defaultToOAuthAuthentication: true
    accessTier: 'Cool'
  }
}
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    changeFeed: {
      enabled: false
    }
    isVersioningEnabled: false
  }
}
resource filesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'files'
  properties: {
    publicAccess: 'None'
    metadata: {
      cachecontrol: 'public, max-age=31536000'
    }
  }
}

// resource defenderForStorageSettings 'Microsoft.Security/defenderForStorageSettings@2022-12-01-preview' = {
//     name: 'current'
//     scope: storageAccount
//     properties: {
//         isEnabled: true
//         malwareScanning: {
//             onUpload: {
//                 isEnabled: true
//                 capGBPerMonth: 5
//             }
//         }
//         sensitiveDataDiscovery: {
//             isEnabled: true
//         }
//         overrideSubscriptionLevelSettings: true
//     }
// }

resource stPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-09-01' = {
  name: stPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: subnet.id
    }
    customNetworkInterfaceName: '${stPrivateEndpointName}-nic'
    privateLinkServiceConnections: [
      {
        name: stPrivateEndpointName
        properties: {
          privateLinkServiceId: storageAccount.id
          groupIds: [
            'blob'
          ]
        }
      }
    ]
  }
}

resource stPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: stPrivateEndpointDnsZoneName
  location: 'global'
}

resource stPrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  name: '${stPrivateEndpointName}-link'
  parent: stPrivateDnsZone
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: { id: vnet.id }
  }
}

resource stPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = {
  name: '${stPrivateEndpointName}-group'
  parent: stPrivateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: stPrivateDnsZone.id
        }
      }
    ]
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: 'DiscussionForumSQLAdmins'
      sid: '71cd5d5a-94e5-4464-9944-86efd18c1c45'
      tenantId: '0424f4a0-f409-4185-9d9d-76ce62c20e4d'
      azureADOnlyAuthentication: true
    }
  }
}

resource sqlserverDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 2
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368
    autoPauseDelay: 60
    requestedBackupStorageRedundancy: 'Local'
    minCapacity: json('0.5')
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    availabilityZone: 'NoPreference'
  }
}

resource sqlPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-09-01' = {
  name: sqlPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: subnet.id
    }
    customNetworkInterfaceName: '${sqlPrivateEndpointName}-nic'
    privateLinkServiceConnections: [
      {
        name: sqlPrivateEndpointName
        properties: {
          privateLinkServiceId: sqlServer.id
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }
}

resource sqlPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: sqlPrivateEndpointDnsZoneName
  location: 'global'
}

resource sqlPrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  name: '${sqlPrivateEndpointName}-link'
  parent: sqlPrivateDnsZone
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: { id: vnet.id }
  }
}

resource sqlPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = {
  name: '${sqlPrivateEndpointName}-group'
  parent: sqlPrivateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: sqlPrivateDnsZone.id
        }
      }
    ]
  }
}

resource signalR 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: signalRName
  location: location
  sku: {
    capacity: 1
    name: 'Free_F1'
  }
  kind: 'SignalR'
  identity: {
    type: 'None'
  }
  properties: {
    disableLocalAuth: true
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'True'
      }
      {
        flag: 'EnableMessagingLogs'
        value: 'False'
      }
      {
        flag: 'EnableLiveTrace'
        value: 'False'
      }
    ]
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
  }
}

resource webappIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${solutionName}-identity'
  location: location
}

resource acrPullRoleDef 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: containerRegistry
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}
resource webappAcrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, webappIdentity.id, acrPullRoleDef.id)
  scope: containerRegistry
  properties: {
    roleDefinitionId: acrPullRoleDef.id
    principalId: webappIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageBlobContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: storageAccount
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}
resource webappStorageRoleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, webappIdentity.id, storageBlobContributor.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: storageBlobContributor.id
    principalId: webappIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource appinsights_monitoring_publisher 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: appInsights
  name: '3913510d-42f4-4e42-8a64-420c390055eb'
}
resource appinsights_monitoring_roleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, webappIdentity.id, appinsights_monitoring_publisher.id)
  scope: appInsights
  properties: {
    roleDefinitionId: appinsights_monitoring_publisher.id
    principalId: webappIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource sqldb_security_manager 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: sqlServer
  name: '056cd41c-7e88-42e1-933e-88ba6a50c9c3'
}
resource sqldb_security_manager_roleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, webappIdentity.id, sqldb_security_manager.id)
  scope: sqlServer
  properties: {
    roleDefinitionId: sqldb_security_manager.id
    principalId: webappIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource sqldb_server_contributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: sqlServer
  name: '6d8ee4ec-f05a-4a1d-8b00-a9b17e38b437'
}
resource sqldb_server_contributor_roleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, webappIdentity.id, sqldb_server_contributor.id)
  scope: sqlServer
  properties: {
    roleDefinitionId: sqldb_server_contributor.id
    principalId: webappIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource signalRAppServerRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: signalR
  name: '420fcaa2-552c-430f-98ca-3264be4806c7'
}
resource webappSignalRRoleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, webappIdentity.id, signalRAppServerRole.id)
  scope: signalR
  properties: {
    roleDefinitionId: signalRAppServerRole.id
    principalId: webappIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
