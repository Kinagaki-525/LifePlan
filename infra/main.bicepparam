using './main.bicep'

param location = 'japaneast'
param resourceGroupName = 'rg-lifeplan-jpe'
param resourceNamePrefix = 'lifeplan'
param customHostNames = [
  'www.futari-kakei.com'
  'futari-kakei.com'
]
