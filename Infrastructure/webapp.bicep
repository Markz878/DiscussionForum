param location string = resourceGroup().location
param solutionName string
param containerRegistryName string = 'acr${solutionName}'
param appinsightsName string = 'ai-${solutionName}'
param containerAppEnvironmentName string = 'cae-${solutionName}'
param storageName string = 'st${solutionName}'
param sqlServerName string = 'sql-${solutionName}'
param databaseName string = 'sqldb-${solutionName}'
param signalRName string = 'sigr-${solutionName}'
param appName string = solutionName
param imageTag string
param oidcClientId string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: containerRegistryName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appinsightsName
}

resource containerappEnvironment 'Microsoft.App/managedEnvironments@2022-11-01-preview' existing = {
  name: containerAppEnvironmentName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageName
}

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' existing = {
  name: sqlServerName
}

resource signalR 'Microsoft.SignalRService/signalR@2023-02-01' existing = {
  name: signalRName
}

resource webApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${webappIdentity.id}': {}
    }
  }
  properties: {
    environmentId: containerappEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 10
      ingress: {
        external: true
        targetPort: 8080
        transport: 'Auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: webappIdentity.id
        }
      ]
    }
    template: {
      revisionSuffix: imageTag
      containers: [
        {
          name: appName
          image: '${containerRegistry.properties.loginServer}/${appName}:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
          env: [
            {
              name: 'ConnectionStrings__SqlServer'
              value: 'Server=${sqlServer.properties.fullyQualifiedDomainName};Initial Catalog=${databaseName};Authentication=Active Directory Managed Identity;User Id=${webappIdentity.properties.clientId};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'FileStorageSettings__StorageUri'
              value: storageAccount.properties.primaryEndpoints.blob
            }
            {
              name: 'ManagedIdentityId'
              value: webappIdentity.properties.clientId
            }
            {
              name: 'Azure__SignalR__ConnectionString'
              value: 'Endpoint=https://sigr-discussionforum.service.signalr.net;AuthType=azure.msi;ClientId=${webappIdentity.properties.clientId};Version=1.0;'
            }
            {
              name: 'Version'
              value: imageTag
            }
          ]
          probes: [
            {
              type: 'Liveness'
              initialDelaySeconds: 15
              failureThreshold: 3
              timeoutSeconds: 15
              httpGet: {
                port: 8080
                path: '/health'
              }
            }
            {
              type: 'Startup'
              timeoutSeconds: 15
              httpGet: {
                port: 8080
                path: '/health'
              }
            }
            {
              type: 'Readiness'
              timeoutSeconds: 15
              initialDelaySeconds: 15
              httpGet: {
                port: 8080
                path: '/health'
              }
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}
resource webappEasyauth 'Microsoft.App/containerApps/authConfigs@2023-05-01' = {
  parent: webApp
  name: 'current'
  properties: {
    platform: {
      enabled: true
    }
    globalValidation: {
      unauthenticatedClientAction: 'AllowAnonymous'
    }
    identityProviders: {
      azureActiveDirectory: {
        registration: {
          openIdIssuer: 'https://login.microsoftonline.com/common/v2.0'
          clientId: oidcClientId
        }
      }
    }
  }
  dependsOn: [
    webappIdentity
    webappAcrPullRoleAssignment
    webappSignalRRoleassignment
    webappStorageRoleassignment
    appinsights_monitoring_roleassignment
  ]
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


