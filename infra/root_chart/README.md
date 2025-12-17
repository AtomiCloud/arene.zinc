# root-chart

![Version: 0.1.0](https://img.shields.io/badge/Version-0.1.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 1.16.0](https://img.shields.io/badge/AppVersion-1.16.0-informational?style=flat-square)

Root Chart for arene zinc

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| file://../api_chart | api(dotnet-chart) | 0.1.0 |
| file://../migration_chart | migration(dotnet-migration) | 0.1.0 |
| oci://ghcr.io/atomicloud/sulfoxide.bromine | bromine(sulfoxide-bromine) | 1.8.0 |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| api.affinity | object | `{}` |  |
| api.annotations."argocd.argoproj.io/hook" | string | `"Sync"` |  |
| api.annotations."argocd.argoproj.io/sync-wave" | string | `"4"` |  |
| api.annotations.drop_log | string | `"true"` |  |
| api.appSettings.App.Mode | string | `"Server"` |  |
| api.autoscaling | object | `{}` |  |
| api.configMountPath | string | `"/app/Config"` |  |
| api.enabled | bool | `true` |  |
| api.envFromSecret | string | `"zinc"` |  |
| api.image.pullPolicy | string | `"IfNotPresent"` |  |
| api.image.repository | string | `"arene-zinc-api"` |  |
| api.image.tag | string | `""` |  |
| api.imagePullSecrets | list | `[]` |  |
| api.ingress.className | string | `"nginx"` |  |
| api.ingress.enabled | bool | `true` |  |
| api.ingress.hosts[0].host | string | `"api.zinc.arene.lapras.lvh.me"` |  |
| api.ingress.hosts[0].paths[0].path | string | `"/"` |  |
| api.ingress.hosts[0].paths[0].pathType | string | `"ImplementationSpecific"` |  |
| api.ingress.tls[0].hosts[0] | string | `"api.zinc.arene.lapras.lvh.me"` |  |
| api.ingress.tls[0].issuerRef | string | `"sample"` |  |
| api.ingress.tls[0].secretName | string | `"sample"` |  |
| api.livenessProbe.httpGet.path | string | `"/"` |  |
| api.livenessProbe.httpGet.port | string | `"http"` |  |
| api.livenessProbe.periodSeconds | int | `30` |  |
| api.nameOverride | string | `"zinc-api"` |  |
| api.nodeSelector | object | `{}` |  |
| api.podAnnotations | object | `{}` |  |
| api.podSecurityContext | object | `{}` |  |
| api.readinessProbe.httpGet.path | string | `"/"` |  |
| api.readinessProbe.httpGet.port | string | `"http"` |  |
| api.readinessProbe.periodSeconds | int | `30` |  |
| api.replicaCount | int | `1` |  |
| api.resources.limits.cpu | string | `"1"` |  |
| api.resources.limits.memory | string | `"1Gi"` |  |
| api.resources.requests.cpu | string | `"100m"` |  |
| api.resources.requests.memory | string | `"128Mi"` |  |
| api.securityContext | object | `{}` |  |
| api.service.containerPort | int | `9000` |  |
| api.service.port | int | `80` |  |
| api.service.type | string | `"ClusterIP"` |  |
| api.serviceAccount.annotations | object | `{}` |  |
| api.serviceAccount.create | bool | `false` |  |
| api.serviceAccount.name | string | `""` |  |
| api.serviceTree.<<.landscape | string | `"lapras"` |  |
| api.serviceTree.<<.layer | string | `"2"` |  |
| api.serviceTree.<<.platform | string | `"arene"` |  |
| api.serviceTree.<<.service | string | `"zinc"` |  |
| api.serviceTree.module | string | `"api"` |  |
| api.tolerations | list | `[]` |  |
| api.topologySpreadConstraints | object | `{}` |  |
| bromine.annotations."argocd.argoproj.io/sync-wave" | string | `"1"` |  |
| bromine.rootSecret.name | string | `"zinc"` |  |
| bromine.rootSecret.ref.clientId | string | `"ARENE_ZINCCLIENT_ID"` |  |
| bromine.rootSecret.ref.clientSecret | string | `"ARENE_ZINCCLIENT_SECRET"` |  |
| bromine.serviceTree.<<.landscape | string | `"lapras"` |  |
| bromine.serviceTree.<<.layer | string | `"2"` |  |
| bromine.serviceTree.<<.platform | string | `"arene"` |  |
| bromine.serviceTree.<<.service | string | `"zinc"` |  |
| bromine.storeName | string | `"zinc"` |  |
| bromine.target | string | `"zinc"` |  |
| caches.main.enabled | bool | `true` |  |
| caches.main.ephemeral | bool | `true` |  |
| caches.main.external.host | string | `""` |  |
| caches.main.external.port | int | `6379` |  |
| caches.main.external.tls | bool | `true` |  |
| caches.main.replicas | int | `1` |  |
| caches.main.resources.limits.cpu | string | `"500m"` |  |
| caches.main.resources.limits.memory | string | `"512Mi"` |  |
| caches.main.resources.requests.cpu | string | `"100m"` |  |
| caches.main.resources.requests.memory | string | `"256Mi"` |  |
| caches.main.secretRef.keys.password | string | `"password"` |  |
| caches.main.secretRef.name | string | `"zinc-maincache-credentials"` |  |
| caches.main.type | string | `"crd"` |  |
| databases.main.backup.enabled | bool | `false` |  |
| databases.main.database | string | `"arene-zinc"` |  |
| databases.main.enabled | bool | `true` |  |
| databases.main.ephemeral | bool | `true` |  |
| databases.main.external.database | string | `"arene-zinc"` |  |
| databases.main.external.host | string | `""` |  |
| databases.main.external.port | int | `5432` |  |
| databases.main.external.sslMode | string | `"require"` |  |
| databases.main.instances | int | `1` |  |
| databases.main.secretRef.keys.connectionString | string | `"connectionString"` |  |
| databases.main.secretRef.keys.password | string | `"password"` |  |
| databases.main.secretRef.keys.username | string | `"username"` |  |
| databases.main.secretRef.name | string | `"zinc-maindb-credentials"` |  |
| databases.main.storage.size | string | `"1Gi"` |  |
| databases.main.storage.storageClass | string | `"local-path"` |  |
| databases.main.type | string | `"crd"` |  |
| migration.affinity | object | `{}` |  |
| migration.annotations."argocd.argoproj.io/hook" | string | `"Sync"` |  |
| migration.annotations."argocd.argoproj.io/sync-wave" | string | `"3"` |  |
| migration.annotations.drop_log | string | `"true"` |  |
| migration.appSettings.App.Mode | string | `"Migration"` |  |
| migration.aspNetEnv | string | `"Development"` |  |
| migration.backoffLimit | int | `4` |  |
| migration.configMountPath | string | `"/app/Config"` |  |
| migration.enabled | bool | `false` |  |
| migration.envFromSecret | string | `"zinc"` |  |
| migration.image.pullPolicy | string | `"IfNotPresent"` |  |
| migration.image.repository | string | `"arene-zinc-migration"` |  |
| migration.image.tag | string | `""` |  |
| migration.imagePullSecrets | list | `[]` |  |
| migration.nameOverride | string | `"zinc-migration"` |  |
| migration.nodeSelector | object | `{}` |  |
| migration.podAnnotations | object | `{}` |  |
| migration.podSecurityContext | object | `{}` |  |
| migration.resources.limits.cpu | string | `"500m"` |  |
| migration.resources.limits.memory | string | `"1Gi"` |  |
| migration.resources.requests.cpu | string | `"100m"` |  |
| migration.resources.requests.memory | string | `"128Mi"` |  |
| migration.securityContext | object | `{}` |  |
| migration.serviceAccount.annotations | object | `{}` |  |
| migration.serviceAccount.create | bool | `false` |  |
| migration.serviceAccount.name | string | `""` |  |
| migration.serviceTree.<<.landscape | string | `"lapras"` |  |
| migration.serviceTree.<<.layer | string | `"2"` |  |
| migration.serviceTree.<<.platform | string | `"arene"` |  |
| migration.serviceTree.<<.service | string | `"zinc"` |  |
| migration.serviceTree.module | string | `"migration"` |  |
| migration.tolerations | list | `[]` |  |
| migration.topologySpreadConstraints | object | `{}` |  |
| serviceTree.landscape | string | `"lapras"` |  |
| serviceTree.layer | string | `"2"` |  |
| serviceTree.platform | string | `"arene"` |  |
| serviceTree.service | string | `"zinc"` |  |
| storages.main.enabled | bool | `true` |  |
| storages.main.ephemeral | bool | `true` |  |
| storages.main.external.bucket | string | `""` |  |
| storages.main.external.endpoint | string | `""` |  |
| storages.main.external.region | string | `""` |  |
| storages.main.ingress.api.host | string | `"mainstorage.zinc.arene.lapras.lvh.me"` |  |
| storages.main.ingress.className | string | `"traefik"` |  |
| storages.main.ingress.console.host | string | `"console-mainstorage.zinc.arene.lapras.lvh.me"` |  |
| storages.main.ingress.enabled | bool | `true` |  |
| storages.main.pools.servers | int | `1` |  |
| storages.main.pools.volumesPerServer | int | `1` |  |
| storages.main.secretRef.keys.accessKey | string | `"accessKey"` |  |
| storages.main.secretRef.keys.secretKey | string | `"secretKey"` |  |
| storages.main.secretRef.name | string | `"zinc-mainstorage-credentials"` |  |
| storages.main.storage.size | string | `"1Gi"` |  |
| storages.main.storage.storageClass | string | `"local-path"` |  |
| storages.main.type | string | `"crd"` |  |
| tags."atomi.cloud/layer" | string | `"2"` |  |
| tags."atomi.cloud/platform" | string | `"arene"` |  |
| tags."atomi.cloud/service" | string | `"zinc"` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
