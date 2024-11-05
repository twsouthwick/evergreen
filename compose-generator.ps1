# docker cp router:/openils/conf/opensrf.xml .

$template = @"
  {{name}}:
    container_name: {{name}}
    extends:
      file: opensrf-service.yml
      service: opensrf-service
    environment:
      OPENSRF_SERVICE: {{service}}
"@

[xml]$apps=gc .\opensrf.xml
$apps.opensrf.hosts.localhost.activeapps.appname | % { $template.Replace("{{name}}", $_.Replace(".", "-")).Replace("{{service}}", $_) + "`n"} | Out-File opensrf-services-compose.yml