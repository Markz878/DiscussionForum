param location string = resourceGroup().location
param solutionName string = 'discussionforum'
param containerRegistryName string = 'acr${solutionName}'
param appinsightsName string = 'ai-${solutionName}'
param containerAppEnvironmentName string = 'cae-${solutionName}'
param storageName string = 'st${solutionName}'
param sqlServerName string = 'sql-${solutionName}'
param databaseName string = 'sqldb-${solutionName}'
param appName string = solutionName
param imageTag string
param oidcClientId string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
    name: containerRegistryName
}

// resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
//     name: appinsightsName
// }

resource containerappEnvironment 'Microsoft.App/managedEnvironments@2022-11-01-preview' existing = {
    name: containerAppEnvironmentName
}

// resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
//     name: storageName
// }

resource webappIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
    name: '${solutionName}-identity'
}

// resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' existing = {
//     name: sqlServerName
// }

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
                targetPort: 80
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
                    // env: [
                    //     {
                    //         name: 'ConnectionStrings__SqlServer'
                    //         value: 'Server=${sqlServer.properties.fullyQualifiedDomainName};Initial Catalog=${databaseName};Authentication=Active Directory Managed Identity;User Id=${webappIdentity.properties.clientId};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
                    //     }
                    //     {
                    //         name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
                    //         value: appInsights.properties.ConnectionString
                    //     }
                    //     {
                    //         name: 'FileStorageSettings__StorageUri'
                    //         value: storageAccount.properties.primaryEndpoints.blob
                    //     }
                    //     {
                    //         name: 'ManagedIdentityId'
                    //         value: webappIdentity.properties.clientId
                    //     }
                    //     {
                    //         name: 'Azure__SignalR__ConnectionString'
                    //         value: 'Endpoint=https:sigr-discussionforum.service.signalr.net;AuthType=azure.msi;ClientId=${webappIdentity.properties.clientId};Version=1.0;'
                    //     }
                    //     {
                    //         name: 'Version'
                    //         value: imageTag
                    //     }
                    // ]
                    // probes: [
                    //     {
                    //         type: 'Liveness'
                    //         initialDelaySeconds: 15
                    //         failureThreshold: 3
                    //         timeoutSeconds: 15
                    //         httpGet: {
                    //             port: 80
                    //             path: '/health'
                    //         }
                    //     }
                    //     {
                    //         type: 'Startup'
                    //         timeoutSeconds: 15
                    //         httpGet: {
                    //             port: 80
                    //             path: '/health'
                    //         }
                    //     }
                    //     {
                    //         type: 'Readiness'
                    //         timeoutSeconds: 15
                    //         initialDelaySeconds: 15
                    //         httpGet: {
                    //             port: 80
                    //             path: '/health'
                    //         }
                    //     }
                    // ]
                }
            ]
            scale: {
                minReplicas: 1
                maxReplicas: 1
                // rules: [
                //     {
                //         name: 'http-scale-rule'
                //         http: {
                //             metadata: {
                //                 concurrentRequests: '1'
                //             }
                //         }
                //     }
                // ]
            }
        }
    }
}
// resource webappEasyauth 'Microsoft.App/containerApps/authConfigs@2023-05-01' = {
//     parent: webApp
//     name: 'current'
//     properties: {
//         platform: {
//             enabled: true
//         }
//         globalValidation: {
//             unauthenticatedClientAction: 'AllowAnonymous'
//         }
//         identityProviders: {
//             azureActiveDirectory: {
//                 registration: {
//                     openIdIssuer: 'https://login.microsoftonline.com/common/v2.0'
//                     clientId: oidcClientId
//                 }
//             }
//         }
//     }
// }
