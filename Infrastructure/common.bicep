param location string = resourceGroup().location
param solutionName string = 'discussionforum'
param containerRegistryName string = 'acr${solutionName}'
param loganalyticsName string = 'log-${solutionName}'
param appinsightsName string = 'ai-${solutionName}'
param vnetName string = 'vnet-${solutionName}'
param containerAppEnvironmentName string = 'cae-${solutionName}'
param storageName string = 'st${solutionName}'
param sqlServerName string = 'sql-${solutionName}'
param databaseName string = 'sqldb-${solutionName}'
param signalRName string = 'sigr-${solutionName}'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
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
        policies: {
            exportPolicy: {
                status: 'enabled'
            }
            azureADAuthenticationAsArmPolicy: {
                status: 'enabled'
            }
        }
        publicNetworkAccess: 'Enabled'
        networkRuleBypassOptions: 'AzureServices'
    }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
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
    }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
    name: appinsightsName
    kind: 'web'
    location: location
    properties: {
        Application_Type: 'web'
        WorkspaceResourceId: logAnalytics.id
    }
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-02-01' = {
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
                    serviceEndpoints: [
                        {
                            service: 'Microsoft.Storage'
                            locations: [
                                'northeurope'
                            ]
                        }
                        {
                            service: 'Microsoft.Sql'
                            locations: [
                                'northeurope'
                            ]
                        }
                    ]
                    privateLinkServiceNetworkPolicies: 'Enabled'
                }
            }
        ]
    }
}

resource containerappEnvironment 'Microsoft.App/managedEnvironments@2022-11-01-preview' = {
    name: containerAppEnvironmentName
    location: location
    properties: {
        vnetConfiguration: {
            internal: false
            infrastructureSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, 'default')
        }
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

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
    name: storageName
    location: location
    kind: 'StorageV2'
    sku: {
        name: 'Standard_ZRS'
    }
    properties: {
        allowSharedKeyAccess: true
        networkAcls: {
            bypass: 'None'
            defaultAction: 'Allow'
            virtualNetworkRules: [
                {
                    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, 'default')
                    action: 'Allow'
                }
            ]
            ipRules: []
        }
        supportsHttpsTrafficOnly: true
        minimumTlsVersion: 'TLS1_2'
        defaultToOAuthAuthentication: true
        accessTier: 'Cool'
    }
}
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-05-01' = {
    parent: storageAccount
    name: 'default'
    properties: {
        changeFeed: {
            enabled: false
        }
        isVersioningEnabled: false
    }
}
resource filesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-05-01' = {
    parent: blobService
    name: 'files'
    properties: {
        publicAccess: 'Blob'
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

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
    name: sqlServerName
    location: location
    properties: {
        minimalTlsVersion: '1.2'
        publicNetworkAccess: 'Enabled'
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
resource sqlServerVnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2022-11-01-preview' = {
    parent: sqlServer
    name: 'vnet-rule'
    properties: {
        virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, 'default')
        ignoreMissingVnetServiceEndpoint: false
    }
}

resource sqlserverDatabase 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
    parent: sqlServer
    name: databaseName
    location: location
    sku: {
        name: 'Basic'
        tier: 'Basic'
        capacity: 5
    }
    properties: {
        collation: 'SQL_Latin1_General_CP1_CI_AS'
        maxSizeBytes: 104857600
    }
}

resource signalR 'Microsoft.SignalRService/signalR@2022-02-01' = {
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
        tls: {
            clientCertEnabled: false
        }
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
        networkACLs: {
            defaultAction: 'Deny'
            publicNetwork: {
                allow: [
                    'ClientConnection'
                    'ServerConnection'
                    'RESTAPI'
                    'Trace'
                ]
            }
        }
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
