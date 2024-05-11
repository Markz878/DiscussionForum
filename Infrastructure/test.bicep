
@description('Generated from /subscriptions/afeb28da-a255-426a-8c3a-a9822266b9e4/resourceGroups/rg-DiscussionForum/providers/Microsoft.Sql/servers/sql-discussionforum')
resource sqldiscussionforum 'Microsoft.Sql/servers@2023-08-01-preview' = {
  kind: 'v12.0'
  properties: {
    administratorLogin: 'CloudSA5631a236'
    version: '12.0'
    state: 'Ready'
    fullyQualifiedDomainName: 'sql-discussionforum.database.windows.net'
    privateEndpointConnections: []
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: 'DiscussionForumSQLAdmins'
      sid: '71cd5d5a-94e5-4464-9944-86efd18c1c45'
      tenantId: '0424f4a0-f409-4185-9d9d-76ce62c20e4d'
      azureADOnlyAuthentication: true
    }
    restrictOutboundNetworkAccess: 'Enabled'
    externalGovernanceStatus: 'Disabled'
  }
  location: 'northeurope'
  name: 'sql-discussionforum'
}
